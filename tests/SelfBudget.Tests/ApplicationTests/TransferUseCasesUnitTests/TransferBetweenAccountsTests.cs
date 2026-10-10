using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.UseCases.TransferUseCases.TransferBetweenAccounts;
using SelfBudget.API.Common;
using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Domain.Entities.TransactionContext;
using System.Data;

namespace SelfBudget.Tests.ApplicationTests.TransferUseCasesUnitTests;

public class TransferBetweenAccountsTests
{
    [Fact]
    public async Task TransferBetweenDifferentUserAccounts_ReturnsCreatedTransferResponse()
    {
        // arrange
        var accountTypeId = Guid.NewGuid();
        var userIdSource = Guid.NewGuid();
        var userIdDestination = Guid.NewGuid();
        var sourceBalance = 100m;
        var destinationBalance = 0m;
        var transferAmount = 50m;
        var expectedStatus = "success";
        var expectedAccountSource = new Account(userIdSource, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var expectedAccountDestination = new Account(userIdDestination, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository.Setup(action => action.GetByIdAsync(expectedAccountSource.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccountSource);

        accountRepository.Setup(action => action.GetByIdAsync(expectedAccountDestination.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccountDestination);

        var expectedTransactionCategoryId = Guid.NewGuid();
        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);
        transactionCategoryRepository.Setup(action => action.GetTransferIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransactionCategoryId);
        
        var transactionRepository = new Mock<ITransactionRepository>();
        Transaction? capturedTransaction = null;

        transactionRepository.Setup(action => action.CreateTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((transaction, _) => capturedTransaction = transaction)
            .ReturnsAsync((Transaction transaction, CancellationToken _) => transaction.Id);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager
            .Setup(action => action.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int, Error>(1));

        transactionManager
            .Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager
            .Setup(action => action.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = expectedAccountSource.Id,
            ToAccountId = expectedAccountDestination.Id,
            TransactionCategoryId = expectedTransactionCategoryId,
            Note = null
        };

        var validator = new TransferBetweenAccountsValidation();

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsSuccess);

        Assert.NotNull(capturedTransaction);

        Assert.Equal(transferAmount, capturedTransaction.Amount);
        Assert.Equal(expectedAccountSource.Id, capturedTransaction.FromAccountId);
        Assert.Equal(expectedAccountDestination.Id, capturedTransaction.ToAccountId);
        Assert.Equal(expectedTransactionCategoryId, capturedTransaction.TransactionCategoryId);

        var value = result.Value;
        Assert.Equal(value.Id, capturedTransaction.Id);
        Assert.Equal(value.Amount, capturedTransaction.Amount);
        Assert.Equal(value.FromAccountId, capturedTransaction.FromAccountId);
        Assert.Equal(value.ToAccountId, capturedTransaction.ToAccountId);
        Assert.Equal(value.CreatedAt, capturedTransaction.CreatedAt);
        Assert.Equal(value.Note, capturedTransaction.Note);
        Assert.Equal(value.Status, expectedStatus);

        Assert.Equal(expectedAccountSource.Balance, sourceBalance - transferAmount);
        Assert.Equal(expectedAccountDestination.Balance, destinationBalance + transferAmount);

        transactionManager.Verify(
            tm => tm.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        
        transactionManager.Verify(
            tm => tm.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        transactionManager.Verify(
            tm => tm.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferBetweenSelfUserAccounts_ReturnsCreatedTransferResponse()
    {
        // arrange
        var accountTypeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sourceBalance = 100m;
        var destinationBalance = 0m;
        var transferAmount = 50m;
        var expectedStatus = "success";
        var expectedAccountSource = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var expectedAccountDestination = new Account(userId, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository.Setup(action => action.GetByIdAsync(expectedAccountSource.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccountSource);

        accountRepository.Setup(action => action.GetByIdAsync(expectedAccountDestination.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccountDestination);

        var expectedTransactionCategoryId = Guid.NewGuid();
        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);
        transactionCategoryRepository.Setup(action => action.GetTransactionCategoryByCode(TransactionCategoriesCodes.SelfTransfer, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransactionCategoryId);

        var transactionRepository = new Mock<ITransactionRepository>();
        Transaction? capturedTransaction = null;

        transactionRepository.Setup(action => action.CreateTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((transaction, _) => capturedTransaction = transaction)
            .ReturnsAsync((Transaction transaction, CancellationToken _) => transaction.Id);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager
            .Setup(action => action.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int, Error>(1));

        transactionManager
            .Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager
            .Setup(action => action.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = expectedAccountSource.Id,
            ToAccountId = expectedAccountDestination.Id,
            TransactionCategoryId = expectedTransactionCategoryId,
            Note = null
        };

        var validator = new TransferBetweenAccountsValidation();

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsSuccess);

        Assert.NotNull(capturedTransaction);

        Assert.Equal(transferAmount, capturedTransaction.Amount);
        Assert.Equal(expectedAccountSource.Id, capturedTransaction.FromAccountId);
        Assert.Equal(expectedAccountDestination.Id, capturedTransaction.ToAccountId);
        Assert.Equal(expectedTransactionCategoryId, capturedTransaction.TransactionCategoryId);

        var value = result.Value;
        Assert.Equal(value.Id, capturedTransaction.Id);
        Assert.Equal(value.Amount, capturedTransaction.Amount);
        Assert.Equal(value.FromAccountId, capturedTransaction.FromAccountId);
        Assert.Equal(value.ToAccountId, capturedTransaction.ToAccountId);
        Assert.Equal(value.CreatedAt, capturedTransaction.CreatedAt);
        Assert.Equal(value.Note, capturedTransaction.Note);
        Assert.Equal(value.Status, expectedStatus);

        Assert.Equal(expectedAccountSource.Balance, sourceBalance - transferAmount);
        Assert.Equal(expectedAccountDestination.Balance, destinationBalance + transferAmount);

        transactionManager.Verify(
            tm => tm.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);

        transactionManager.Verify(
            tm => tm.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

        transactionManager.Verify(
            tm => tm.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task TransferBetweenAccounts_ThrowErrorInvalidAmountLesserThanOrEqual(decimal transferAmount)
    {
        // arrange

        var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);

        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);

        var transactionRepository = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

        var validator = new TransferBetweenAccountsValidation();

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = Guid.NewGuid(),
            Note = null,
            ToAccountId = Guid.NewGuid(),
            TransactionCategoryId = Guid.NewGuid()
        };

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object, 
            transactionCategoryRepository.Object, 
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        Assert.Equal("Сумма должна быть больше 1", result.Error.Message);
    }

    [Fact]
    public async Task TransferBetweenAccounts_ThrowErrorInvalidAmountMoreThanTwoSignsAfterComma()
    {
        // arrange

        var transferAmount = 1.001m;

        var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);

        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);

        var transactionRepository = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);

        var validator = new TransferBetweenAccountsValidation();

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = Guid.NewGuid(),
            Note = null,
            ToAccountId = Guid.NewGuid(),
            TransactionCategoryId = Guid.NewGuid()
        };

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        Assert.Equal("Сумма должна иметь не более 2 знаков после запятой", result.Error.Message);
    }

    [Fact]
    public async Task TransferBetweenAccounts_ThrowErrorSourceAccountNotFound()
    {
        // arrange
        var transferAmount = 50;
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
        accountRepository.Setup(action => action.GetByIdAsync(sourceAccountId, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Account?>(null));
        accountRepository.Setup(action => action.GetByIdAsync(destinationAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<Account>());

        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);

        var transactionRepository = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager.Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager.Setup(action => action.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var validator = new TransferBetweenAccountsValidation();

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = sourceAccountId,
            Note = null,
            ToAccountId = destinationAccountId,
            TransactionCategoryId = Guid.NewGuid()
        };

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        transactionManager.Verify(tm => tm.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        transactionManager.Verify(tm => tm.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferBetweenAccounts_ThrowErrorDestinationAccountNotFound()
    {
        // arrange
        var transferAmount = 50;
        var sourceAccountId = Guid.NewGuid();
        var destinationAccountId = Guid.NewGuid();
        var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
        accountRepository.Setup(action => action.GetByIdAsync(sourceAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(It.IsAny<Account>());
        accountRepository.Setup(action => action.GetByIdAsync(destinationAccountId, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Account?>(null));

        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);

        var transactionRepository = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager.Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager.Setup(action => action.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var validator = new TransferBetweenAccountsValidation();

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = sourceAccountId,
            Note = null,
            ToAccountId = destinationAccountId,
            TransactionCategoryId = Guid.NewGuid()
        };

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        transactionManager.Verify(tm => tm.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        transactionManager.Verify(tm => tm.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferBetweenAccounts_ThrowErrorDifferentCurrencies()
    {
        // arrange
        var transferAmount = 25;
        var userId = Guid.NewGuid();
        var accountTypeId = Guid.NewGuid();
        var sourceBalance = 50m;
        var destinationBalance = 0m;
        var sourceAccount = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var destinationAccount = new Account(userId, "destination", "USD", accountTypeId)
        {
            Balance = destinationBalance
        };

        var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
        accountRepository.Setup(action => action.GetByIdAsync(sourceAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        accountRepository.Setup(action => action.GetByIdAsync(destinationAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);
        transactionCategoryRepository.Setup(action => action.GetTransactionCategoryByCode(
                TransactionCategoriesCodes.SelfTransfer,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var transactionRepository = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager.Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager.Setup(action => action.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var validator = new TransferBetweenAccountsValidation();

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = sourceAccount.Id,
            Note = null,
            ToAccountId = destinationAccount.Id,
            TransactionCategoryId = Guid.NewGuid()
        };

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        Assert.Equal("Счета должны иметь одну и ту же валюту", result.Error.Message);
        Assert.Equal(sourceBalance, sourceAccount.Balance);
        Assert.Equal(destinationBalance, destinationAccount.Balance);
        transactionManager.Verify(tm => tm.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        transactionManager.Verify(tm => tm.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferBetweenAccounts_ThrowErrorTransactionCategoryNotFound()
    {
        // arrange
        var transferAmount = 25;
        var userId = Guid.NewGuid();
        var accountTypeId = Guid.NewGuid();
        var sourceBalance = 50m;
        var destinationBalance = 0m;
        var sourceAccount = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var destinationAccount = new Account(userId, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        var accountRepository = new Mock<IAccountRepository>(MockBehavior.Strict);
        accountRepository.Setup(action => action.GetByIdAsync(sourceAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        accountRepository.Setup(action => action.GetByIdAsync(destinationAccount.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);

        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);
        transactionCategoryRepository.Setup(action => action.GetTransactionCategoryByCode(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<Guid?>(null));

        var transactionRepository = new Mock<ITransactionRepository>(MockBehavior.Strict);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager.Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager.Setup(action => action.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var validator = new TransferBetweenAccountsValidation();

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = sourceAccount.Id,
            Note = null,
            ToAccountId = destinationAccount.Id,
            TransactionCategoryId = Guid.NewGuid()
        };

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        Assert.Equal(sourceBalance, sourceAccount.Balance);
        Assert.Equal(destinationBalance, destinationAccount.Balance);
        transactionManager.Verify(tm => tm.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        transactionManager.Verify(tm => tm.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TransferBetweenAccounts_ThrowErrorLowBalance()
    {
        // arrange
        var accountTypeId = Guid.NewGuid();
        var userIdSource = Guid.NewGuid();
        var userIdDestination = Guid.NewGuid();
        var sourceBalance = 100m;
        var destinationBalance = 0m;
        var transferAmount = 200m;
        var expectedAccountSource = new Account(userIdSource, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var expectedAccountDestination = new Account(userIdDestination, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        var accountRepository = new Mock<IAccountRepository>();
        accountRepository.Setup(action => action.GetByIdAsync(expectedAccountSource.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccountSource);

        accountRepository.Setup(action => action.GetByIdAsync(expectedAccountDestination.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAccountDestination);

        var expectedTransactionCategoryId = Guid.NewGuid();
        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>(MockBehavior.Strict);
        transactionCategoryRepository.Setup(action => action.GetTransferIdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTransactionCategoryId);

        var transactionRepository = new Mock<ITransactionRepository>();
        Transaction? capturedTransaction = null;

        transactionRepository.Setup(action => action.CreateTransactionAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .Callback<Transaction, CancellationToken>((transaction, _) => capturedTransaction = transaction)
            .ReturnsAsync((Transaction transaction, CancellationToken _) => transaction.Id);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager
            .Setup(action => action.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        transactionManager
            .Setup(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = expectedAccountSource.Id,
            ToAccountId = expectedAccountDestination.Id,
            TransactionCategoryId = expectedTransactionCategoryId,
            Note = null
        };

        var validator = new TransferBetweenAccountsValidation();

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object,
            validator);

        // act

        var result = await sut.Handle(command, CancellationToken.None);

        // assert

        Assert.True(result.IsFailure);
        Assert.Equal("Недостаточно средств на источнике счета", result.Error.Message);
        Assert.Equal(sourceBalance, expectedAccountSource.Balance);
        Assert.Equal(destinationBalance, expectedAccountDestination.Balance);
        transactionManager.Verify(action => action.BeginTransactionAsync(IsolationLevel.ReadCommitted, It.IsAny<CancellationToken>()), Times.Once);
        transactionManager.Verify(action => action.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
