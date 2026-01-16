using EventService.Application.Models.Artists;

namespace EventService.Application.Abstractions.Repositories;

public interface IArtistRepository
{
    Task<Artist?> GetByIdAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Artist> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(Artist entity, CancellationToken cancellationToken);

    Task UpdateAsync(Artist entity, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Artist> GetByEventAsync(long eventId, CancellationToken cancellationToken);
}