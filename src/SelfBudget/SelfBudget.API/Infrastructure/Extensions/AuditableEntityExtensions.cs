using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Infrastructure.Abstractions;

namespace SelfBudget.API.Infrastructure.Extensions;

public static class AuditableEntityExtensions
{
    public static void ConfigureAuditableEntity<T>(this EntityTypeBuilder<T> modelBuilder)
        where T: AuditableEntity
    {
        modelBuilder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired(false);

        modelBuilder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        modelBuilder.Property(u => u.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(255)
            .IsRequired(false);

        modelBuilder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(255)
            .IsRequired(false);
    }
}
