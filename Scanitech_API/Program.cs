using Scanitech_API_BLL.Services;
using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Repositories;
using Scanitech_API_DAL.Contexts; // Ny reference til din DbContext
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// --- 1. LOGGING (Vision 5.0 Standard) ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Scanitech API v5.0 (EF Core Edition) starter op...");

    // --- 2. DEPENDENCY INJECTION ---
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Hent forbindelsesstreng
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Kritisk fejl: Forbindelsesstrengen 'DefaultConnection' mangler i appsettings.json.");

    // --- EF CORE KONFIGURATION ---
    // Dette er din DbContext som underviseren vil se
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Registrering af repositories og services
    // Repository kræver nu ikke længere manuel string-injektion, da det bruger AppDbContext
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
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

    // Global fejlhåndtering
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

    Log.Information("Scanitech API v5.0 kører nu med EF Core.");
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