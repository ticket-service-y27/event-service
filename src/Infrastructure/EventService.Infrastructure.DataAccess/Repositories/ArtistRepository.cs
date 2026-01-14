using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Artists;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class ArtistRepository : Repository<Artist>, IArtistRepository
{
    private readonly EventDbContext _context;

    public ArtistRepository(EventDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Artist>> GetByEventAsync(long eventId)
    {
        return await _context.Artists
            .Where(a => a.Events.Any(e => e.Id == eventId))
            .AsNoTracking()
            .ToListAsync();
    }
}