using EventService.Application.Models.EventEntities;

namespace EventService.Application.Abstractions.Repositories;

public interface IEventRepository
{
    Task<EventEntity?> GetByIdAsync(long id);

    Task<IReadOnlyList<EventEntity>> GetAllAsync();

    Task AddAsync(EventEntity entity);

    Task UpdateAsync(EventEntity entity);

    Task DeleteAsync(long id);

    Task<IReadOnlyList<EventEntity>> GetByCategoryAsync(long categoryId);

    Task<IReadOnlyList<EventEntity>> GetByVenueAsync(long venueId);

    Task<IReadOnlyList<EventEntity>> GetByDateRangeAsync(DateTime left, DateTime right);

    Task<bool> ExistsAsync(long eventId);
}