using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities;

namespace SelfBudget.API.Database.Configurations;

public class TransactionCategoryTypeConfiguration : IEntityTypeConfiguration<TransactionCategoryType>
{
    public void Configure(EntityTypeBuilder<TransactionCategoryType> builder)
    {
        builder.ToTable("transaction_category_types");

        builder.HasKey(t => t.Id)
            .HasName("pk_transaction_category_types_id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        // Связь
        builder.HasMany(t => t.Categories)
            .WithOne(c => c.Type)
            .HasForeignKey(c => c.TransactionCategoryTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}