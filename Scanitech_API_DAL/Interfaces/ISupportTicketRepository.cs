using Scanitech_API_DAL.Entities;

namespace Scanitech_API_DAL.Interfaces;

/// <summary>
/// Kontrakt for database-operationer relateret til Support Tickets (Vision 5.0)
/// </summary>
public interface ISupportTicketRepository
{
    /// <summary>
    /// Indsætter en ny ticket i databasen og returnerer det genererede ID.
    /// </summary>
    Task<int> InsertAsync(SupportTicketEntity entity, CancellationToken ct);

    /// <summary>
    /// Henter en specifik ticket ud fra ID.
    /// </summary>
    Task<SupportTicketEntity?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Henter alle tickets tilhørende en bestemt kunde (optimeret til read-only).
    /// </summary>
    Task<IReadOnlyList<SupportTicketEntity>> GetAllByCustomerIdAsync(int customerId, CancellationToken ct);

    /// <summary>
    /// Opdaterer en eksisterende ticket (f.eks. ved status-skift).
    /// </summary>
    Task UpdateAsync(SupportTicketEntity entity, CancellationToken ct);
}