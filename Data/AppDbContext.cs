using Microsoft.EntityFrameworkCore;
using Scanitech_Hjemmeside.Models;

namespace Scanitech_Hjemmeside.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vision 5.0: Fluent API Konfiguration
            modelBuilder.Entity<Customer>(entity =>
            {
                // Sikrer lynhurtig søgning på email og forhindrer dubletter
                entity.HasIndex(e => e.Email).IsUnique();

                // Eksempel på default værdier (Postgres kompatibel)
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }

        // Automatisk opdatering af UpdatedAt ved ændringer
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Customer && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((Customer)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

                if (entityEntry.State == EntityState.Added)
                {
                    ((Customer)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}