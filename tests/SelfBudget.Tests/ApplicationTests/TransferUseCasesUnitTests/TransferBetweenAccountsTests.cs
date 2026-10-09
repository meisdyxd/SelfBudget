using CSharpFunctionalExtensions;
using Moq;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.UseCases.TransferUseCases.TransferBetweenAccounts;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.Responses.TransferResponses;
using SelfBudget.API.Domain.Entities.AccountContext;
using SelfBudget.API.Domain.Entities.TransactionContext;
using System.Data;

namespace SelfBudget.Tests.ApplicationTests.TransferUseCasesUnitTests;

public class TransferBetweenAccountsTests
{
    [Fact]
    public async Task TransferBetweenAccounts_ReturnsCreatedTransferResponse()
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
        var transactionCategoryRepository = new Mock<ITransactionCategoryRepository>();
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

        var sut = new TransferBetweenAccountsHandler(
            accountRepository.Object,
            transactionCategoryRepository.Object,
            transactionRepository.Object,
            transactionManager.Object);

        var command = new TransferBetweenAccountsCommand()
        {
            Amount = transferAmount,
            FromAccountId = expectedAccountSource.Id,
            ToAccountId = expectedAccountDestination.Id,
            TransactionCategoryId = expectedTransactionCategoryId,
            Note = null
        };

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
}
