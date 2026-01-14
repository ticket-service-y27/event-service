using EventService.Application.Models.Organizers;

namespace EventService.Application.Abstractions.Repositories;

public interface IOrganizerRepository
{
    Task<Organizer?> GetByIdAsync(long id);

    Task<IReadOnlyList<Organizer>> GetAllAsync();

    Task AddAsync(Organizer entity);

    Task UpdateAsync(Organizer entity);

    Task DeleteAsync(long id);

    Task<IReadOnlyList<Organizer>> GetByEventAsync(long eventId);
}