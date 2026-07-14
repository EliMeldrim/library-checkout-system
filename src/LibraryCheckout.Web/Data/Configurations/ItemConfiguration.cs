using LibraryCheckout.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryCheckout.Web.Data.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Creator)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Identifier)
            .HasMaxLength(40);

        // Store the enum as text so the database stays human-readable.
        builder.Property(i => i.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(i => i.Title);
        builder.HasIndex(i => i.Type);
    }
}
