using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages;

public class IndexModel : PageModel
{
    private readonly LibraryContext _db;
    private readonly ICirculationService _circulation;

    public IndexModel(LibraryContext db, ICirculationService circulation)
    {
        _db = db;
        _circulation = circulation;
    }

    public int ItemCount { get; private set; }
    public int MemberCount { get; private set; }
    public int ActiveLoanCount { get; private set; }
    public int OverdueCount { get; private set; }
    public IReadOnlyList<ActivityEntry> RecentActivity { get; private set; } = Array.Empty<ActivityEntry>();

    public record ActivityEntry(DateTime Date, string Kind, string ItemTitle, int ItemId, string MemberName, int MemberId);

    public async Task OnGetAsync()
    {
        ItemCount = await _db.Items.CountAsync();
        MemberCount = await _db.Members.CountAsync();
        ActiveLoanCount = await _db.Loans.CountAsync(l => l.ReturnedDate == null);
        OverdueCount = (await _circulation.GetOverdueLoansAsync()).Count;

        // Build a merged checkout/return activity feed from recent loans.
        var recentLoans = await _db.Loans
            .AsNoTracking()
            .Include(l => l.Item)
            .Include(l => l.Member)
            .OrderByDescending(l => l.ReturnedDate ?? l.CheckedOutDate)
            .Take(20)
            .ToListAsync();

        RecentActivity = recentLoans
            .SelectMany(l =>
            {
                var events = new List<ActivityEntry>
                {
                    new(l.CheckedOutDate, "Checked out", l.Item.Title, l.ItemId, l.Member.Name, l.MemberId)
                };
                if (l.ReturnedDate is not null)
                {
                    events.Add(new(l.ReturnedDate.Value, "Returned", l.Item.Title, l.ItemId, l.Member.Name, l.MemberId));
                }
                return events;
            })
            .OrderByDescending(e => e.Date)
            .Take(10)
            .ToList();
    }
}
