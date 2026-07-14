using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using LibraryCheckout.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Loans;

public class IndexModel : PageModel
{
    private readonly LibraryContext _db;
    private readonly ICirculationService _circulation;
    private readonly TimeProvider _time;

    public IndexModel(LibraryContext db, ICirculationService circulation, TimeProvider time)
    {
        _db = db;
        _circulation = circulation;
        _time = time;
    }

    [BindProperty(SupportsGet = true)]
    public string Filter { get; set; } = "active";

    public IReadOnlyList<Loan> Loans { get; private set; } = Array.Empty<Loan>();
    public DateTime Today => _time.GetLocalNow().Date;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostReturnAsync(int loanId)
    {
        var result = await _circulation.ReturnAsync(loanId);
        if (!result.Succeeded)
        {
            await LoadAsync();
            ModelState.AddModelError(string.Empty, result.Error!);
            return Page();
        }

        StatusMessage = $"Returned \"{result.Loan!.Item.Title}\".";
        return RedirectToPage(new { filter = Filter });
    }

    private async Task LoadAsync()
    {
        var today = Today;
        var query = _db.Loans
            .AsNoTracking()
            .Include(l => l.Item)
            .Include(l => l.Member)
            .AsQueryable();

        query = Filter switch
        {
            "overdue" => query.Where(l => l.ReturnedDate == null && l.DueDate < today),
            "returned" => query.Where(l => l.ReturnedDate != null),
            "all" => query,
            _ => query.Where(l => l.ReturnedDate == null)
        };

        Loans = await query
            .OrderByDescending(l => l.CheckedOutDate)
            .ThenByDescending(l => l.Id)
            .ToListAsync();
    }
}
