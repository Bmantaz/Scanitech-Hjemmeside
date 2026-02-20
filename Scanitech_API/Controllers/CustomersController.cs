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

        return Created($"/api/v1/Customers/{newCustomerId}", new
        {
            Meta = operationResult,
            Data = new { Id = newCustomerId, dto.Name, dto.Email }
        });
    }

    /// <summary>
    /// Opdaterer en eksisterende kunde.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateDto dto, CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP PUT anmodning for at opdatere kunde {Id}.", id);

        // Sikkerhed: Vi sikrer at de ikke prøver at rette ID 2, men sender ID 3 i deres JSON
        if (id != dto.Id)
        {
            Log.Warning("API: Returnerer 400 Bad Request. ID i URL matcher ikke ID i body.");
            return BadRequest(new { Message = "ID i URL matcher ikke ID i data." });
        }

        var operationResult = await _customerService.UpdateCustomerAsync(dto, cancellationToken);

        if (operationResult.SuccessCount == 0)
        {
            // Hvis BLL fortalte os, at kunden ikke findes, returnerer vi korrekt 404
            if (operationResult.Errors.Any(e => e.Contains("ikke fundet")))
            {
                return NotFound(new { Message = "Kunden blev ikke fundet.", Errors = operationResult.Errors });
            }

            return BadRequest(new { Message = "Kunne ikke opdatere kunden.", Errors = operationResult.Errors });
        }

        return Ok(new
        {
            Meta = operationResult,
            Message = "Kunden blev opdateret succesfuldt."
        });
    }

    /// <summary>
    /// Sletter en kunde fra systemet.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP DELETE anmodning for at slette kunde {Id}.", id);

        var operationResult = await _customerService.DeleteCustomerAsync(id, cancellationToken);

        if (operationResult.SuccessCount == 0)
        {
            return BadRequest(new { Message = "Kunne ikke slette kunden.", Errors = operationResult.Errors });
        }

        return Ok(new
        {
            Meta = operationResult,
            Message = "Kunden blev slettet succesfuldt."
        });
    }
}