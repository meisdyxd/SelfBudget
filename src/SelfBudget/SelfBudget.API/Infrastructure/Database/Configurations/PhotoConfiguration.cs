using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Infrastructure.Extensions;

namespace SelfBudget.API.Infrastructure.Database.Configurations;

public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("photos");

        builder.HasKey(p => p.Id)
            .HasName("pk_photos_id");

        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(p => p.Uri)
            .HasColumnName("uri")
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(p => p.Size)
            .HasColumnName("size")
            .IsRequired();

        // AuditableEntity
        builder.ConfigureAuditableEntity();

        // Связи
        builder.HasOne(p => p.User)
            .WithOne(u => u.Photo)
            .HasForeignKey<Photo>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}