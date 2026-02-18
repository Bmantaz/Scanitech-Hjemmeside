using Scanitech_DataAccess.Interfaces;
using Scanitech_DataAccess.Repositories;
using Scanitech_Hjemmeside.Components; 
using Scanitech_Hjemmeside.Scanitech_Service.Components;
using Scanitech_Logic.Configuration;
using Scanitech_Logic.Security;
using Scanitech_Logic.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOGGING ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

try
{
    // --- 2. SIKKERHED & KONFIGURATION ---
    builder.Services.AddSingleton<IPasswordProtector, PasswordProtector>();

    var dbSection = builder.Configuration.GetSection("DatabaseInfo");
    builder.Services.AddSingleton<IDatabaseInfo>(sp => new DatabaseInfo
    {
        Server = dbSection["Server"] ?? "localhost",
        Port = dbSection["Port"] ?? "1433",
        Database = dbSection["Database"] ?? "",
        User = dbSection["User"] ?? "",
        Password = sp.GetRequiredService<IPasswordProtector>().Unprotect(dbSection["Password"] ?? "")
    });

    // --- 3. DEPENDENCY INJECTION ---
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<CustomerService>();
    builder.Services.AddScoped<SupportService>();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    var app = builder.Build();

    // --- 4. PIPELINE ---
    app.UseStaticFiles();
    app.UseAntiforgery();
    app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Startup failed");
}
finally
{
    Log.CloseAndFlush();
}