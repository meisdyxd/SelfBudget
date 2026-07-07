using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities.TransactionContext;
using SelfBudget.API.Extensions;

namespace SelfBudget.API.Database.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id)
            .HasName("pk_transactions_id");

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.FromAccountId)
            .HasColumnName("from_account_id")
            .IsRequired();

        builder.Property(t => t.ToAccountId)
            .HasColumnName("to_account_id")
            .IsRequired();

        builder.Property(t => t.TransactionCategoryId)
            .HasColumnName("transaction_category_id")
            .IsRequired();

        builder.Property(t => t.Note)
            .HasColumnName("note")
            .HasMaxLength(1000);

        // AuditableEntity
        builder.ConfigureAuditableEntity();

        // Связи
        builder.HasOne(t => t.FromAccount)
            .WithMany(a => a.OutTransactions)
            .HasForeignKey(t => t.FromAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToAccount)
            .WithMany(a => a.InTransactions)
            .HasForeignKey(t => t.ToAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.TransactionCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы для внешних ключей
        builder.HasIndex(t => t.FromAccountId);
        builder.HasIndex(t => t.ToAccountId);
        builder.HasIndex(t => t.TransactionCategoryId);
    }
}