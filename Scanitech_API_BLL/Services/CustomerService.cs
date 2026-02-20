using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API_BLL.Services;

// DTO til at sende data UD af API'et (Get)
public record CustomerDto(int Id, string Name, string Email, DateTime CreatedAt);

// DTO til at tage data IND i API'et (Post)
public record CustomerCreateDto(string Name, string Email);

/// <summary>
/// Service til håndtering af forretningslogik vedrørende kunder.
/// </summary>
public sealed class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>
    /// Henter alle kunder fra databasen, mapper dem til DTO'er og returnerer resultatet.
    /// </summary>
    public async Task<(OperationResult Result, IReadOnlyList<CustomerDto> Data)> GetAllCustomersAsync(CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter hentning af alle kunder.");

            var entities = await _customerRepository.GetAllAsync(ct);

            var dtos = entities
                .Select(e => new CustomerDto(e.Id, e.Name, e.Email, e.CreatedAt))
                .ToList()
                .AsReadOnly();

            Log.Information("BLL: Hentede og mappede succesfuldt {Count} kunder.", dtos.Count);

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
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved hentning af kunder.");

            var result = new OperationResult(
                SuccessCount: 0,
                Errors: ["Systemet kunne ikke hente kundelisten. Prøv igen senere."],
                Warnings: []
            );

            return (result, []);
        }
    }

    /// <summary>
    /// Opretter en ny kunde i databasen baseret på input DTO'en.
    /// </summary>
    public async Task<(OperationResult Result, int? NewCustomerId)> CreateCustomerAsync(CustomerCreateDto dto, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter oprettelse af ny kunde: {Name}", dto.Name);

            // 1. Simpel validering (Guard clauses)
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            {
                Log.Warning("BLL: Validering fejlede ved oprettelse af kunde. Navn og Email manglede.");

                var errorResult = new OperationResult(
                    SuccessCount: 0,
                    Errors: ["Navn og Email er påkrævet."],
                    Warnings: []
                );
                return (errorResult, null);
            }

            // 2. Map fra DTO til Entity
            var entity = new CustomerEntity
            {
                Name = dto.Name,
                Email = dto.Email
                // CreatedAt sættes automatisk af databasens default value (GETDATE())
            };

            // 3. Gem i databasen
            int newId = await _customerRepository.InsertAsync(entity, ct);

            Log.Information("BLL: Kunde oprettet succesfuldt med ID: {Id}", newId);

            // 4. Returner succes
            var successResult = new OperationResult(
                SuccessCount: 1,
                Errors: [],
                Warnings: []
            );

            return (successResult, newId);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Oprettelse af kunde blev annulleret af klienten.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved oprettelse af kunde.");

            var result = new OperationResult(
                SuccessCount: 0,
                Errors: ["Systemet kunne ikke oprette kunden. Prøv igen senere."],
                Warnings: []
            );

            return (result, null);
        }
    }
}