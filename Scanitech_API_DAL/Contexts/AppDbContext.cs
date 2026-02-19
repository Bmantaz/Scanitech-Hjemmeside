using Microsoft.EntityFrameworkCore;
using Scanitech_API_DAL.Entities;
using System.Reflection.Emit;

namespace Scanitech_API_DAL.Contexts;

/// <summary>
/// Database kontekst for Scanitech API (Vision 5.0).
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
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });
    }
}