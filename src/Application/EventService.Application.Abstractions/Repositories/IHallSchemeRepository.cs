using EventService.Application.Models.Schemes;

namespace EventService.Application.Abstractions.Repositories;

public interface IHallSchemeRepository
{
    Task<HallScheme?> GetByIdAsync(long id);

    Task<IReadOnlyList<HallScheme>> GetAllAsync();

    Task AddAsync(HallScheme entity);

    Task UpdateAsync(HallScheme entity);

    Task DeleteAsync(long id);

    Task<IReadOnlyList<HallScheme>> GetByVenueAsync(long venueId);
}