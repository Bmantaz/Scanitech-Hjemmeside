using Microsoft.AspNetCore.Mvc;
using Scanitech_API_BLL.Services;
using Serilog;

namespace Scanitech_API.Controllers;

/// <summary>
/// API Controller for håndtering af AI Chat.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public sealed class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService ?? throw new ArgumentNullException(nameof(chatService));
    }

    /// <summary>
    /// Sender en besked til AI'en og returnerer svaret.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto dto, CancellationToken cancellationToken)
    {
        Log.Information("API: Modtog HTTP POST anmodning til AI Chat.");

        var (operationResult, responseData) = await _chatService.SendMessageAsync(dto, cancellationToken);

        if (operationResult.SuccessCount == 0)
        {
            if (operationResult.Errors.Any(e => e.Contains("tom")))
            {
                return BadRequest(new { Message = "Ugyldigt input.", Errors = operationResult.Errors });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "Kunne ikke kommunikere med AI-tjenesten.",
                Errors = operationResult.Errors
            });
        }

        return Ok(new
        {
            Meta = operationResult,
            Data = responseData
        });
    }
}