namespace EventService.Application.Contracts.SeatValidationServices;

public interface ISeatValidationService
{
    Task<bool> SeatExistsAsync(long hallSchemeId, int row, int seatNumber);

    Task<bool> IsSeatAvailableAsync(long hallSchemeId, int row, int seatNumber);

    Task<string> GetSeatStatusAsync(long hallSchemeId, int row, int seatNumber);

    Task BookSeatsAsync(long hallSchemeId, IEnumerable<(int Row, int SeatNumber)> seats);
}