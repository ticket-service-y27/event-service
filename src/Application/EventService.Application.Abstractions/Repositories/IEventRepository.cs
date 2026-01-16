using EventService.Application.Models.EventEntities;

namespace EventService.Application.Abstractions.Repositories;

public interface IEventRepository
{
    Task<EventEntity?> GetByIdAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<EventEntity> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(EventEntity entity, CancellationToken cancellationToken);

    Task UpdateAsync(EventEntity entity, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<EventEntity>> GetByCategoryAsync(long categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<EventEntity>> GetByVenueAsync(long venueId, CancellationToken cancellationToken);

    Task<IReadOnlyList<EventEntity>> GetByDateRangeAsync(DateTime left, DateTime right, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(long eventId, CancellationToken cancellationToken);
}