namespace SelfBudget.API.Dtos.Responses;

public class TransferResponse
{
    public Guid Id { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
}
