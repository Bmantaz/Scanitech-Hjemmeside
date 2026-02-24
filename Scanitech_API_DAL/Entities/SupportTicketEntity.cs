using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scanitech_API_DAL.Entities;

[Table("SupportTickets")]
public class SupportTicketEntity
{
    [Key]
    public int Id { get; set; }

    // Fremmednøgle: Hvilken kunde (fra din Customers-tabel) skal have regningen for denne sag?
    public int CustomerId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    // Status kan f.eks. være: "Ny", "I gang", "Afventer Kunde", "Afsluttet"
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Ny";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation property, så Entity Framework forstår relationen til CustomerEntity
    [ForeignKey(nameof(CustomerId))]
    public CustomerEntity? Customer { get; set; }
}