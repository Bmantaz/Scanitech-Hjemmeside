using Scanitech_API_DAL.Interfaces;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API_BLL.Services;

/// <summary>
/// Service til håndtering af forretningslogik vedrørende kunder.
/// </summary>
/// <remarks>
/// Designbeslutning: Denne service fungerer som en orchestrator mellem API-controlleren 
/// og DataAccess-laget. Den håndterer logning og transformering af data.
/// SRP: Klassen har kun ansvar for koordinering af kundelogi.
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
    /// Henter alle kunder fra databasen og returnerer et samlet resultatobjekt.
    /// </summary>
    /// <param name="ct">Token til annullering af asynkrone operationer.</param>
    /// <returns>En <see cref="OperationResult"/> der indeholder status og eventuelle fejlbeskeder.</returns>
    /// <remarks>
    /// Fejlhåndtering: Vi fanger exceptions her for at undgå at crashe API'et og returnerer i stedet et OperationResult.
    /// </remarks>
    public async Task<OperationResult> GetAllCustomersAsync(CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter hentning af alle kunder.");

            var customers = await _customerRepository.GetAllAsync(ct);

            Log.Information("BLL: Hentede succesfuldt {Count} kunder.", customers.Count);

            // Returnerer succes-resultat (Vision 5.0 mønster)
            return new OperationResult(
                SuccessCount: customers.Count,
                Errors: Array.Empty<string>(),
                Warnings: Array.Empty<string>()
            );
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Hentning af kunder blev annulleret.");
            throw; // Lad kalderen vide at det blev afbrudt
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved hentning af kunder.");
            return new OperationResult(
                SuccessCount: 0,
                Errors: new[] { "Systemet kunne ikke hente kundelisten. Prøv igen senere." },
                Warnings: Array.Empty<string>()
            );
        }
    }
}