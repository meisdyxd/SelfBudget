using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities;
using SelfBudget.API.Extensions;

namespace SelfBudget.API.Database.Configurations;

public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
{
    public void Configure(EntityTypeBuilder<AccountType> builder)
    {
        builder.ToTable("account_types");

        builder.HasKey(t => t.Id)
            .HasName("pk_account_types_id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        // AuditableEntity
        builder.ConfigureAuditableEntity();

        // Связь
        builder.HasMany(t => t.Accounts)
            .WithOne(a => a.Type)
            .HasForeignKey(a => a.TypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}