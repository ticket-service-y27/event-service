using EventService.Application.Models.Events;

namespace EventService.Application.Abstractions.Messaging;

public interface ISeatReturnedPublisher
{
    Task PublishAsync(SeatReturnedEvent evt, CancellationToken ct);
}