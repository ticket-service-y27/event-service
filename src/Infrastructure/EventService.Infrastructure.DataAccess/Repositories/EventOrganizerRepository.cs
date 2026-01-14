using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Organizers;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class EventOrganizerRepository : IEventOrganizerRepository
{
    private readonly EventDbContext _context;

    public EventOrganizerRepository(EventDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EventOrganizer entity)
    {
        await _context.EventOrganizers.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(long eventId, long organizerId)
    {
        EventOrganizer? entity = await _context.EventOrganizers
            .FirstOrDefaultAsync(e =>
                e.EventId == eventId &&
                e.OrganizerId == organizerId);

        if (entity == null)
            return;

        _context.EventOrganizers.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<EventOrganizer>> GetByEventAsync(long eventId)
    {
        return await _context.EventOrganizers
            .Where(e => e.EventId == eventId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<EventOrganizer>> GetByOrganizerAsync(long organizerId)
    {
        return await _context.EventOrganizers
            .Where(e => e.OrganizerId == organizerId)
            .ToListAsync();
    }
}