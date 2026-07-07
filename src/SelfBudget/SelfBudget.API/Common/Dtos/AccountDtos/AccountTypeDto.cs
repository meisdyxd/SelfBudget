namespace SelfBudget.API.Common.Dtos.AccountDtos;

public class AccountTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
