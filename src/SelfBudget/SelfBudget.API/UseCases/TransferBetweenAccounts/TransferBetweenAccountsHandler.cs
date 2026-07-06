using SelfBudget.API.Abstractions;
using SelfBudget.API.Abstractions.Repositories;
using SelfBudget.API.Entities;
using System.Data;

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

    public async Task<bool> Handler(
        TransferBetweenAccountsCommand command,
        CancellationToken cancellationToken)
    {
        await _transactionManager.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        var sourceAccount = await _accountRepository.GetByIdAsync(command.FromAccountId, cancellationToken)
            ?? throw new InvalidOperationException("Source account not found");
        var destinationAccount = await _accountRepository.GetByIdAsync(command.ToAccountId, cancellationToken)
            ?? throw new InvalidOperationException("Destination account not found");

        if ((sourceAccount.Balance + sourceAccount.OverdraftLimit) < command.Amount)
            throw new InvalidOperationException("Insufficient funds");

        var transferCategoryId = await _transactionCategoryRepository.GetTransferIdAsync(cancellationToken)
            ?? throw new InvalidOperationException("Идентификатор категория перевода не найден");

        sourceAccount.Balance -= command.Amount;
        destinationAccount.Balance += command.Amount;

        var transaction = new Transaction(
            command.Amount,
            sourceAccount.Id,
            destinationAccount.Id,
            transferCategoryId,
            command.Note);

        await _transactionRepository.CreateTransactionAsync(transaction, cancellationToken);
        await _transactionManager.SaveChangesAsync(cancellationToken);
        await _transactionManager.CommitAsync(cancellationToken);
        return true;
    }
}
