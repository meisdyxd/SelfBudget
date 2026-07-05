using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Database.Configurations;

public class TransactionCategoryConfiguration : IEntityTypeConfiguration<TransactionCategory>
{
    public void Configure(EntityTypeBuilder<TransactionCategory> builder)
    {
        builder.ToTable("transaction_categories");

        builder.HasKey(c => c.Id)
            .HasName("pk_transaction_categories_id");

        builder.Property(c => c.BaseTransactionCategoryId)
            .HasColumnName("base_transaction_category_id");

        builder.Property(c => c.TransactionCategoryTypeId)
            .HasColumnName("transaction_category_type_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        // Связи
        builder.HasOne(c => c.BaseTransactionCategory)
            .WithMany(c => c.Childrens)
            .HasForeignKey(c => c.BaseTransactionCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Type)
            .WithMany(t => t.Categories)
            .HasForeignKey(c => c.TransactionCategoryTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы
        builder.HasIndex(c => c.BaseTransactionCategoryId);
        builder.HasIndex(c => c.TransactionCategoryTypeId);
    }
}