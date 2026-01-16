using EventService.Application.Models.Events;

namespace EventService.Presentation.Kafka.Abstractions;

public interface IVenueCreatedPublisher
{
    Task PublishAsync(VenueCreatedEvent evt, CancellationToken ct);
}