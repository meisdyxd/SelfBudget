using CSharpFunctionalExtensions;
using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Common;
using SelfBudget.API.Dtos;
using SelfBudget.API.Dtos.Responses;
using SelfBudget.API.Entities;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace SelfBudget.API.UseCases.TransferBetweenAccounts;

public class TransferBetweenAccountsHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionCategoryRepository _transactionCategoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionManager _transactionManager;

    public TransferBetweenAccountsHandler(
        IAccountRepository accountrepository,
        ITransactionCategoryRepository transactionCategoryRepository,
        ITransactionRepository transactionRepository,
        ITransactionManager transactionManager)
    {
        _accountRepository = accountrepository;
        _transactionCategoryRepository = transactionCategoryRepository;
        _transactionRepository = transactionRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<TransferResponse, Error>> Handle(
        TransferBetweenAccountsCommand command,
        CancellationToken cancellationToken)
    {
        var validationResult = ValidateCommand(command);
        if (validationResult.IsFailure)
            return validationResult.Error;

        await _transactionManager.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        try
        {
            var sourceAccount = await _accountRepository.GetByIdAsync(command.FromAccountId, cancellationToken);
            var destinationAccount = await _accountRepository.GetByIdAsync(command.ToAccountId, cancellationToken);
            var validationResultAccounts = ValidateAccounts(
                sourceAccount,
                destinationAccount,
                command.Amount);

            if (validationResultAccounts.IsFailure)
                return validationResultAccounts.Error;

            var transferCategoryId = await _transactionCategoryRepository.GetTransferIdAsync(cancellationToken);
            if (transferCategoryId == null)
                return new Error("Категория перевода не найдена");

            sourceAccount!.Balance -= command.Amount;
            destinationAccount!.Balance += command.Amount;

            var transaction = new Transaction(
                command.Amount,
                sourceAccount.Id,
                destinationAccount.Id,
                transferCategoryId.Value,
                command.Note);

            await _accountRepository.UpdateAsync(sourceAccount, cancellationToken);
            await _accountRepository.UpdateAsync(destinationAccount, cancellationToken);

            await _transactionRepository.CreateTransactionAsync(transaction, cancellationToken);
            var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveResult.IsFailure)
            {
                return saveResult.Error;
            }

            await _transactionManager.CommitAsync(cancellationToken);
            return new TransferResponse
            {
                Amount = command.Amount,
                CreatedAt = transaction.CreatedAt ?? DateTime.UtcNow,
                FromAccountId = command.FromAccountId,
                ToAccountId = command.ToAccountId,
                Id = transaction.Id,
                Note = command.Note,
                Status = "success"
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackAsync(cancellationToken);
            return new Error("Ошибка при выполнении перевода: " + ex.Message);
        }
    }

    private static Result<bool, Error> ValidateCommand(TransferBetweenAccountsCommand command)
    {
        if (command.Amount <= 1)
            return new Error("Сумма должна быть больше 1");

        if ((command.Amount * 100) % 1 != 0)
            return new Error("Сумма должна иметь не более 2 знаков после запятой");

        return true;
    }

    private static Result<bool, Error> ValidateAccounts(
        AccountDto? sourceAccount,
        AccountDto? destinationAccount,
        decimal amount)
    {
        if (sourceAccount == null)
            return new Error("Источник счет не найден");

        if (destinationAccount == null)
            return new Error("Получатель счет не найден");

        if (sourceAccount.CurrencyCode != destinationAccount.CurrencyCode)
            return new Error("Счета должны иметь одну и ту же валюту");

        if ((sourceAccount.Balance + sourceAccount.OverdraftLimit) < amount)
            return new Error("Недостаточно средств на источнике счета");

        return true;
    }
}
