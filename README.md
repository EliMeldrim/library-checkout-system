# Library & Media Checkout System

A server-rendered **ASP.NET Core 8 Razor Pages** application for managing a small library's catalog, members, and circulation (checkouts, returns, and overdue tracking). Built deliberately as a classic server-rendered app — no SPA, no client framework — to showcase Razor Pages, EF Core, and a clean service-layer architecture.

## Features

- **Dashboard** — at-a-glance counts (items, members, active loans, overdue) plus a merged checkout/return activity feed.
- **Catalog** — searchable and filterable item list (title, author/creator, type) with live availability shown as copies available vs. owned.
- **Item detail** — full metadata, complete loan history, and a checkout form with server-side validation (no checkout when all copies are out, member must be Active, no duplicate checkout of the same item).
- **Members** — list, create, and edit members (with unique-email validation); member detail shows current loans and full history.
- **Return flow** — mark a loan returned from the item detail page, the member detail page, the loans list, or the overdue report.
- **Overdue report** — every unreturned loan past its due date, with days overdue, sortable by item, member, due date, or days overdue; overdue rows are highlighted throughout the app.
- **Seed data** — on first run the database is created and seeded with 25 items across all four media types, 8 members, returned loan history, active loans, and several overdue loans (dated relative to "today" so the demo always has overdue rows).

## Domain Model

| Entity | Key fields | Notes |
| --- | --- | --- |
| **Item** | Title, Type (Book / DVD / Audiobook / Magazine), Creator, Identifier (ISBN etc.), Year, CopiesOwned | Availability = CopiesOwned minus active loans |
| **Member** | Name, Email (unique), JoinDate, Status (Active / Suspended / Lapsed) | Only Active members may check items out |
| **Loan** | Item + Member, CheckedOutDate, DueDate (14 days by default), ReturnedDate (nullable) | A loan with no ReturnedDate is active; active and past due = overdue |

Circulation rules live in `CirculationService` (checkout validation, returns, availability, overdue calculation) behind an `ICirculationService` interface, keeping the Razor Page models thin. Enums are stored as strings so the SQLite database stays human-readable.

## Tech Stack

- ASP.NET Core 8 (Razor Pages, server-rendered)
- Entity Framework Core 8 + SQLite (`EnsureCreated` + seeding on first run)
- Bootstrap 5 with a custom theme (navbar, cards, badges, table highlighting)
- `TimeProvider` abstraction for testable date logic
- xUnit for the test suite

## Project Layout

```
src/LibraryCheckout.Web/
  Models/           Item, Member, Loan, enums
  Data/             LibraryContext, entity configurations, SeedData
  Services/         ICirculationService, CirculationService, LoanPolicy
  Pages/            Dashboard, Catalog, Members, Loans, Reports/Overdue
tests/LibraryCheckout.Tests/
  CirculationServiceTests.cs   business-rule tests on in-memory SQLite
```

## Running the App

Requires the .NET 8 SDK.

```bash
dotnet run --project src/LibraryCheckout.Web
```

Then open **http://localhost:5130**. The SQLite database (`library.db`) is created and seeded automatically on first run; delete the file to reset to fresh seed data.

## Running the Tests

```bash
dotnet test
```

The test project exercises the checkout/return/overdue business rules against EF Core running on an in-memory SQLite connection (same provider as production, so relational behavior matches), with a frozen `TimeProvider` for deterministic date assertions.

## Screenshots

<!-- screenshot: dashboard -->
<!-- screenshot: catalog -->
<!-- screenshot: item-detail -->
<!-- screenshot: member-detail -->
<!-- screenshot: overdue-report -->
