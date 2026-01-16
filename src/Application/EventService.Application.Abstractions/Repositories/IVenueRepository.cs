using EventService.Application.Models.Venues;

namespace EventService.Application.Abstractions.Repositories;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken);

    Task<long> AddAsync(Venue entity, CancellationToken cancellationToken);

    Task UpdateAsync(Venue entity, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    Task<bool> HasHallSchemesAsync(long venueId, CancellationToken cancellationToken);
}