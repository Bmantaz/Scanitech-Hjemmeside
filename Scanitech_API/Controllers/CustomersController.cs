using Microsoft.AspNetCore.Mvc;
using Scanitech_API_BLL.Services;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API.Controllers;

/// <summary>
/// API Controller for håndtering af kunder.
/// </summary>
/// <remarks>
/// Designbeslutning: Dette er en "tynd" controller. Ingen forretningslogik eller 
/// direkte databasekald findes her. Alt delegeres til CustomerService (Vision 5.0 standard).
/// </remarks>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")] // Vision 5.0: Eksplicit API-kontrakt
public sealed class CustomersController : ControllerBase // Vision 5.0: Sealed for JIT optimering
{
    private readonly CustomerService _customerService;

    /// <summary>
    /// Initialiserer en ny instans af <see cref="CustomersController"/>.
    /// </summary>
    /// <param name="customerService">Service til håndtering af kundelogik.</param>
    /// <exception cref="ArgumentNullException">Kastes hvis customerService er null.</exception>
    public CustomersController(CustomerService customerService)
    {
        // Guard clause for at sikre dependency injection
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// Henter en liste over alle kunder i systemet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for at afbryde requestet hvis klienten lukker forbindelsen.</param>
    /// <returns>Et HTTP resultat der indeholder operationens status og kundedata.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP GET anmodning for at hente alle kunder.");

        // VISION 5.0: Vi dekonstruerer vores Tuple fra BLL-laget, så vi får BÅDE status og data.
        var (operationResult, customers) = await _customerService.GetAllCustomersAsync(cancellationToken);

        // Hvis der er fejl, returner 500 Internal Server Error.
        // I Vision 5.0 afslører vi ikke interne databasedetaljer til klienten.
        if (operationResult.SuccessCount == 0 && operationResult.Errors.Count > 0)
        {
            Log.Warning("API: Returnerer 500 Internal Server Error pga. fejl i BLL.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Der opstod en fejl under hentning af data.",
                Errors = operationResult.Errors
            });
        }

        // Returnerer data til klienten, pakket pænt ind med metadata
        return Ok(new
        {
            Meta = operationResult,
            Data = customers
        });
    }
}