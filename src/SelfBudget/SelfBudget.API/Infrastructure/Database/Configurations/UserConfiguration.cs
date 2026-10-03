using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SelfBudget.API.Domain.Entities.UserContext;
using SelfBudget.API.Domain.ValueObjects;
using SelfBudget.API.Infrastructure.Extensions;

namespace SelfBudget.API.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasKey(u => u.Id)
            .HasName("pk_users_id");

        builder.Property(u => u.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        var converter = new ValueConverter<EmailValueObject, string>(toDb => toDb.Value, fromDb => EmailValueObject.Create(fromDb).Value);

        builder.Property(u => u.Email)
            .HasConversion(converter)
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
            .HasColumnType("date")
            .IsRequired();

        // AuditableEntity
        builder.ConfigureAuditableEntity();

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