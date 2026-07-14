using System.ComponentModel.DataAnnotations;
using LibraryCheckout.Web.Models;

namespace LibraryCheckout.Web.Pages.Members;

/// <summary>Form model shared by the Create and Edit member pages.</summary>
public class MemberInput
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Join Date")]
    public DateTime? JoinDate { get; set; }

    [Required]
    public MemberStatus Status { get; set; } = MemberStatus.Active;
}
