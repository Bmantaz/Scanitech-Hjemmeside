using Microsoft.AspNetCore.Mvc;
using Scanitech_API_BLL.Services;
using Serilog;

namespace Scanitech_API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] TicketCreateDto dto, CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog anmodning om oprettelse af ticket.");

        var (operationResult, newTicketId) = await _ticketService.CreateTicketAsync(dto, cancellationToken);

        if (operationResult.SuccessCount == 0)
        {
            // Hvis det er et godkendelsesproblem, returner 403 Forbidden
            if (operationResult.Errors.Any(e => e.Contains("afventer stadig godkendelse")))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Adgang nægtet.", Errors = operationResult.Errors });
            }

            return BadRequest(new { Message = "Kunne ikke oprette ticket.", Errors = operationResult.Errors });
        }

        return Created($"/api/v1/Tickets/{newTicketId}", new { Meta = operationResult, Data = new { Id = newTicketId } });
    }
}