namespace EventService.Application.Contracts.SeatValidationServices;

public interface ISeatValidationService
{
    Task<bool> SeatExistsAsync(long hallSchemeId, int row, int seatNumber);
    
    Task<bool> IsSeatAvailableAsync(long eventId, int row, int seatNumber);
    
    Task<string> GetSeatStatusAsync(long eventId, int row, int seatNumber);
}