namespace SelfBudget.API.Entities.TransactionContext;

public class TransactionTag
{
    protected TransactionTag() { }

    public TransactionTag(
        Guid tagId,
        Guid transactionId)
    {
        TagId = tagId;
        TransactionId = transactionId;
        AssignedAt = DateTime.UtcNow;
    }

    public Guid TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;

    public Guid TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
}
