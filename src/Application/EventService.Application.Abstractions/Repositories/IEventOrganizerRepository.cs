using EventService.Application.Models.Organizers;

namespace EventService.Application.Abstractions.Repositories;

public interface IEventOrganizerRepository
{
    Task AddAsync(EventOrganizer entity, CancellationToken cancellationToken);

    Task RemoveAsync(long eventId, long organizerId, CancellationToken cancellationToken);

    IAsyncEnumerable<EventOrganizer> GetByEventAsync(long eventId, CancellationToken cancellationToken);

    Task<IReadOnlyList<EventOrganizer>> GetByOrganizerAsync(long organizerId, CancellationToken cancellationToken);
}