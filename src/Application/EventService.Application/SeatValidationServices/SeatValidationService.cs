using EventService.Application.Abstractions.Messaging;
using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.SeatValidationServices;
using EventService.Application.Models.Events;
using EventService.Application.Models.Schemes;

namespace EventService.Application.SeatValidationServices;

public class SeatValidationService : ISeatValidationService
{
    private readonly IHallSchemeRepository _hallSchemeRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ISeatBookedPublisher _seatBookedPublisher;

    public SeatValidationService(
        IHallSchemeRepository hallSchemeRepository,
        ISeatRepository seatRepository,
        ISeatBookedPublisher seatBookedPublisher)
    {
        _hallSchemeRepository = hallSchemeRepository;
        _seatRepository = seatRepository;
        _seatBookedPublisher = seatBookedPublisher;
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

    public async Task BookSeatsAsync(long hallSchemeId, IEnumerable<(int Row, int SeatNumber)> seats)
    {
        var seatList = seats.ToList();

        if (seatList.Count == 0)
            throw new ArgumentException("No seats provided");

        int seatCount = seatList.Count;

        foreach ((int row, int seatNumber) in seatList)
        {
            if (!await SeatExistsAsync(hallSchemeId, row, seatNumber))
                throw new ArgumentException($"Seat does not exist: row {row}, seat {seatNumber}");
            seatCount--;
        }

        foreach ((int row, int seatNumber) in seatList)
        {
            if (!await IsSeatAvailableAsync(hallSchemeId, row, seatNumber))
                throw new InvalidOperationException($"Seat already booked: row {row}, seat {seatNumber}");
            seatCount--;
        }

        foreach ((int row, int seatNumber) in seatList)
        {
            await _seatRepository.SetStatusAsync(
                hallSchemeId,
                row,
                seatNumber,
                "Booked");
        }

        await _seatBookedPublisher.PublishAsync(
            new SeatBookedEvent(
                HallSchemeId: hallSchemeId,
                BookedSeats: seatCount),
            CancellationToken.None);
    }
}