namespace EventService.Application.Abstractions.Repositories;

public interface ISeatRepository
{
    Task<string?> GetStatusAsync(long hallSchemeId, int row, int seatNumber);

    Task SetStatusAsync(long hallSchemeId, int row, int seatNumber, string status);
}