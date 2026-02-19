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
public class CustomersController : ControllerBase
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
    /// <returns>Et HTTP resultat der indeholder operationens status og data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(OperationResult), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP GET anmodning for at hente alle kunder.");

        var result = await _customerService.GetAllCustomersAsync(cancellationToken);

        // Hvis der er fejl (og ingen succesfulde hentninger), returner 500 Internal Server Error.
        // I Vision 5.0 afslører vi ikke interne databasedetaljer til klienten.
        if (result.SuccessCount == 0 && result.Errors.Any())
        {
            Log.Warning("API: Returnerer 500 Internal Server Error pga. fejl i BLL.");
            return StatusCode(500, new { Message = "Der opstod en fejl under hentning af data.", Errors = result.Errors });
        }

        return Ok(result);
    }
}