using EventService.Application.Models.EventEntities;

namespace EventService.Application.Contracts.EventManagerServices;

public interface IEventManagerService
{
    Task<EventEntity> CreateEventAsync(
        long organizerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        long categoryId,
        long venueId);

    Task<EventEntity> UpdateEventAsync(
        long organizerId,
        long eventId,
        string? title = null,
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        long? categoryId = null,
        long? venueId = null);

    Task<bool> CanEditEventAsync(long organizerId, long eventId);

    Task<bool> IsAdminAsync(long userId);
}