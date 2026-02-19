using Microsoft.EntityFrameworkCore;
using Scanitech_API_DAL.Interfaces;
using Scanitech_API_DAL.Entities;
using Scanitech_API_DAL.Contexts;

namespace Scanitech_API_DAL.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<CustomerEntity?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Customers
            .AsNoTracking() // Optimering til læse-operationer
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<CustomerEntity>> GetAllAsync(CancellationToken ct)
    {
        var result = await _context.Customers.AsNoTracking().ToListAsync(ct);
        return result.AsReadOnly();
    }

    public async Task<int> InsertAsync(CustomerEntity entity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _context.Customers.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);

        return entity.Id;
    }
}