using LibraryCheckout.Web.Models;
using LibraryCheckout.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryCheckout.Web.Data;

/// <summary>
/// Populates a freshly created database with demo data so every page has
/// something to show: a mixed catalog, members in each status, returned
/// history, active loans, and several overdue loans (dates are relative
/// to "today" so the overdue report always demos well).
/// </summary>
public static class SeedData
{
    public static async Task EnsureSeededAsync(LibraryContext db, TimeProvider time)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Items.AnyAsync())
        {
            return;
        }

        var today = time.GetLocalNow().Date;

        var items = new List<Item>
        {
            // Books
            new() { Title = "The Name of the Wind", Type = ItemType.Book, Creator = "Patrick Rothfuss", Identifier = "978-0756404741", Year = 2007, CopiesOwned = 3 },
            new() { Title = "Project Hail Mary", Type = ItemType.Book, Creator = "Andy Weir", Identifier = "978-0593135204", Year = 2021, CopiesOwned = 4 },
            new() { Title = "Educated", Type = ItemType.Book, Creator = "Tara Westover", Identifier = "978-0399590504", Year = 2018, CopiesOwned = 2 },
            new() { Title = "The Pragmatic Programmer", Type = ItemType.Book, Creator = "David Thomas, Andrew Hunt", Identifier = "978-0135957059", Year = 2019, CopiesOwned = 2 },
            new() { Title = "A Gentleman in Moscow", Type = ItemType.Book, Creator = "Amor Towles", Identifier = "978-0670026197", Year = 2016, CopiesOwned = 2 },
            new() { Title = "The Left Hand of Darkness", Type = ItemType.Book, Creator = "Ursula K. Le Guin", Identifier = "978-0441478125", Year = 1969, CopiesOwned = 1 },
            new() { Title = "Kitchen Confidential", Type = ItemType.Book, Creator = "Anthony Bourdain", Identifier = "978-0060899226", Year = 2000, CopiesOwned = 2 },
            new() { Title = "Braiding Sweetgrass", Type = ItemType.Book, Creator = "Robin Wall Kimmerer", Identifier = "978-1571313560", Year = 2013, CopiesOwned = 3 },
            new() { Title = "The Thursday Murder Club", Type = ItemType.Book, Creator = "Richard Osman", Identifier = "978-1984880963", Year = 2020, CopiesOwned = 3 },
            new() { Title = "Snow Crash", Type = ItemType.Book, Creator = "Neal Stephenson", Identifier = "978-0553380958", Year = 1992, CopiesOwned = 1 },

            // DVDs
            new() { Title = "Spirited Away", Type = ItemType.Dvd, Creator = "Hayao Miyazaki", Identifier = "DVD-000214", Year = 2001, CopiesOwned = 2 },
            new() { Title = "The Grand Budapest Hotel", Type = ItemType.Dvd, Creator = "Wes Anderson", Identifier = "DVD-000377", Year = 2014, CopiesOwned = 2 },
            new() { Title = "Planet Earth II", Type = ItemType.Dvd, Creator = "BBC / David Attenborough", Identifier = "DVD-000412", Year = 2016, CopiesOwned = 3 },
            new() { Title = "Arrival", Type = ItemType.Dvd, Creator = "Denis Villeneuve", Identifier = "DVD-000538", Year = 2016, CopiesOwned = 1 },
            new() { Title = "Paddington 2", Type = ItemType.Dvd, Creator = "Paul King", Identifier = "DVD-000601", Year = 2017, CopiesOwned = 2 },
            new() { Title = "Everything Everywhere All at Once", Type = ItemType.Dvd, Creator = "Daniels", Identifier = "DVD-000689", Year = 2022, CopiesOwned = 2 },

            // Audiobooks
            new() { Title = "Born a Crime", Type = ItemType.Audiobook, Creator = "Trevor Noah", Identifier = "AUD-001102", Year = 2016, CopiesOwned = 2 },
            new() { Title = "The Martian", Type = ItemType.Audiobook, Creator = "Andy Weir, read by R.C. Bray", Identifier = "AUD-001177", Year = 2013, CopiesOwned = 2 },
            new() { Title = "Becoming", Type = ItemType.Audiobook, Creator = "Michelle Obama", Identifier = "AUD-001248", Year = 2018, CopiesOwned = 1 },
            new() { Title = "Dune", Type = ItemType.Audiobook, Creator = "Frank Herbert, full cast", Identifier = "AUD-001305", Year = 1965, CopiesOwned = 2 },
            new() { Title = "Atomic Habits", Type = ItemType.Audiobook, Creator = "James Clear", Identifier = "AUD-001366", Year = 2018, CopiesOwned = 3 },

            // Magazines
            new() { Title = "National Geographic — July Issue", Type = ItemType.Magazine, Creator = "National Geographic Society", Identifier = "MAG-2026-07-NG", Year = 2026, CopiesOwned = 4 },
            new() { Title = "The Atlantic — Summer Issue", Type = ItemType.Magazine, Creator = "The Atlantic Monthly Group", Identifier = "MAG-2026-06-ATL", Year = 2026, CopiesOwned = 3 },
            new() { Title = "Wired — June Issue", Type = ItemType.Magazine, Creator = "Condé Nast", Identifier = "MAG-2026-06-WRD", Year = 2026, CopiesOwned = 2 },
            new() { Title = "Cook's Illustrated — Spring Issue", Type = ItemType.Magazine, Creator = "America's Test Kitchen", Identifier = "MAG-2026-04-CI", Year = 2026, CopiesOwned = 2 },
        };

        var members = new List<Member>
        {
            new() { Name = "Amara Okafor", Email = "amara.okafor@example.com", JoinDate = today.AddYears(-3).AddDays(-40), Status = MemberStatus.Active },
            new() { Name = "Ben Tran", Email = "ben.tran@example.com", JoinDate = today.AddYears(-2).AddDays(-12), Status = MemberStatus.Active },
            new() { Name = "Carmen Delgado", Email = "carmen.delgado@example.com", JoinDate = today.AddYears(-1).AddDays(-200), Status = MemberStatus.Active },
            new() { Name = "Dmitri Volkov", Email = "dmitri.volkov@example.com", JoinDate = today.AddYears(-1).AddDays(-95), Status = MemberStatus.Active },
            new() { Name = "Elsie Whitehorse", Email = "elsie.whitehorse@example.com", JoinDate = today.AddMonths(-10), Status = MemberStatus.Active },
            new() { Name = "Farid Haddad", Email = "farid.haddad@example.com", JoinDate = today.AddMonths(-7), Status = MemberStatus.Suspended },
            new() { Name = "Grace Kim", Email = "grace.kim@example.com", JoinDate = today.AddMonths(-4), Status = MemberStatus.Active },
            new() { Name = "Henry Abelard", Email = "henry.abelard@example.com", JoinDate = today.AddYears(-4), Status = MemberStatus.Lapsed },
        };

        db.Items.AddRange(items);
        db.Members.AddRange(members);
        await db.SaveChangesAsync();

        Loan Returned(Item item, Member member, int checkedOutDaysAgo, int loanLengthDays) => new()
        {
            ItemId = item.Id,
            MemberId = member.Id,
            CheckedOutDate = today.AddDays(-checkedOutDaysAgo),
            DueDate = today.AddDays(-checkedOutDaysAgo + LoanPolicy.LoanPeriodDays),
            ReturnedDate = today.AddDays(-checkedOutDaysAgo + loanLengthDays)
        };

        Loan Active(Item item, Member member, int checkedOutDaysAgo) => new()
        {
            ItemId = item.Id,
            MemberId = member.Id,
            CheckedOutDate = today.AddDays(-checkedOutDaysAgo),
            DueDate = today.AddDays(-checkedOutDaysAgo + LoanPolicy.LoanPeriodDays)
        };

        var loans = new List<Loan>
        {
            // Returned history
            Returned(items[0], members[0], 60, 12),   // Name of the Wind -> Amara
            Returned(items[1], members[1], 55, 9),    // Project Hail Mary -> Ben
            Returned(items[10], members[2], 48, 6),   // Spirited Away -> Carmen
            Returned(items[16], members[3], 45, 14),  // Born a Crime -> Dmitri
            Returned(items[2], members[4], 40, 17),   // Educated -> Elsie (was late)
            Returned(items[21], members[6], 33, 4),   // Nat Geo -> Grace
            Returned(items[12], members[0], 30, 11),  // Planet Earth II -> Amara
            Returned(items[7], members[2], 26, 13),   // Braiding Sweetgrass -> Carmen
            Returned(items[19], members[5], 70, 10),  // Dune -> Farid (before suspension)
            Returned(items[8], members[1], 22, 15),   // Thursday Murder Club -> Ben (was late)

            // Overdue (checked out more than 14 days ago, never returned)
            Active(items[5], members[3], 35),   // Left Hand of Darkness -> Dmitri, 21 days overdue
            Active(items[13], members[4], 28),  // Arrival -> Elsie, 14 days overdue
            Active(items[9], members[1], 22),   // Snow Crash -> Ben, 8 days overdue
            Active(items[18], members[6], 17),  // Becoming -> Grace, 3 days overdue

            // Active, not yet due
            Active(items[1], members[0], 10),   // Project Hail Mary -> Amara
            Active(items[3], members[2], 6),    // Pragmatic Programmer -> Carmen
            Active(items[20], members[4], 4),   // Atomic Habits -> Elsie
            Active(items[11], members[6], 2),   // Grand Budapest Hotel -> Grace
            Active(items[22], members[0], 1),   // The Atlantic -> Amara
        };

        db.Loans.AddRange(loans);
        await db.SaveChangesAsync();
    }
}
