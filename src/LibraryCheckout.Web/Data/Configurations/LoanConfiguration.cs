using LibraryCheckout.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryCheckout.Web.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");

        builder.HasOne(l => l.Item)
            .WithMany(i => i.Loans)
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Speeds up the "active loans for this item" availability check.
        builder.HasIndex(l => new { l.ItemId, l.ReturnedDate });
        builder.HasIndex(l => new { l.MemberId, l.ReturnedDate });
        builder.HasIndex(l => l.DueDate);
    }
}
