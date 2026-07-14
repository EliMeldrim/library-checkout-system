using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Models;
using LibraryCheckout.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Tests;

/// <summary>
/// Business-rule tests for checkout, return, availability, and the overdue
/// report, running against EF Core on an in-memory SQLite database so the
/// relational behavior (constraints, translations) matches production.
/// </summary>
public sealed class CirculationServiceTests : IDisposable
{
    private static readonly DateTimeOffset BaseNow =
        new(2026, 3, 2, 10, 30, 0, TimeSpan.Zero);

    private readonly SqliteConnection _connection;
    private readonly LibraryContext _db;
    private readonly FixedTimeProvider _time;
    private readonly CirculationService _service;

    public CirculationServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new LibraryContext(options);
        _db.Database.EnsureCreated();

        _time = new FixedTimeProvider(BaseNow);
        _service = new CirculationService(_db, _time);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private DateTime Today => _time.GetLocalNow().Date;

    private async Task<Item> AddItemAsync(int copiesOwned = 1, string title = "Test Item")
    {
        var item = new Item
        {
            Title = title,
            Type = ItemType.Book,
            Creator = "Test Author",
            Year = 2020,
            CopiesOwned = copiesOwned
        };
        _db.Items.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    private async Task<Member> AddMemberAsync(
        MemberStatus status = MemberStatus.Active, string name = "Test Member")
    {
        var member = new Member
        {
            Name = name,
            Email = $"{Guid.NewGuid():N}@example.com",
            JoinDate = Today.AddYears(-1),
            Status = status
        };
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return member;
    }

    // ---- Checkout ----

    [Fact]
    public async Task Checkout_Succeeds_And_SetsDueDate14DaysOut()
    {
        var item = await AddItemAsync();
        var member = await AddMemberAsync();

        var result = await _service.CheckoutAsync(item.Id, member.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Loan);
        Assert.Equal(Today, result.Loan!.CheckedOutDate);
        Assert.Equal(Today.AddDays(14), result.Loan.DueDate);
        Assert.Null(result.Loan.ReturnedDate);
        Assert.Equal(1, await _db.Loans.CountAsync());
    }

    [Fact]
    public async Task Checkout_Fails_WhenNoCopiesAvailable()
    {
        var item = await AddItemAsync(copiesOwned: 1);
        var first = await AddMemberAsync(name: "First");
        var second = await AddMemberAsync(name: "Second");

        Assert.True((await _service.CheckoutAsync(item.Id, first.Id)).Succeeded);

        var result = await _service.CheckoutAsync(item.Id, second.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("No copies", result.Error);
        Assert.Equal(1, await _db.Loans.CountAsync());
    }

    [Fact]
    public async Task Checkout_Succeeds_AfterACopyIsReturned()
    {
        var item = await AddItemAsync(copiesOwned: 1);
        var first = await AddMemberAsync(name: "First");
        var second = await AddMemberAsync(name: "Second");

        var loan = (await _service.CheckoutAsync(item.Id, first.Id)).Loan!;
        Assert.True((await _service.ReturnAsync(loan.Id)).Succeeded);

        var result = await _service.CheckoutAsync(item.Id, second.Id);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Checkout_CountsOnlyActiveLoans_AgainstAvailability()
    {
        var item = await AddItemAsync(copiesOwned: 2);
        var a = await AddMemberAsync(name: "A");
        var b = await AddMemberAsync(name: "B");
        var c = await AddMemberAsync(name: "C");

        var loanA = (await _service.CheckoutAsync(item.Id, a.Id)).Loan!;
        Assert.True((await _service.CheckoutAsync(item.Id, b.Id)).Succeeded);
        Assert.False((await _service.CheckoutAsync(item.Id, c.Id)).Succeeded);

        await _service.ReturnAsync(loanA.Id);

        Assert.True((await _service.CheckoutAsync(item.Id, c.Id)).Succeeded);
    }

    [Theory]
    [InlineData(MemberStatus.Suspended)]
    [InlineData(MemberStatus.Lapsed)]
    public async Task Checkout_Fails_ForNonActiveMember(MemberStatus status)
    {
        var item = await AddItemAsync();
        var member = await AddMemberAsync(status: status);

        var result = await _service.CheckoutAsync(item.Id, member.Id);

        Assert.False(result.Succeeded);
        Assert.Contains(status.ToString(), result.Error);
        Assert.Equal(0, await _db.Loans.CountAsync());
    }

    [Fact]
    public async Task Checkout_Fails_WhenMemberAlreadyHasTheItem()
    {
        var item = await AddItemAsync(copiesOwned: 3);
        var member = await AddMemberAsync();

        Assert.True((await _service.CheckoutAsync(item.Id, member.Id)).Succeeded);

        var result = await _service.CheckoutAsync(item.Id, member.Id);

        Assert.False(result.Succeeded);
        Assert.Contains("already has", result.Error);
    }

    [Fact]
    public async Task Checkout_Fails_ForUnknownItemOrMember()
    {
        var item = await AddItemAsync();
        var member = await AddMemberAsync();

        var missingItem = await _service.CheckoutAsync(itemId: 9999, memberId: member.Id);
        var missingMember = await _service.CheckoutAsync(itemId: item.Id, memberId: 9999);

        Assert.False(missingItem.Succeeded);
        Assert.Equal("Item not found.", missingItem.Error);
        Assert.False(missingMember.Succeeded);
        Assert.Equal("Member not found.", missingMember.Error);
    }

    // ---- Return ----

    [Fact]
    public async Task Return_SetsReturnedDate_ToToday()
    {
        var item = await AddItemAsync();
        var member = await AddMemberAsync();
        var loan = (await _service.CheckoutAsync(item.Id, member.Id)).Loan!;

        _time.Advance(TimeSpan.FromDays(5));

        var result = await _service.ReturnAsync(loan.Id);

        Assert.True(result.Succeeded);
        Assert.Equal(Today, result.Loan!.ReturnedDate);
    }

    [Fact]
    public async Task Return_Fails_WhenAlreadyReturned()
    {
        var item = await AddItemAsync();
        var member = await AddMemberAsync();
        var loan = (await _service.CheckoutAsync(item.Id, member.Id)).Loan!;

        Assert.True((await _service.ReturnAsync(loan.Id)).Succeeded);

        var second = await _service.ReturnAsync(loan.Id);

        Assert.False(second.Succeeded);
        Assert.Contains("already returned", second.Error);
    }

    [Fact]
    public async Task Return_Fails_ForUnknownLoan()
    {
        var result = await _service.ReturnAsync(loanId: 424242);

        Assert.False(result.Succeeded);
        Assert.Equal("Loan not found.", result.Error);
    }

    // ---- Availability ----

    [Fact]
    public async Task AvailableCopies_ReflectsActiveLoansOnly()
    {
        var item = await AddItemAsync(copiesOwned: 3);
        var a = await AddMemberAsync(name: "A");
        var b = await AddMemberAsync(name: "B");

        Assert.Equal(3, await _service.GetAvailableCopiesAsync(item.Id));

        var loanA = (await _service.CheckoutAsync(item.Id, a.Id)).Loan!;
        await _service.CheckoutAsync(item.Id, b.Id);
        Assert.Equal(1, await _service.GetAvailableCopiesAsync(item.Id));

        await _service.ReturnAsync(loanA.Id);
        Assert.Equal(2, await _service.GetAvailableCopiesAsync(item.Id));
    }

    [Fact]
    public async Task AvailableCopies_IsZero_ForUnknownItem()
    {
        Assert.Equal(0, await _service.GetAvailableCopiesAsync(9999));
    }

    // ---- Overdue ----

    [Fact]
    public async Task Overdue_ExcludesLoansDueTodayOrLater_AndReturnedLoans()
    {
        var item = await AddItemAsync(copiesOwned: 4);
        var onTime = await AddMemberAsync(name: "On Time");
        var dueToday = await AddMemberAsync(name: "Due Today");
        var late = await AddMemberAsync(name: "Late");
        var returnedLate = await AddMemberAsync(name: "Returned Late");

        // Checked out 20 days before "now": due 6 days ago.
        var lateLoan = (await _service.CheckoutAsync(item.Id, late.Id)).Loan!;
        var returnedLoan = (await _service.CheckoutAsync(item.Id, returnedLate.Id)).Loan!;

        _time.Advance(TimeSpan.FromDays(6));

        // Checked out 14 days before "now": due exactly today (not overdue).
        await _service.CheckoutAsync(item.Id, dueToday.Id);

        _time.Advance(TimeSpan.FromDays(14));

        // Fresh checkout: due in 14 days.
        await _service.CheckoutAsync(item.Id, onTime.Id);

        // A late loan that was returned should not appear.
        await _service.ReturnAsync(returnedLoan.Id);

        var overdue = await _service.GetOverdueLoansAsync();

        var entry = Assert.Single(overdue);
        Assert.Equal(lateLoan.Id, entry.Loan.Id);
        Assert.Equal("Late", entry.Loan.Member.Name);
    }

    [Fact]
    public async Task Overdue_ComputesDaysOverdue_AndSortsMostOverdueFirst()
    {
        var item = await AddItemAsync(copiesOwned: 3);
        var first = await AddMemberAsync(name: "First");
        var second = await AddMemberAsync(name: "Second");

        var oldest = (await _service.CheckoutAsync(item.Id, first.Id)).Loan!;
        _time.Advance(TimeSpan.FromDays(10));
        var newer = (await _service.CheckoutAsync(item.Id, second.Id)).Loan!;

        // Now 30 days past the first checkout: 16 and 6 days overdue.
        _time.Advance(TimeSpan.FromDays(20));

        var overdue = await _service.GetOverdueLoansAsync();

        Assert.Equal(2, overdue.Count);
        Assert.Equal(oldest.Id, overdue[0].Loan.Id);
        Assert.Equal(16, overdue[0].DaysOverdue);
        Assert.Equal(newer.Id, overdue[1].Loan.Id);
        Assert.Equal(6, overdue[1].DaysOverdue);
    }

    [Fact]
    public async Task Overdue_IsEmpty_WhenNothingIsLate()
    {
        var item = await AddItemAsync();
        var member = await AddMemberAsync();
        await _service.CheckoutAsync(item.Id, member.Id);

        var overdue = await _service.GetOverdueLoansAsync();

        Assert.Empty(overdue);
    }
}
