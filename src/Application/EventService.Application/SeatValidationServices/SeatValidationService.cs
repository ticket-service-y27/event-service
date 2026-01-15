using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.SeatValidationServices;
using EventService.Application.Models.Schemes;

namespace EventService.Application.SeatValidationServices;

public class SeatValidationService : ISeatValidationService
{
    private readonly IHallSchemeRepository _hallSchemeRepository;
    private readonly ISeatRepository _seatRepository;

    public SeatValidationService(
        IHallSchemeRepository hallSchemeRepository,
        ISeatRepository seatRepository)
    {
        _hallSchemeRepository = hallSchemeRepository;
        _seatRepository = seatRepository;
    }

    public async Task<bool> SeatExistsAsync(long hallSchemeId, int row, int seatNumber)
    {
        HallScheme? scheme = await _hallSchemeRepository.GetByIdAsync(hallSchemeId);

        if (scheme == null)
            return false;

        return row > 0 &&
               seatNumber > 0 &&
               row <= scheme.Rows &&
               seatNumber <= scheme.Columns;
    }

    public async Task<bool> IsSeatAvailableAsync(long hallSchemeId, int row, int seatNumber)
    {
        string? status = await _seatRepository
            .GetStatusAsync(hallSchemeId, row, seatNumber);

        return status == null || status.Equals("Free", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> GetSeatStatusAsync(long hallSchemeId, int row, int seatNumber)
    {
        return await _seatRepository.GetStatusAsync(hallSchemeId, row, seatNumber)
               ?? "Free";
    }

    public async Task BookSeatAsync(long hallSchemeId, int row, int seatNumber)
    {
        if (!await SeatExistsAsync(hallSchemeId, row, seatNumber))
            throw new ArgumentException("Seat does not exist");

        if (!await IsSeatAvailableAsync(hallSchemeId, row, seatNumber))
            throw new InvalidOperationException("Seat already booked");

        await _seatRepository.SetStatusAsync(
            hallSchemeId,
            row,
            seatNumber,
            "Booked");
    }
}