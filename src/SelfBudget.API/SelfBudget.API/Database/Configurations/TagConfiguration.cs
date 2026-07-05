using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SelfBudget.API.Entities;
using SelfBudget.API.Extensions;

namespace SelfBudget.API.Database.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");

        builder.HasKey(t => t.Id)
            .HasName("pk_tags_id");

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        // AuditableEntity
        builder.ConfigureAuditableEntity();

        // Уникальность имени тега (глобально)
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}