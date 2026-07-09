using CSharpFunctionalExtensions;
using Moq;
using SelfBudget.API.Application.Abstractions;
using SelfBudget.API.Application.Abstractions.Repositories;
using SelfBudget.API.Application.UseCases.AccountUseCases.CreateAccount;
using SelfBudget.API.Common;
using SelfBudget.API.Common.Dtos.Requests.AccountRequests;
using SelfBudget.API.Domain.Entities.AccountContext;

namespace SelfBudget.Tests.ApplicationTests.AccountUseCasesUnitTests;

public class CreateAccountTests
{
    [Fact]
    public async Task CreateAccount_ReturnsCreatedAccount()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        Account? capturedAccount = null;

        var repository = new Mock<IAccountRepository>();
        repository.Setup(action => action.CreateAccountAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Callback<Account, CancellationToken>((account, _) => capturedAccount = account)
            .ReturnsAsync(expectedId);

        var transactionManager = new Mock<ITransactionManager>(MockBehavior.Strict);
        transactionManager
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<int, Error>(1));

        var sut = new CreateAccountHandler(repository.Object, transactionManager.Object);

        var accountTypeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new CreateAccountRequest
        {
            Name = "name",
            AccountTypeId = accountTypeId,
            CurrencyCode = "USD",
            UserId = userId,
        };

        // Act

        var result = await sut.Handle(CreateAccountCommand.FromRequest(request), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value, expectedId);

        Assert.NotNull(capturedAccount);
        Assert.Equal(userId, capturedAccount.UserId);
        Assert.Equal(accountTypeId, capturedAccount.TypeId);
        Assert.Equal("name", capturedAccount.Name);
        Assert.Equal("USD", capturedAccount.CurrencyCode);

        repository.Verify(x => x.CreateAccountAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
        transactionManager.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        repository.VerifyNoOtherCalls();
        transactionManager.VerifyNoOtherCalls();
    }
}
