using Scanitech_Hjemmeside;
using Scanitech_Hjemmeside.Components;
// using Scanitech_Frontend_BLL.Services; // Udkommenteret, indtil frontend BLL services er oprettet
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOGGING (Vision 5.0 Standard) ---
// Designbeslutning: Serilog konfigureres med fil-logning (daglig rullende) 
// for at forhindre ukontrolleret filvækst og sikre audit-spor.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/frontend-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Scanitech Frontend v5.0 starter op...");

    // --- 2. DEPENDENCY INJECTION ---
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Registrering af HttpClient til at tale med API'et
    // Guard clause: Fail fast hvis BaseUrl mangler (forhindrer skjulte runtime fejl)
    string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException("Kritisk Fejl: 'ApiSettings:BaseUrl' mangler i appsettings.json.");

    builder.Services.AddHttpClient("ScanitechAPI", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    var app = builder.Build();

    // --- 3. PIPELINE ---
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // HSTS (HTTP Strict Transport Security) forbedrer sikkerheden i produktion
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Scanitech Frontend v5.0 kører nu og er klar til requests.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Frontend startup fejlede uventet.");
}
finally
{
    Log.CloseAndFlush();
}