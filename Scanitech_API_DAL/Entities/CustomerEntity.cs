namespace Scanitech_API_DAL.Entities;

/// <summary>
/// Repræsentationsmodel for en kunde i SQL-databasen.
/// </summary>
/// <remarks>
/// Designbeslutning: Denne klasse er markeret som 'sealed', da den repræsenterer en flad 
/// datastruktur fra databasen og ikke er designet til nedarvning. 
/// Vi bruger 'required' for at sikre dataintegritet ved objekt-initialisering.
/// </remarks>
public sealed class CustomerEntity
{
    /// <summary>
    /// Unikt ID genereret af databasen (Primary Key).
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Kundens fulde navn eller firmanavn.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Kontakt-email. Kan være null hvis kunden kun er oprettet med telefon.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// UTC-tidsstempel for hvornår recorden blev oprettet.
    /// </summary>
    /// <remarks>
    /// Standardiseret til UTC for at undgå tidszone-konflikter på tværs af servere.
    /// </remarks>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}