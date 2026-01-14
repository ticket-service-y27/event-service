using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Venues;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class VenueRepository : Repository<Venue>, IVenueRepository
{
    private readonly EventDbContext _context;

    public VenueRepository(EventDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> HasHallSchemesAsync(long venueId)
    {
        return await _context.HallSchemes
            .AnyAsync(h => h.VenueId == venueId);
    }
}