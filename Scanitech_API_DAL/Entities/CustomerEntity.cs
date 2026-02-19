namespace Scanitech_API_DAL.Entities;

/// <summary>
/// Repræsentante for en kunde i databasen.
/// </summary>
public sealed class CustomerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}