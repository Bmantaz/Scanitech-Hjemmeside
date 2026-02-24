using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_BLL.Models;
using Serilog;

namespace Scanitech_API_BLL.Services;

// DTO til at sende data UD af API'et (Get)
public record CustomerDto(int Id, string Name, string Email, string Address, string PostalCode, string City, string? CVR, bool IsApproved, DateTime CreatedAt);

// DTO til at tage data IND i API'et (Post)
public record CustomerCreateDto(string Name, string Email, string Address, string PostalCode, string City, string? CVR);

// DTO til at opdatere en eksisterende kunde (Put)
public record CustomerUpdateDto(int Id, string Name, string Email, string Address, string PostalCode, string City, string? CVR);

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

    public async Task<(OperationResult Result, IReadOnlyList<CustomerDto> Data)> GetAllCustomersAsync(CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter hentning af alle kunder.");

            var entities = await _customerRepository.GetAllAsync(ct);

            var dtos = entities
                .Select(e => new CustomerDto(e.Id, e.Name, e.Email, e.Address, e.PostalCode, e.City, e.CVR, e.IsApproved, e.CreatedAt))
                .ToList()
                .AsReadOnly();

            Log.Information("BLL: Hentede og mappede succesfuldt {Count} kunder.", dtos.Count);

            return (new OperationResult(dtos.Count, [], []), dtos);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Hentning af kunder blev annulleret af klienten.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved hentning af kunder.");
            return (new OperationResult(0, ["Systemet kunne ikke hente kundelisten. Prøv igen senere."], []), []);
        }
    }

    public async Task<(OperationResult Result, int? NewCustomerId)> CreateCustomerAsync(CustomerCreateDto dto, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter oprettelse af ny kunde: {Name}", dto.Name);

            // Strammere 5.0 validering
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Address) || string.IsNullOrWhiteSpace(dto.City) ||
                string.IsNullOrWhiteSpace(dto.PostalCode))
            {
                Log.Warning("BLL: Validering fejlede ved oprettelse af kunde. Mangler basis faktureringsdata.");
                return (new OperationResult(0, ["Navn, Email, Adresse, Postnummer og By er påkrævet for at oprette en fakturerbar konto."], []), null);
            }

            var entity = new CustomerEntity
            {
                Name = dto.Name,
                Email = dto.Email,
                Address = dto.Address,
                PostalCode = dto.PostalCode,
                City = dto.City,
                CVR = dto.CVR,
                IsApproved = false, // Security by Design: Altid falsk ved oprettelse
                CreatedAt = DateTime.UtcNow
            };

            int newId = await _customerRepository.InsertAsync(entity, ct);

            Log.Information("BLL: Kunde oprettet succesfuldt med ID: {Id}. Afventer godkendelse.", newId);
            return (new OperationResult(1, [], []), newId);
        }
        catch (OperationCanceledException)
        {
            Log.Warning("BLL: Oprettelse af kunde blev annulleret af klienten.");
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved oprettelse af kunde.");
            return (new OperationResult(0, ["Systemet kunne ikke oprette kunden. Prøv igen senere."], []), null);
        }
    }

    public async Task<OperationResult> UpdateCustomerAsync(CustomerUpdateDto dto, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter opdatering af kunde med ID: {Id}", dto.Id);

            if (dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
            {
                Log.Warning("BLL: Validering fejlede ved opdatering af kunde {Id}.", dto.Id);
                return new OperationResult(0, ["Ugyldigt input. ID, navn og email er påkrævet."], []);
            }

            var existingCustomer = await _customerRepository.GetByIdAsync(dto.Id, ct);
            if (existingCustomer == null)
            {
                Log.Warning("BLL: Forsøgte at opdatere en kunde der ikke findes (ID: {Id}).", dto.Id);
                return new OperationResult(0, ["Kunden blev ikke fundet i databasen."], []);
            }

            existingCustomer.Name = dto.Name;
            existingCustomer.Email = dto.Email;
            existingCustomer.Address = dto.Address;
            existingCustomer.PostalCode = dto.PostalCode;
            existingCustomer.City = dto.City;
            existingCustomer.CVR = dto.CVR;

            await _customerRepository.UpdateAsync(existingCustomer, ct);

            Log.Information("BLL: Kunde {Id} opdateret succesfuldt.", dto.Id);
            return new OperationResult(1, [], []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved opdatering af kunde {Id}.", dto.Id);
            return new OperationResult(0, ["Der opstod en fejl ved opdatering af kunden."], []);
        }
    }

    /// <summary>
    /// Godkender en kunde, så de får adgang til ticket-systemet.
    /// </summary>
    public async Task<OperationResult> ApproveCustomerAsync(int id, CancellationToken ct)
    {
        try
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(id, ct);
            if (existingCustomer == null)
            {
                return new OperationResult(0, ["Kunden blev ikke fundet."], []);
            }

            existingCustomer.IsApproved = true;
            await _customerRepository.UpdateAsync(existingCustomer, ct);

            Log.Information("BLL: Kunde {Id} er nu godkendt til ticket-oprettelse.", id);
            return new OperationResult(1, [], []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fejl under godkendelse af kunde {Id}.", id);
            return new OperationResult(0, ["Kunne ikke godkende kunden."], []);
        }
    }

    public async Task<OperationResult> DeleteCustomerAsync(int id, CancellationToken ct)
    {
        try
        {
            Log.Information("BLL: Starter sletning af kunde med ID: {Id}", id);

            if (id <= 0) return new OperationResult(0, ["Ugyldigt kunde-ID."], []);

            await _customerRepository.DeleteAsync(id, ct);

            Log.Information("BLL: Kunde {Id} slettet succesfuldt.", id);
            return new OperationResult(1, [], []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BLL: Fatal fejl ved sletning af kunde {Id}.", id);
            return new OperationResult(0, ["Kunne ikke slette kunden. Prøv igen senere."], []);
        }
    }
}