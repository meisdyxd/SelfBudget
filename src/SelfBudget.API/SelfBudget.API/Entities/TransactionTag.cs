namespace SelfBudget.API.Entities;

public class TransactionTag
{
    public TransactionTag(
        Guid tagId,
        Guid transactionId)
    {
        TagId = tagId;
        TransactionId = transactionId;
        AssignedAt = DateTime.UtcNow;
    }

    public Guid TagId { get; set; }
    public virtual Tag Tag { get; set; }

    public Guid TransactionId { get; set; }
    public virtual Transaction Transaction { get; set; }

    public DateTime AssignedAt { get; set; }
}
