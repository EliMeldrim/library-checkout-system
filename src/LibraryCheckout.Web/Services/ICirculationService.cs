namespace LibraryCheckout.Web.Services;

public interface ICirculationService
{
    /// <summary>
    /// Checks an item out to a member, enforcing availability and membership rules.
    /// The due date is set <see cref="LoanPolicy.LoanPeriodDays"/> days from today.
    /// </summary>
    Task<CirculationResult> CheckoutAsync(int itemId, int memberId, CancellationToken ct = default);

    /// <summary>Marks an active loan as returned today.</summary>
    Task<CirculationResult> ReturnAsync(int loanId, CancellationToken ct = default);

    /// <summary>Copies of the item not currently out on loan.</summary>
    Task<int> GetAvailableCopiesAsync(int itemId, CancellationToken ct = default);

    /// <summary>All unreturned loans past their due date, most overdue first.</summary>
    Task<IReadOnlyList<OverdueLoan>> GetOverdueLoansAsync(CancellationToken ct = default);
}
