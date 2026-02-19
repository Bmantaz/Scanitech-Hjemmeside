using Scanitech_API_BLL.Services;
using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOGGING (Vision 5.0 Standard) ---
// Opsætning af Serilog med daglig rolling file og konsol-output.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Scanitech API v5.0 starter op...");

    // --- 2. DEPENDENCY INJECTION ---
    builder.Services.AddControllers();

    // OpenAPI/Swagger konfiguration
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Hent forbindelsesstreng fra appsettings.json
    // Designbeslutning: Vi fejler hurtigt hvis forbindelsen mangler.
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Kritisk fejl: Forbindelsesstrengen 'DefaultConnection' mangler i appsettings.json.");

    // Registrering af lagene
    // CustomerRepository kræver forbindelsesstrengen direkte i sin constructor.
    builder.Services.AddScoped<ICustomerRepository>(sp => new CustomerRepository(connectionString));
    builder.Services.AddScoped<CustomerService>();

    var app = builder.Build();

    // --- 3. PIPELINE KONFIGURATION ---
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("Swagger er tilgængelig i Development mode.");
    }

    app.UseHttpsRedirection();

    // Global fejlhåndtering (standard i Vision 5.0)
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { Error = "Der opstod en uventet fejl i API'et." });
        });
    });

    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Scanitech API v5.0 kører nu.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API startup fejlede uventet.");
}
finally
{
    Log.CloseAndFlush();
}