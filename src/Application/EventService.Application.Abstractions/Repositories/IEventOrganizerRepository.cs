using EventService.Application.Models.Organizers;

namespace EventService.Application.Abstractions.Repositories;

public interface IEventOrganizerRepository
{
    Task AddAsync(EventOrganizer entity);
    
    Task RemoveAsync(long eventId, long organizerId);
    
    Task<IReadOnlyList<EventOrganizer>> GetByEventAsync(long eventId);
    
    Task<IReadOnlyList<EventOrganizer>> GetByOrganizerAsync(long organizerId);
}