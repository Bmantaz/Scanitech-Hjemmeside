using Microsoft.EntityFrameworkCore;
using Scanitech_API_DAL.Entities;

namespace Scanitech_API_DAL.Contexts;

/// <summary>
/// Database kontekst for Scanitech API (Vision 5.0 - LocalDB/SQL Server).
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CustomerEntity> Customers => Set<CustomerEntity>();
    public DbSet<SupportTicketEntity> SupportTickets => Set<SupportTicketEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --------------------------------------------------------
        // FLUENT API: CUSTOMER ENTITY
        // --------------------------------------------------------
        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);

            // Vision 5.0 - Faktureringsfelter
            entity.Property(e => e.Address).IsRequired().HasMaxLength(250);
            entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CVR).HasMaxLength(50); // Optional

            // Sikkerhed: Standard altid falsk i DB-laget også
            entity.Property(e => e.IsApproved).HasDefaultValue(false);

            // Datoer
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // --------------------------------------------------------
        // FLUENT API: SUPPORT TICKET ENTITY
        // --------------------------------------------------------
        modelBuilder.Entity<SupportTicketEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Ny");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relation til Customer: Hvis en kunde slettes, slettes tickets IKKE automatisk (forhindrer tab af fakturering)
            entity.HasOne(t => t.Customer)
                  .WithMany()
                  .HasForeignKey(t => t.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --------------------------------------------------------
        // DATA SEEDING (Udvikler-data)
        // --------------------------------------------------------
        modelBuilder.Entity<CustomerEntity>().HasData(
            new CustomerEntity
            {
                Id = 1,
                Name = "Test Person 1 (Arbejde)",
                Email = "test1@scanitech.dk",
                Address = "Teknologivej 1",
                PostalCode = "8000",
                City = "Aarhus C",
                CVR = "12345678",
                IsApproved = true, // Godkendt: Kan oprette tickets
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new CustomerEntity
            {
                Id = 2,
                Name = "Test Person 2 (Hjemme)",
                Email = "test2@scanitech.dk",
                Address = "Hjemmevej 42",
                PostalCode = "9000",
                City = "Aalborg",
                CVR = null,
                IsApproved = false, // Ikke godkendt: Bliver afvist i BLL
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}