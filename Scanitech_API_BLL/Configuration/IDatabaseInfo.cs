namespace Scanitech_API_BLL.Configuration;

/// <summary>
/// Kontrakt for database-forbindelsesoplysninger.
/// </summary>
public interface IDatabaseInfo
{
    string Server { get; init; }
    string Port { get; init; }
    string Database { get; init; }
    string User { get; init; }
    string Password { get; init; }
}

/// <summary>
/// Implementering af database-oplysninger.
/// </summary>
public sealed class DatabaseInfo : IDatabaseInfo
{
    public required string Server { get; init; }
    public required string Port { get; init; }
    public required string Database { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
}