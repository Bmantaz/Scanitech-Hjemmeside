using Microsoft.EntityFrameworkCore;
using Scanitech_API_DAL.Contexts;
using Scanitech_API_DAL.Entities;
using Scanitech_API_DAL.Interfaces;

namespace Scanitech_API_DAL.Repositories;

/// <summary>
/// Håndterer den direkte database-adgang for Support Tickets.
/// </summary>
public sealed class SupportTicketRepository : ISupportTicketRepository
{
    private readonly AppDbContext _context;

    public SupportTicketRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<int> InsertAsync(SupportTicketEntity entity, CancellationToken ct)
    {
        await _context.SupportTickets.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<SupportTicketEntity?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.SupportTickets
            .Include(t => t.Customer) // Hent også kunde-info med
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<IReadOnlyList<SupportTicketEntity>> GetAllByCustomerIdAsync(int customerId, CancellationToken ct)
    {
        return await _context.SupportTickets
            .AsNoTracking() // Performance-optimering (Vision 5.0) for read-only data
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(SupportTicketEntity entity, CancellationToken ct)
    {
        _context.SupportTickets.Update(entity);
        await _context.SaveChangesAsync(ct);
    }
}