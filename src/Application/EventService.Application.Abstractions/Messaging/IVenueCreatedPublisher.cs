using EventService.Application.Models.Events;

namespace EventService.Application.Abstractions.Messaging;

public interface IVenueCreatedPublisher
{
    Task PublishAsync(VenueCreatedEvent evt, CancellationToken ct);
}