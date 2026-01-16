namespace EventService.Application.Models.Events;

public sealed record EventCreatedEvent(
    long EventId,
    long ArtistId,
    int TotalSeats,
    DateTimeOffset EventDate,
    long VenueId);