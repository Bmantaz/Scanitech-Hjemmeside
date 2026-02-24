namespace Scanitech_API_DAL.Entities;

/// <summary>
/// Repræsentation af en kunde i databasen. (Version 5.0)
/// </summary>
public sealed class CustomerEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Faktureringsoplysninger kræves for at kunne oprette tickets
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? CVR { get; set; } // Optional for B2C, påkrævet for B2B

    // Sikkerhed: Kunder kan ikke oprette tickets før dette er true
    public bool IsApproved { get; set; } = false;

    public DateTime CreatedAt { get; set; }
}