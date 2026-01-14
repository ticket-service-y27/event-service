using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Schemes;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class HallSchemeRepository : Repository<HallScheme>, IHallSchemeRepository
{
    private readonly EventDbContext _context;

    public HallSchemeRepository(EventDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<HallScheme>> GetByVenueAsync(long venueId)
    {
        return await _context.HallSchemes
            .Where(x => x.VenueId == venueId)
            .AsNoTracking()
            .ToListAsync();
    }
}