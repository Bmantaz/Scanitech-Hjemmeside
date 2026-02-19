using Scanitech_API_DAL.Interfaces;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_Frontend_BLL.Services;

/// <summary>
/// Service til håndtering af forretningslogik vedrørende kunder.
/// </summary>
/// <remarks>
/// Designbeslutning: Denne service fungerer som en orchestrator mellem API-controlleren 
/// og DataAccess-laget. Den håndterer logning og transformering af data.
/// </remarks>
public sealed class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    /// <summary>Initialiserer en ny instans med nødvendige afhængigheder.</summary>
    /// <param name="customerRepository">Repository til kundedata.</param>
    public CustomerService(ICustomerRepository customerRepository)
    {
        // Guard clause: Fail fast hvis afhængigheder mangler
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>Henter alle kunder og returnerer et samlet resultat.</summary>
    /// <param name="ct">Token til annullering.</param>
    public async Task<OperationResult> GetAllCustomersAsync(CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Henter alle kunder");
            var customers = await _customerRepository.GetAllAsync(ct);

            return new OperationResult(customers.Count, Array.Empty<string>(), Array.Empty<string>());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved hentning af kunder");
            return new OperationResult(0, new[] { "Kunne ikke hente kundeliste" }, Array.Empty<string>());
        }
    }
}