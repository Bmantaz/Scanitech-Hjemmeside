using Scanitech_Frontend.Components;
using Scanitech_Frontend_BLL.Services;
using Scanitech_Frontend_DAL; // Her vil dine RestSharp kald bo
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOGGING (Vision 5.0 Standard) ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

try
{
    // --- 2. DEPENDENCY INJECTION ---
    // Her registrerer vi kun ting der er relevante for FRONTEND.
    // Vi registrerer IKKE CustomerRepository her, da det nu bor i API'et!

    // builder.Services.AddScoped<ICustomerService, CustomerService>(); 

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Registrering af HttpClient til at tale med din API
    builder.Services.AddHttpClient("ScanitechAPI", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001/");
    });

    var app = builder.Build();

    // --- 3. PIPELINE ---
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("Scanitech Frontend v5.0 kører...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Frontend startup fejlede");
}
finally
{
    Log.CloseAndFlush();
}