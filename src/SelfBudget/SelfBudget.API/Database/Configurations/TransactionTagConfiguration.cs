using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities.TransactionContext;

namespace SelfBudget.API.Database.Configurations;

public class TransactionTagConfiguration : IEntityTypeConfiguration<TransactionTag>
{
    public void Configure(EntityTypeBuilder<TransactionTag> builder)
    {
        builder.ToTable("transaction_tags");

        builder.HasKey(tt => new { tt.TagId, tt.TransactionId })
            .HasName("pk_transaction_tags");

        builder.Property(tt => tt.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        // Связи
        builder.HasOne(tt => tt.Tag)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tt => tt.Transaction)
            .WithMany(t => t.TransactionTags)
            .HasForeignKey(tt => tt.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}