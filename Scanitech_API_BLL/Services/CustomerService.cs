using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API_BLL.Services;

// VISION 5.0: DTO (Data Transfer Object) defineret som en 'record'
// Records er immutable og allokerer minimal hukommelse. Perfekt til at sende data videre.
public record CustomerDto(int Id, string Name, string Email, DateTime CreatedAt);

/// <summary>
/// Service til håndtering af forretningslogik vedrørende kunder.
/// </summary>
/// <remarks>
/// Designbeslutning: Denne service fungerer som en orchestrator mellem API-controlleren 
/// og DataAccess-laget. Den håndterer logning, fejlhåndtering og DTO-mapping.
/// SRP: Klassen har kun ansvar for koordinering af kundelogik.
/// </remarks>
public sealed class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    /// <summary>
    /// Initialiserer en ny instans af <see cref="CustomerService"/> med nødvendige afhængigheder.
    /// </summary>
    /// <param name="customerRepository">Repository til kundedata.</param>
    /// <exception cref="ArgumentNullException">Kastes hvis customerRepository er null.</exception>
    public CustomerService(ICustomerRepository customerRepository)
    {
        // Guard clause: Fail fast hvis afhængigheder mangler (Coding Standards)
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>
    /// Henter alle kunder fra databasen, mapper dem til DTO'er og returnerer resultatet.
    /// </summary>
    /// <param name="ct">Token til annullering af asynkrone operationer.</param>
    /// <returns>En tuple bestående af en <see cref="OperationResult"/> og en skrivebeskyttet liste af <see cref="CustomerDto"/>.</returns>
    public async Task<(OperationResult Result, IReadOnlyList<CustomerDto> Data)> GetAllCustomersAsync(CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter hentning af alle kunder.");

            // 1. Hent rå data (Entities) fra databasen
            var entities = await _customerRepository.GetAllAsync(ct);

            // 2. Map Entities til DTO'er (Vision 5.0 Arkitektur)
            // WHY: Vi beskytter vores database-skema ved kun at returnere de felter, frontend'en har brug for.
            var dtos = entities
                .Select(e => new CustomerDto(e.Id, e.Name, e.Email, e.CreatedAt))
                .ToList()
                .AsReadOnly();

            Log.Information("BLL: Hentede og mappede succesfuldt {Count} kunder.", dtos.Count);

            // 3. Opret succes-resultat (C# 12 syntax med [])
            var result = new OperationResult(
                SuccessCount: dtos.Count,
                Errors: [],
                Warnings: []
            );

            return (result, dtos);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Hentning af kunder blev annulleret af klienten.");
            throw; // Lad frameworket håndtere den afbrudte request
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved hentning af kunder.");

            var result = new OperationResult(
                SuccessCount: 0,
                Errors: ["Systemet kunne ikke hente kundelisten. Prøv igen senere."],
                Warnings: []
            );

            // Returnerer et tomt array sammen med fejlen for at undgå null-reference exceptions i API-laget
            return (result, []);
        }
    }
}