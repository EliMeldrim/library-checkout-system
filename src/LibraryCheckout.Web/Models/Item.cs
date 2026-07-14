using System.ComponentModel.DataAnnotations;

namespace LibraryCheckout.Web.Models;

public class Item
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Type")]
    public ItemType Type { get; set; }

    [Required, StringLength(150)]
    [Display(Name = "Author / Creator")]
    public string Creator { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "ISBN / Identifier")]
    public string? Identifier { get; set; }

    [Range(1400, 2100)]
    public int Year { get; set; }

    [Range(0, 1000)]
    [Display(Name = "Copies Owned")]
    public int CopiesOwned { get; set; }

    public List<Loan> Loans { get; set; } = new();
}
