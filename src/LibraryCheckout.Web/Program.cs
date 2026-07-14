using LibraryCheckout.Web.Data;
using LibraryCheckout.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Library")
                      ?? "Data Source=library.db"));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICirculationService, CirculationService>();

var app = builder.Build();

// Create and seed the database on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var time = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    await SeedData.EnsureSeededAsync(db, time);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
