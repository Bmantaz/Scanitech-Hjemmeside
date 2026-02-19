using Scanitech_API_DAL.Entities;

namespace Scanitech_API_DAL.Interfaces;

/// <summary>
/// Kontrakt for dataadgangsoperationer relateret til kunder.
/// </summary>
/// <remarks>
/// Designbeslutning: Alle metoder er asynkrone for at supportere 'Async-first' princippet 
/// og sikre optimal skalering af tråde under database-I/O.
/// </remarks>
public interface ICustomerRepository
{
    /// <summary>
    /// Henter en specifik kunde baseret på ID.
    /// </summary>
    /// <param name="id">Kundens unikke identifikator.</param>
    /// <param name="ct">Token til annullering af igangværende database-kald.</param>
    /// <returns>En CustomerEntity hvis fundet; ellers null.</returns>
    Task<CustomerEntity?> GetByIdAsync(int id, CancellationToken ct);

    /// <summary>
    /// Henter alle kunder fra systemet.
    /// </summary>
    /// <param name="ct">Token til annullering af igangværende database-kald.</param>
    /// <returns>En read-only liste af alle kunder.</returns>
    /// <remarks>
    /// Returnerer IReadOnlyList for at indikere over for Logic-laget, at samlingen ikke skal modificeres direkte.
    /// </remarks>
    Task<IReadOnlyList<CustomerEntity>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Indsætter en ny kunde og returnerer det genererede ID.
    /// </summary>
    /// <param name="entity">Entiteten der skal persisteres.</param>
    /// <param name="ct">Token til annullering af operationen.</param>
    /// <returns>Det tildelte ID fra databasen.</returns>
    Task<int> InsertAsync(CustomerEntity entity, CancellationToken ct);
}