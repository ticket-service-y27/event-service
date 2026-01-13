using EventService.Application.Models.Artists;

namespace EventService.Application.Abstractions.Repositories;

public interface IArtistRepository : IRepository<Artist>
{
    Task<IReadOnlyList<Artist>> GetByEventAsync(long eventId);
}