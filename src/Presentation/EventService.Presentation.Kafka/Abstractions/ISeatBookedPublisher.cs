using EventService.Application.Models.Events;

namespace EventService.Presentation.Kafka.Abstractions;

public interface ISeatBookedPublisher
{
    Task PublishAsync(SeatBookedEvent evt, CancellationToken ct);
}