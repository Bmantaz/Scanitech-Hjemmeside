using Scanitech_API_BLL.Services;
using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Repositories;
using Scanitech_API_DAL.Contexts;
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
    builder.Services.AddOpenApi();

    // Hent forbindelsesstreng
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Kritisk fejl: Forbindelsesstrengen 'DefaultConnection' mangler i appsettings.json.");

    // --- EF CORE KONFIGURATION ---
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Registrering af repositories og services
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>(); // <-- TILFØJ DENNE
    builder.Services.AddScoped<CustomerService>();
    builder.Services.AddScoped<TicketService>();
    builder.Services.AddScoped<ChatService>();

    // --- CORS KONFIGURATION (Tillader at vores HTML-frontend kan kalde API'et) ---
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // --- VISION 5.0: AUTOMATISK MIGRATION & SEEDING ---
    // Dette loop tjekker databasen ved hver opstart. Perfekt til multi-pc udvikling.
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            Log.Information("Tjekker og opdaterer LocalDB database...");

            var context = services.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync(); // Asynkron database-opbygning

            Log.Information("Database migration og seeding gennemført med succes!");
        }
        catch (Exception ex)
        {
            // Vi fanger fejlen kritisk, så vi kan se præcis, hvis LocalDB driller
            Log.Fatal(ex, "Der opstod en kritisk fejl under automatisk database migration.");
        }
    }

    // --- 3. PIPELINE KONFIGURATION ---
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi(); // Microsofts native OpenAPI endpoint
        Log.Information("Microsoft OpenAPI er aktivt i Development mode.");
    }

    app.UseHttpsRedirection();

    // --- AKTIVER CORS I PIPELINEN ---
    app.UseCors("AllowAll");

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