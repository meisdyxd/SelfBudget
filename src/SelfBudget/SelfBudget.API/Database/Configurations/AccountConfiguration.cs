using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities.AccountContext;
using SelfBudget.API.Extensions;

namespace SelfBudget.API.Database.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id)
            .HasName("pk_accounts_id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(a => a.TypeId)
            .HasColumnName("type_id")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.CurrencyCode)
            .HasColumnName("currency_code")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(a => a.Balance)
            .HasColumnName("balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(a => a.OverdraftLimit)
            .HasColumnName("overdraft_limit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        // AuditableEntity
        builder.ConfigureAuditableEntity();

        // Связи
        builder.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Type)
            .WithMany(t => t.Accounts)
            .HasForeignKey(a => a.TypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.TypeId);
    }
}
