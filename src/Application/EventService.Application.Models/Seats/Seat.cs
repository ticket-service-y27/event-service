using EventService.Application.Models.Statuses;

namespace EventService.Application.Models.Seats;

public record Seat(int Row, int Number, SeatStatus Status);