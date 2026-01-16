namespace EventService.Application.Models.Events;

public sealed record SeatBookedEvent(long EventId, int BookedSeats);