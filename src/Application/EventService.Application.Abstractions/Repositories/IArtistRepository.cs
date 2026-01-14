using EventService.Application.Models.Artists;

namespace EventService.Application.Abstractions.Repositories;

public interface IArtistRepository
{
    Task<Artist?> GetByIdAsync(long id);

    Task<IReadOnlyList<Artist>> GetAllAsync();

    Task AddAsync(Artist entity);

    Task UpdateAsync(Artist entity);

    Task DeleteAsync(long id);

    Task<IReadOnlyList<Artist>> GetByEventAsync(long eventId);
}