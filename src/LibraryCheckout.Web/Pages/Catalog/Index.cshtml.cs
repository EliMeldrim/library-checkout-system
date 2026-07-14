using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Pages.Catalog;

public class IndexModel : PageModel
{
    private readonly LibraryContext _db;

    public IndexModel(LibraryContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public ItemType? Type { get; set; }

    public IReadOnlyList<CatalogRow> Items { get; private set; } = Array.Empty<CatalogRow>();

    public record CatalogRow(
        int Id, string Title, ItemType Type, string Creator, int Year,
        int CopiesOwned, int CopiesAvailable);

    public async Task OnGetAsync()
    {
        var query = _db.Items.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var pattern = $"%{Search.Trim()}%";
            query = query.Where(i =>
                EF.Functions.Like(i.Title, pattern) ||
                EF.Functions.Like(i.Creator, pattern));
        }

        if (Type.HasValue)
        {
            var type = Type.Value;
            query = query.Where(i => i.Type == type);
        }

        Items = await query
            .OrderBy(i => i.Title)
            .Select(i => new CatalogRow(
                i.Id, i.Title, i.Type, i.Creator, i.Year,
                i.CopiesOwned,
                i.CopiesOwned - i.Loans.Count(l => l.ReturnedDate == null)))
            .ToListAsync();
    }
}
