using System.ComponentModel.DataAnnotations;

namespace LibraryCheckout.Web.Models;

public class Loan
{
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int MemberId { get; set; }
    public Member Member { get; set; } = null!;

    [DataType(DataType.Date)]
    [Display(Name = "Checked Out")]
    public DateTime CheckedOutDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Due")]
    public DateTime DueDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Returned")]
    public DateTime? ReturnedDate { get; set; }

    public bool IsActive => ReturnedDate is null;

    public bool IsOverdueAsOf(DateTime asOf) => IsActive && DueDate.Date < asOf.Date;

    public int DaysOverdueAsOf(DateTime asOf) =>
        IsOverdueAsOf(asOf) ? (asOf.Date - DueDate.Date).Days : 0;
}
