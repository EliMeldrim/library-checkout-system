using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Members;

public class CreateModel : PageModel
{
    private readonly LibraryContext _db;
    private readonly TimeProvider _time;

    public CreateModel(LibraryContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    [BindProperty]
    public MemberInput Input { get; set; } = new();

    public void OnGet()
    {
        Input.JoinDate = _time.GetLocalNow().Date;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        if (await _db.Members.AnyAsync(m => m.Email == email))
        {
            ModelState.AddModelError("Input.Email", "A member with this email already exists.");
            return Page();
        }

        var member = new Member
        {
            Name = Input.Name.Trim(),
            Email = email,
            JoinDate = Input.JoinDate!.Value.Date,
            Status = Input.Status
        };

        _db.Members.Add(member);
        await _db.SaveChangesAsync();

        return RedirectToPage("./Details", new { id = member.Id });
    }
}
