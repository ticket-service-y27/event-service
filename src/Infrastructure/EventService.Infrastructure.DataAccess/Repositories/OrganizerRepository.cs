using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Organizers;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class OrganizerRepository : Repository<Organizer>, IOrganizerRepository
{
    private readonly EventDbContext _context;

    public OrganizerRepository(EventDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Organizer>> GetByEventAsync(long eventId)
    {
        return await _context.Organizers
            .Where(o => o.Events.Any(e => e.EventId == eventId))
            .AsNoTracking()
            .ToListAsync();
    }
}