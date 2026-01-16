namespace EventService.Application.Models.Events;

public sealed record VenueCreatedEvent(long VenueId, int TotalSeats, string Address);