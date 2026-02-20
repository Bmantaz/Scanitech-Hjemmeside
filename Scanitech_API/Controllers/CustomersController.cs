using Microsoft.AspNetCore.Mvc;
using Scanitech_API_BLL.Services;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API.Controllers;

/// <summary>
/// API Controller for håndtering af kunder.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;

    public CustomersController(CustomerService customerService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// Henter en liste over alle kunder i systemet.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP GET anmodning for at hente alle kunder.");

        var (operationResult, customers) = await _customerService.GetAllCustomersAsync(cancellationToken);

        if (operationResult.SuccessCount == 0 && operationResult.Errors.Any())
        {
            Log.Warning("API: Returnerer 500 Internal Server Error pga. fejl i BLL.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Der opstod en fejl under hentning af data.",
                Errors = operationResult.Errors
            });
        }

        return Ok(new
        {
            Meta = operationResult,
            Data = customers
        });
    }

    /// <summary>
    /// Opretter en ny kunde.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CustomerCreateDto dto, CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP POST anmodning for at oprette en ny kunde.");

        var (operationResult, newCustomerId) = await _customerService.CreateCustomerAsync(dto, cancellationToken);

        // Håndter fejl fra servicen (f.eks. manglende input eller databasefejl)
        if (operationResult.SuccessCount == 0)
        {
            if (operationResult.Errors.Any(e => e.Contains("påkrævet")))
            {
                Log.Warning("API: Returnerer 400 Bad Request pga. valideringsfejl.");
                return BadRequest(new { Message = "Ugyldigt input.", Errors = operationResult.Errors });
            }

            Log.Error("API: Returnerer 500 Internal Server Error pga. fejl i BLL.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Der opstod en fejl under oprettelse af kunden.",
                Errors = operationResult.Errors
            });
        }

        // Returner 201 Created sammen med det genererede ID
        return Created($"/api/v1/Customers/{newCustomerId}", new
        {
            Meta = operationResult,
            Data = new { Id = newCustomerId, dto.Name, dto.Email }
        });
    }
}