using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities.UserContext;
using SelfBudget.API.Extensions;

namespace SelfBudget.API.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id)
            .HasName("pk_users_id");

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PhotoId)
            .HasColumnName("photo_id");

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(u => u.Birthdate)
            .HasColumnName("birthdate")
            .IsRequired();

        // AuditableEntity
        builder.ConfigureAuditableEntity();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasOne(u => u.Photo)
            .WithOne(p => p.User)
            .HasForeignKey<Photo>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Accounts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}