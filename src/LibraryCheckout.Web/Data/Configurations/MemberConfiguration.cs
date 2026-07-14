using LibraryCheckout.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryCheckout.Web.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(m => m.Email)
            .IsUnique();
    }
}
