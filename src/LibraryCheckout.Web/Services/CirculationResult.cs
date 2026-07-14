using LibraryCheckout.Web.Models;

namespace LibraryCheckout.Web.Services;

/// <summary>Outcome of a checkout or return operation.</summary>
public record CirculationResult(bool Succeeded, string? Error = null, Loan? Loan = null)
{
    public static CirculationResult Ok(Loan loan) => new(true, null, loan);
    public static CirculationResult Fail(string error) => new(false, error);
}

/// <summary>An unreturned loan that is past its due date.</summary>
public record OverdueLoan(Loan Loan, int DaysOverdue);
