using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API_BLL.Services;

// DTO til at tage data IND i API'et fra frontenden
public record TicketCreateDto(int CustomerId, string Title, string Description, bool HasConsented);

public sealed class TicketService
{
    private readonly ISupportTicketRepository _ticketRepository;
    private readonly ICustomerRepository _customerRepository;

    public TicketService(ISupportTicketRepository ticketRepository, ICustomerRepository customerRepository)
    {
        _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>
    /// Opretter en ny ticket, KUN hvis kunden er godkendt og har givet samtykke til fakturering.
    /// </summary>
    public async Task<(OperationResult Result, int? NewTicketId)> CreateTicketAsync(TicketCreateDto dto, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Forsøger at oprette ticket for CustomerId: {CustomerId}", dto.CustomerId);

            // 1. Validering af input og samtykke (Frictionless but Secure)
            if (!dto.HasConsented)
            {
                Log.Warning("BLL: Afvist. Kunden har ikke givet samtykke til fakturering.");
                return (new OperationResult(0, ["Du skal acceptere betingelserne for fakturering for at oprette en sag."], []), null);
            }

            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Description))
            {
                return (new OperationResult(0, ["Titel og beskrivelse er påkrævet."], []), null);
            }

            // 2. Validering af kunden og godkendelsesstatus (Vision 5.0 Standard)
            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId, ct);

            if (customer == null)
            {
                Log.Warning("BLL: Afvist. Kunden findes ikke (ID: {CustomerId}).", dto.CustomerId);
                return (new OperationResult(0, ["Kundekontoen blev ikke fundet."], []), null);
            }

            if (!customer.IsApproved)
            {
                Log.Warning("BLL: Afvist. Kunden (ID: {CustomerId}) er endnu ikke valideret/godkendt.", dto.CustomerId);
                return (new OperationResult(0, ["Din konto afventer stadig godkendelse. Du kan oprette tickets, når vi har valideret dine oplysninger."], []), null);
            }

            // 3. Oprettelse af entitet med Audit Trail
            var ticketEntity = new SupportTicketEntity
            {
                CustomerId = dto.CustomerId,
                Title = dto.Title,
                Description = dto.Description,
                Status = "Ny",
                CreatedAt = DateTime.UtcNow,
                ConsentGivenAt = DateTime.UtcNow // Backend bestemmer tiden for at undgå svindel
            };

            int newId = await _ticketRepository.InsertAsync(ticketEntity, ct);

            Log.Information("BLL: Ticket oprettet succesfuldt med ID: {Id}", newId);
            return (new OperationResult(1, [], []), newId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved oprettelse af ticket for CustomerId {CustomerId}.", dto.CustomerId);
            return (new OperationResult(0, ["Der opstod en systemfejl. Prøv igen senere."], []), null);
        }
    }
}