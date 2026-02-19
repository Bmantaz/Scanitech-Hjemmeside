using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;

namespace Scanitech_API_DAL.Repositories;

/// <summary>
/// Dapper-baseret repository til håndtering af kundedata i SQL Server.
/// </summary>
/// <remarks>
/// Designbeslutning: Vi benytter Dapper frem for EF Core her, da det giver os fuld kontrol 
/// over SQL-optimering og minimerer overhead ved high-volume forespørgsler.
/// Forbindelsen åbnes og lukkes per operation for at undgå connection pool exhaustion.
/// </remarks>
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initialiserer en ny instans af <see cref="CustomerRepository"/>.
    /// </summary>
    /// <param name="connectionString">Den dekrypterede forbindelsesstreng til SQL Server.</param>
    /// <exception cref="ArgumentNullException">Kastes hvis connectionString er tom.</exception>
    public CustomerRepository(string connectionString)
    {
        // Guard clause for at sikre at vi ikke starter uden en valid forbindelse
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        _connectionString = connectionString;
    }

    /// <inheritdoc />
    public async Task<CustomerEntity?> GetByIdAsync(int id, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        const string sql = "SELECT Id, Name, Email, CreatedAt FROM Customers WHERE Id = @Id";

        // CommandDefinition gør det muligt at sende CancellationTokens direkte til Dapper
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: ct);

        return await connection.QuerySingleOrDefaultAsync<CustomerEntity>(command);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CustomerEntity>> GetAllAsync(CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);

        const string sql = "SELECT Id, Name, Email, CreatedAt FROM Customers";

        var command = new CommandDefinition(sql, cancellationToken: ct);

        var result = await connection.QueryAsync<CustomerEntity>(command);

        // Returnerer som ReadOnlyList for at overholde arkitektur-princippet om immutabilitet ud af DAL
        return result.ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<int> InsertAsync(CustomerEntity entity, CancellationToken ct)
    {
        // Validering af input før database-interaktion
        ArgumentNullException.ThrowIfNull(entity);

        await using var connection = new SqlConnection(_connectionString);

        // SCOPE_IDENTITY() er sikrere end @@IDENTITY da den er begrænset til den aktuelle kørsel
        const string sql = @"
            INSERT INTO Customers (Name, Email, CreatedAt) 
            VALUES (@Name, @Email, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var command = new CommandDefinition(sql, entity, cancellationToken: ct);

        return await connection.ExecuteScalarAsync<int>(command);
    }
}