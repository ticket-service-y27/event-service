using EventService.Application.Models.Events;

namespace EventService.Presentation.Kafka.Abstractions;

public interface IEventCreatedPublisher
{
    Task PublishAsync(EventCreatedEvent evt, CancellationToken ct);
}