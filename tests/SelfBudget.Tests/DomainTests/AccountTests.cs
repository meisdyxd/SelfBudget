using SelfBudget.API.Domain.Entities.AccountContext;

namespace SelfBudget.Tests.DomainTests;

public class AccountTests
{
    [Fact]
    public void TransferTo_ChangedTwoBalances()
    {
        // arrange
        var userId = Guid.NewGuid();
        var accountTypeId = Guid.NewGuid();
        var sourceBalance = 150m;
        var destinationBalance = 50m;
        var transferAmount = 100m;
        var expectedSourceBalance = sourceBalance - transferAmount;
        var expectedDestinationBalance = destinationBalance + transferAmount;
        var sourceAccount = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var destinationAccount = new Account(userId, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        // act
        var result = sourceAccount.TransferTo(destinationAccount, transferAmount);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(sourceAccount.Balance, expectedSourceBalance, 2);
        Assert.Equal(destinationAccount.Balance, expectedDestinationBalance, 2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(0.5)]
    public void TransferTo_CancelledTransfer_InvalidTransferAmount(decimal transferAmount)
    {
        // arrange
        var userId = Guid.NewGuid();
        var accountTypeId = Guid.NewGuid();
        var sourceBalance = 150m;
        var destinationBalance = 50m;
        var sourceAccount = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var destinationAccount = new Account(userId, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        // act
        var result = sourceAccount.TransferTo(destinationAccount, transferAmount);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Code);
        Assert.Contains("amount", result.Error.Code);
    }

    [Fact]
    public void TransferTo_CancelledTransfer_SourceBalanceLessThanTransferAmountWithZeroOverdraft()
    {
        // arrange
        var userId = Guid.NewGuid();
        var accountTypeId = Guid.NewGuid();
        var sourceBalance = 150m;
        var destinationBalance = 50m;
        var transferAmount = sourceBalance + 1m;
        var sourceAccount = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance
        };
        var destinationAccount = new Account(userId, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        // act
        var result = sourceAccount.TransferTo(destinationAccount, transferAmount);

        // assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.NotNull(result.Error.Code);
        Assert.Contains("overdraft", result.Error.Code);
    }

    [Fact]
    public void TransferTo_ChangedTwoBalance_WithOverdraft()
    {
        // arrange
        var userId = Guid.NewGuid();
        var accountTypeId = Guid.NewGuid();
        var sourceBalance = 150m;
        var sourceOverdraft = 50m;
        var destinationBalance = 50m;
        var transferAmount = sourceBalance + 25m;
        var expectedSourceBalance = sourceBalance - transferAmount;
        var expectedDestinationBalance = destinationBalance + transferAmount;
        var sourceAccount = new Account(userId, "source", "RUB", accountTypeId)
        {
            Balance = sourceBalance,
            OverdraftLimit = sourceOverdraft
        };
        var destinationAccount = new Account(userId, "destination", "RUB", accountTypeId)
        {
            Balance = destinationBalance
        };

        // act
        var result = sourceAccount.TransferTo(destinationAccount, transferAmount);

        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(sourceAccount.Balance, expectedSourceBalance, 2);
        Assert.Equal(destinationAccount.Balance, expectedDestinationBalance, 2);
    }
}
