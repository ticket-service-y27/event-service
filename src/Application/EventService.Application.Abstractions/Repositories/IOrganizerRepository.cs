using EventService.Application.Models.Organizers;

namespace EventService.Application.Abstractions.Repositories;

public interface IOrganizerRepository : IRepository<Organizer>
{
    Task<IReadOnlyList<Organizer>> GetByEventAsync(long eventId);
}