using EventService.Application.Models.Events;

namespace EventService.Application.Abstractions.Repositories;

public interface IEventRepository : IRepository<EventEntity>
{
    Task<IReadOnlyList<EventEntity>> GetByCategoryAsync(long categoryId);

    Task<IReadOnlyList<EventEntity>> GetByVenueAsync(long venueId);

    Task<IReadOnlyList<EventEntity>> GetByDateRangeAsync(DateTime left, DateTime right);

    Task<bool> ExistsAsync(long eventId);
}