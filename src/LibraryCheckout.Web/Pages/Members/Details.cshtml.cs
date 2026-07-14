using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using LibraryCheckout.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Members;

public class DetailsModel : PageModel
{
    private readonly LibraryContext _db;
    private readonly ICirculationService _circulation;
    private readonly TimeProvider _time;

    public DetailsModel(LibraryContext db, ICirculationService circulation, TimeProvider time)
    {
        _db = db;
        _circulation = circulation;
        _time = time;
    }

    public Member Member { get; private set; } = null!;
    public IReadOnlyList<Loan> CurrentLoans { get; private set; } = Array.Empty<Loan>();
    public IReadOnlyList<Loan> PastLoans { get; private set; } = Array.Empty<Loan>();
    public DateTime Today => _time.GetLocalNow().Date;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadAsync(id) ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostReturnAsync(int id, int loanId)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        var result = await _circulation.ReturnAsync(loanId);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return Page();
        }

        StatusMessage = $"Returned \"{result.Loan!.Item.Title}\".";
        return RedirectToPage(new { id });
    }

    private async Task<bool> LoadAsync(int id)
    {
        var member = await _db.Members
            .AsNoTracking()
            .Include(m => m.Loans)
                .ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (member is null)
        {
            return false;
        }

        Member = member;
        CurrentLoans = member.Loans
            .Where(l => l.IsActive)
            .OrderBy(l => l.DueDate)
            .ToList();
        PastLoans = member.Loans
            .Where(l => !l.IsActive)
            .OrderByDescending(l => l.ReturnedDate)
            .ToList();

        return true;
    }
}
