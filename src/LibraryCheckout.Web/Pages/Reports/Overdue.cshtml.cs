using LibraryCheckout.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LibraryCheckout.Web.Pages.Reports;

public class OverdueModel : PageModel
{
    private readonly ICirculationService _circulation;

    public OverdueModel(ICirculationService circulation)
    {
        _circulation = circulation;
    }

    [BindProperty(SupportsGet = true)]
    public string Sort { get; set; } = "days";

    [BindProperty(SupportsGet = true)]
    public string Dir { get; set; } = "desc";

    public IReadOnlyList<OverdueLoan> Overdue { get; private set; } = Array.Empty<OverdueLoan>();

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
        return RedirectToPage(new { sort = Sort, dir = Dir });
    }

    public string NextDir(string column) =>
        Sort == column && Dir == "asc" ? "desc" : "asc";

    public string SortIndicator(string column) =>
        Sort != column ? string.Empty : Dir == "asc" ? " ▲" : " ▼";

    private async Task LoadAsync()
    {
        var overdue = await _circulation.GetOverdueLoansAsync();

        IOrderedEnumerable<OverdueLoan> sorted = Sort switch
        {
            "title" => overdue.OrderBy(o => o.Loan.Item.Title, StringComparer.OrdinalIgnoreCase),
            "member" => overdue.OrderBy(o => o.Loan.Member.Name, StringComparer.OrdinalIgnoreCase),
            "due" => overdue.OrderBy(o => o.Loan.DueDate),
            _ => overdue.OrderBy(o => o.DaysOverdue)
        };

        Overdue = (Dir == "desc" ? sorted.Reverse() : sorted).ToList();
    }
}
