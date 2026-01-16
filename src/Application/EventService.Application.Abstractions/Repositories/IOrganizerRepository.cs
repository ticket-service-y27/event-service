using EventService.Application.Models.Organizers;

namespace EventService.Application.Abstractions.Repositories;

public interface IOrganizerRepository
{
    Task<Organizer?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Organizer>> GetAllAsync(CancellationToken cancellationToken);

    Task<long> AddAsync(Organizer entity, CancellationToken cancellationToken);

    Task UpdateAsync(Organizer entity, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Organizer> GetByEventAsync(long eventId, CancellationToken cancellationToken);
}