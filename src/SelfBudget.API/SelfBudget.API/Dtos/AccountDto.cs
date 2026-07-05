namespace SelfBudget.API.Dtos;

public sealed record AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string CurrencyCode { get; set; }
    public decimal Balance { get; set; }
    public decimal OverdraftLimit { get; set; }
    public string Type { get; set; }
    public Guid TypeId { get; set; }
}
