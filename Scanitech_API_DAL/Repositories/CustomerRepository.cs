using Microsoft.EntityFrameworkCore;
using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_DAL.Contexts;

namespace Scanitech_API_DAL.Repositories;

/// <summary>
/// Entity Framework Core implementering af ICustomerRepository.
/// </summary>
/// <remarks>
/// Arbejder udelukkende med database-I/O. Indeholder ingen forretningslogik (SRP).
/// </remarks>
public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        // Fail-fast beskyttelse mod manglende afhængigheder via Dependency Injection
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc/>
    public async Task<CustomerEntity?> GetByIdAsync(int id, CancellationToken ct)
    {
        // WHY: AsNoTracking bruges fordi vi kun læser data (Read-Only). Det sparer EF Core 
        // for at opbygge et overvågningstræ i hukommelsen, hvilket øger performance markant.
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CustomerEntity>> GetAllAsync(CancellationToken ct)
    {
        // WHY: List<T> implementerer IReadOnlyList<T>. Vi returnerer ToListAsync() direkte 
        // for at spare en unødvendig hukommelsesallokering (Allocation-free optimering).
        return await _context.Customers
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<int> InsertAsync(CustomerEntity entity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _context.Customers.AddAsync(entity, ct);

        // Gemmer ændringer i databasen asynkront. EF Core håndterer automatisk 
        // transaktionen bagved, hvis der var flere objekter der skulle gemmes.
        await _context.SaveChangesAsync(ct);

        // Efter SaveChangesAsync har EF Core automatisk populært 'Id' feltet fra LocalDB
        return entity.Id;
    }
}