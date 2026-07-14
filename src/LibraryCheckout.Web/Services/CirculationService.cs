using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Services;

public class CirculationService : ICirculationService
{
    private readonly LibraryContext _db;
    private readonly TimeProvider _time;

    public CirculationService(LibraryContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    private DateTime Today => _time.GetLocalNow().Date;

    public async Task<CirculationResult> CheckoutAsync(int itemId, int memberId, CancellationToken ct = default)
    {
        var item = await _db.Items.FindAsync(new object[] { itemId }, ct);
        if (item is null)
        {
            return CirculationResult.Fail("Item not found.");
        }

        var member = await _db.Members.FindAsync(new object[] { memberId }, ct);
        if (member is null)
        {
            return CirculationResult.Fail("Member not found.");
        }

        if (member.Status != MemberStatus.Active)
        {
            return CirculationResult.Fail(
                $"{member.Name} cannot check out items: membership status is {member.Status}.");
        }

        var activeLoanCount = await _db.Loans
            .CountAsync(l => l.ItemId == itemId && l.ReturnedDate == null, ct);
        if (activeLoanCount >= item.CopiesOwned)
        {
            return CirculationResult.Fail(
                $"No copies of \"{item.Title}\" are available: all {item.CopiesOwned} are checked out.");
        }

        var alreadyBorrowed = await _db.Loans.AnyAsync(
            l => l.ItemId == itemId && l.MemberId == memberId && l.ReturnedDate == null, ct);
        if (alreadyBorrowed)
        {
            return CirculationResult.Fail(
                $"{member.Name} already has \"{item.Title}\" checked out.");
        }

        var loan = new Loan
        {
            ItemId = itemId,
            MemberId = memberId,
            CheckedOutDate = Today,
            DueDate = Today.AddDays(LoanPolicy.LoanPeriodDays)
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync(ct);

        return CirculationResult.Ok(loan);
    }

    public async Task<CirculationResult> ReturnAsync(int loanId, CancellationToken ct = default)
    {
        var loan = await _db.Loans
            .Include(l => l.Item)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == loanId, ct);

        if (loan is null)
        {
            return CirculationResult.Fail("Loan not found.");
        }

        if (loan.ReturnedDate is not null)
        {
            return CirculationResult.Fail(
                $"\"{loan.Item.Title}\" was already returned on {loan.ReturnedDate:yyyy-MM-dd}.");
        }

        loan.ReturnedDate = Today;
        await _db.SaveChangesAsync(ct);

        return CirculationResult.Ok(loan);
    }

    public async Task<int> GetAvailableCopiesAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.Items.FindAsync(new object[] { itemId }, ct);
        if (item is null)
        {
            return 0;
        }

        var activeLoanCount = await _db.Loans
            .CountAsync(l => l.ItemId == itemId && l.ReturnedDate == null, ct);

        return Math.Max(0, item.CopiesOwned - activeLoanCount);
    }

    public async Task<IReadOnlyList<OverdueLoan>> GetOverdueLoansAsync(CancellationToken ct = default)
    {
        var today = Today;

        var loans = await _db.Loans
            .AsNoTracking()
            .Include(l => l.Item)
            .Include(l => l.Member)
            .Where(l => l.ReturnedDate == null && l.DueDate < today)
            .OrderBy(l => l.DueDate)
            .ToListAsync(ct);

        return loans
            .Select(l => new OverdueLoan(l, (today - l.DueDate.Date).Days))
            .ToList();
    }
}
