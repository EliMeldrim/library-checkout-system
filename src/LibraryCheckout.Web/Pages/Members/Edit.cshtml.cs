using LibraryCheckout.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Members;

public class EditModel : PageModel
{
    private readonly LibraryContext _db;

    public EditModel(LibraryContext db)
    {
        _db = db;
    }

    [BindProperty]
    public MemberInput Input { get; set; } = new();

    public string MemberName { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var member = await _db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (member is null)
        {
            return NotFound();
        }

        MemberName = member.Name;
        Input = new MemberInput
        {
            Name = member.Name,
            Email = member.Email,
            JoinDate = member.JoinDate,
            Status = member.Status
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (member is null)
        {
            return NotFound();
        }

        MemberName = member.Name;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        if (await _db.Members.AnyAsync(m => m.Email == email && m.Id != id))
        {
            ModelState.AddModelError("Input.Email", "Another member already uses this email.");
            return Page();
        }

        member.Name = Input.Name.Trim();
        member.Email = email;
        member.JoinDate = Input.JoinDate!.Value.Date;
        member.Status = Input.Status;

        await _db.SaveChangesAsync();

        return RedirectToPage("./Details", new { id });
    }
}
