namespace EventService.Application.Models.Events;

public sealed record SeatBookedEvent(long HallSchemeId, int BookedSeats);