using EventService.Application.Models.Schemes;

namespace EventService.Application.Abstractions.Repositories;

public interface IHallSchemeRepository
{
    Task<HallScheme?> GetByIdAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<HallScheme> GetAllAsync(CancellationToken cancellationToken);

    Task<long> AddAsync(HallScheme entity, CancellationToken cancellationToken);

    Task UpdateAsync(HallScheme entity, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<HallScheme> GetByVenueAsync(long venueId, CancellationToken cancellationToken);
}