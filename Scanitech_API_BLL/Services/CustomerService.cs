using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API_BLL.Services;

// DTO til at sende data UD af API'et (Get)
public record CustomerDto(int Id, string Name, string Email, DateTime CreatedAt);

// DTO til at tage data IND i API'et (Post)
public record CustomerCreateDto(string Name, string Email);

// DTO til at opdatere en eksisterende kunde (Put)
public record CustomerUpdateDto(int Id, string Name, string Email);

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

            var entity = new CustomerEntity
            {
                Name = dto.Name,
                Email = dto.Email
            };

            int newId = await _customerRepository.InsertAsync(entity, ct);

            Log.Information("BLL: Kunde oprettet succesfuldt med ID: {Id}", newId);

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

    /// <summary>
    /// Opdaterer en eksisterende kunde ud fra input DTO'en.
    /// </summary>
    public async Task<OperationResult> UpdateCustomerAsync(CustomerUpdateDto dto, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter opdatering af kunde med ID: {Id}", dto.Id);

            // 1. Valider input
            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            {
                Log.Warning("BLL: Validering fejlede ved opdatering af kunde {Id}.", dto.Id);
                return new OperationResult(0, ["Ugyldigt input. ID, navn og email er påkrævet."], []);
            }

            // 2. Tjek om kunden findes i forvejen
            var existingCustomer = await _customerRepository.GetByIdAsync(dto.Id, ct);
            if (existingCustomer == null)
            {
                Log.Warning("BLL: Forsøgte at opdatere en kunde der ikke findes (ID: {Id}).", dto.Id);
                return new OperationResult(0, ["Kunden blev ikke fundet i databasen."], []);
            }

            // 3. Opdater entiteten med nye data (CreatedAt bibeholdes da vi kun ændrer Name og Email)
            existingCustomer.Name = dto.Name;
            existingCustomer.Email = dto.Email;

            // 4. Gem ændringer via repository
            await _customerRepository.UpdateAsync(existingCustomer, ct);

            Log.Information("BLL: Kunde {Id} opdateret succesfuldt.", dto.Id);
            return new OperationResult(1, [], []);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Opdatering af kunde blev annulleret.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved opdatering af kunde {Id}.", dto.Id);
            return new OperationResult(0, ["Der opstod en fejl ved opdatering af kunden."], []);
        }
    }

    /// <summary>
    /// Sletter en kunde ud fra ID.
    /// </summary>
    public async Task<OperationResult> DeleteCustomerAsync(int id, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter sletning af kunde med ID: {Id}", id);

            if (id <= 0)
            {
                Log.Warning("BLL: Validering fejlede ved sletning af kunde. ID var ugyldigt ({Id}).", id);
                return new OperationResult(0, ["Ugyldigt kunde-ID."], []);
            }

            // Kalder repository, som skyder en direkte slette-kommando (ExecuteDeleteAsync) afsted
            await _customerRepository.DeleteAsync(id, ct);

            Log.Information("BLL: Kunde {Id} slettet succesfuldt.", id);
            return new OperationResult(1, [], []);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Sletning af kunde blev annulleret.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved sletning af kunde {Id}.", id);
            return new OperationResult(0, ["Kunne ikke slette kunden. Prøv igen senere."], []);
        }
    }
}