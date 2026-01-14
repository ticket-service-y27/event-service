using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Events;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class EventRepository : Repository<EventEntity>, IEventRepository
{
    private readonly EventDbContext _context;

    public EventRepository(EventDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EventEntity>> GetByCategoryAsync(long categoryId)
    {
        return await _context.Events
            .Where(e => e.CategoryId == categoryId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EventEntity>> GetByVenueAsync(long venueId)
    {
        return await _context.Events
            .Where(e => e.VenueId == venueId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EventEntity>> GetByDateRangeAsync(DateTime left, DateTime right)
    {
        return await _context.Events
            .Where(e => e.StartDate >= left && e.StartDate <= right)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(long eventId)
    {
        return await _context.Events
            .AnyAsync(e => e.Id == eventId);
    }
}