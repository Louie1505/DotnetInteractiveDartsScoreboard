using DartsScoreboardGames.Components;
using DartsScoreboardGames.Infrastructure.Data;
using DartsScoreboardGames.Infrastructure.Services;
using DartsScoreboardGames.Services;
using DartsScoreboardGames.Theming;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddMudServices()
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(o => {
    o.UseSqlite("Data Source=db.db");
});

builder.Services.AddSingleton<CustomThemeProvider>();
builder.Services.AddSingleton<GameStateProvider>();
builder.Services.AddSingleton<AvatarImageProvider>();
builder.Services.AddSingleton<CheckoutCalculator>();

builder.Services.AddScoped<PlayerHistoryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using var scope = app.Services.CreateScope();
using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await context.Database.EnsureCreatedAsync();

await app.RunAsync("http://*:5144");
