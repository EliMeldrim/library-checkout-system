using System.ComponentModel.DataAnnotations;
using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using LibraryCheckout.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Catalog;

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

    public Item Item { get; private set; } = null!;
    public int CopiesAvailable { get; private set; }
    public IReadOnlyList<Loan> Loans { get; private set; } = Array.Empty<Loan>();
    public SelectList MemberOptions { get; private set; } = null!;
    public DateTime Today => _time.GetLocalNow().Date;

    [BindProperty]
    [Required(ErrorMessage = "Choose a member to check this item out to.")]
    public int? SelectedMemberId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        return await LoadAsync(id) ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostCheckoutAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _circulation.CheckoutAsync(id, SelectedMemberId!.Value);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return Page();
        }

        StatusMessage = $"Checked out \"{Item.Title}\" — due {result.Loan!.DueDate:MMM d, yyyy}.";
        return RedirectToPage(new { id });
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
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Loans)
                .ThenInclude(l => l.Member)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item is null)
        {
            return false;
        }

        Item = item;
        Loans = item.Loans
            .OrderByDescending(l => l.CheckedOutDate)
            .ThenByDescending(l => l.Id)
            .ToList();
        CopiesAvailable = await _circulation.GetAvailableCopiesAsync(id);

        var activeMembers = await _db.Members
            .AsNoTracking()
            .Where(m => m.Status == MemberStatus.Active)
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, m.Name })
            .ToListAsync();
        MemberOptions = new SelectList(activeMembers, "Id", "Name");

        return true;
    }
}
