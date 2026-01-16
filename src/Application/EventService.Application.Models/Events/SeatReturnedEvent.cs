namespace EventService.Application.Models.Events;

public sealed record SeatReturnedEvent(long HallSchemeId, int ReturnedSeats);