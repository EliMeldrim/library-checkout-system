using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Members;

public class IndexModel : PageModel
{
    private readonly LibraryContext _db;

    public IndexModel(LibraryContext db)
    {
        _db = db;
    }

    public IReadOnlyList<MemberRow> Members { get; private set; } = Array.Empty<MemberRow>();

    public record MemberRow(
        int Id, string Name, string Email, DateTime JoinDate,
        MemberStatus Status, int ActiveLoanCount);

    public async Task OnGetAsync()
    {
        Members = await _db.Members
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .Select(m => new MemberRow(
                m.Id, m.Name, m.Email, m.JoinDate, m.Status,
                m.Loans.Count(l => l.ReturnedDate == null)))
            .ToListAsync();
    }
}
