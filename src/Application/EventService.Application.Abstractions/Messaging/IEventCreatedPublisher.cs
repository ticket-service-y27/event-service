using EventService.Application.Models.Events;

namespace EventService.Application.Abstractions.Messaging;

public interface IEventCreatedPublisher
{
    Task PublishAsync(EventCreatedEvent evt, CancellationToken ct);
}