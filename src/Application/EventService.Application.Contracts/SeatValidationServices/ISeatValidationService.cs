namespace EventService.Application.Contracts.SeatValidationServices;

public interface ISeatValidationService
{
    Task<bool> SeatExistsAsync(long hallSchemeId, int row, int seatNumber,  CancellationToken cancellationToken);

    Task<bool> IsSeatAvailableAsync(long hallSchemeId, int row, int seatNumber, CancellationToken cancellationToken);

    Task<string> GetSeatStatusAsync(long hallSchemeId, int row, int seatNumber,  CancellationToken cancellationToken);

    Task BookSeatsAsync(long hallSchemeId, IEnumerable<(int Row, int SeatNumber)> seats,  CancellationToken cancellationToken);

    Task ReturnSeatsAsync(
        long hallSchemeId,
        IEnumerable<(int Row, int SeatNumber)> seats,
        CancellationToken cancellationToken);
}