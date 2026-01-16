using EventService.Application.Models.Events;

namespace EventService.Application.Abstractions.Messaging;

public interface ISeatBookedPublisher
{
    Task PublishAsync(SeatBookedEvent evt, CancellationToken ct);
}