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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API konfiguration for bedre kontrol og sikkerhed
        modelBuilder.Entity<CustomerEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        });

        // VISION 5.0: Automatisk Data Seeding til dine udvikler-maskiner
        modelBuilder.Entity<CustomerEntity>().HasData(
            new CustomerEntity { Id = 1, Name = "Test Person 1 (Arbejde)", Email = "test1@scanitech.dk", CreatedAt = new DateTime(2025, 1, 1) },
            new CustomerEntity { Id = 2, Name = "Test Person 2 (Hjemme)", Email = "test2@scanitech.dk", CreatedAt = new DateTime(2025, 1, 1) }
        );
    }
}