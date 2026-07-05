namespace SelfBudget.API.Dtos;

public sealed record AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OverdraftLimit { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid TypeId { get; set; }
}
