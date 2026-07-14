using System.ComponentModel.DataAnnotations;

namespace LibraryCheckout.Web.Models;

public class Member
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Join Date")]
    public DateTime JoinDate { get; set; }

    public MemberStatus Status { get; set; } = MemberStatus.Active;

    public List<Loan> Loans { get; set; } = new();
}
