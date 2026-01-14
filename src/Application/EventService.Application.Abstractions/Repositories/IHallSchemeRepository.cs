using EventService.Application.Models.Schemes;

namespace EventService.Application.Abstractions.Repositories;

public interface IHallSchemeRepository : IRepository<HallScheme>
{
    Task<IReadOnlyList<HallScheme>> GetByVenueAsync(long venueId);
}