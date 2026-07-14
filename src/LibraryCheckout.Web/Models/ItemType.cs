using System.ComponentModel.DataAnnotations;

namespace LibraryCheckout.Web.Models;

public enum ItemType
{
    Book,

    [Display(Name = "DVD")]
    Dvd,

    Audiobook,

    Magazine
}
