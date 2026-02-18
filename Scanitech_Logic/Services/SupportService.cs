using Scanitech_Logic.Models;
using Scanitech_DataAccess.Interfaces;
using Serilog;

namespace Scanitech_Logic.Services;

public sealed class SupportService
{
    private readonly ICustomerRepository _repository;

    public SupportService(ICustomerRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository)); // Guard Clause
    }

    public async Task<OperationResult> CreateSupportTicketAsync(string customerEmail, string issue)
    {
        var errors = new List<string>();

        try
        {
            // Validering
            ArgumentException.ThrowIfNullOrWhiteSpace(customerEmail);

            Log.Information("Opretter support ticket for {Email}", customerEmail); // Structured logging

            // Logik her (fx kald til repository)
            return new OperationResult(1, Array.Empty<string>(), Array.Empty<string>());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fatal fejl ved oprettelse af ticket for {Email}", customerEmail);
            errors.Add(ex.Message);
            return new OperationResult(0, errors, Array.Empty<string>());
        }
    }
}