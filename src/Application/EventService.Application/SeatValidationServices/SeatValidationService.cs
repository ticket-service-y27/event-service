using EventService.Application.Abstractions.Messaging;
using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.SeatValidationServices;
using EventService.Application.Models.Events;
using EventService.Application.Models.Schemes;
using System.Transactions;

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

    public async Task<bool> SeatExistsAsync(
        long hallSchemeId,
        int row,
        int seatNumber,
        CancellationToken cancellationToken)
    {
        HallScheme? scheme = await _hallSchemeRepository.GetByIdAsync(hallSchemeId, cancellationToken);
        if (scheme == null) return false;

        return row > 0 &&
               seatNumber > 0 &&
               row <= scheme.Rows &&
               seatNumber <= scheme.Columns;
    }

    public async Task<bool> IsSeatAvailableAsync(
        long hallSchemeId,
        int row,
        int seatNumber,
        CancellationToken cancellationToken)
    {
        string? status = await _seatRepository.GetStatusAsync(hallSchemeId, row, seatNumber, cancellationToken);
        return status == null || status.Equals("Free", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> GetSeatStatusAsync(
        long hallSchemeId,
        int row,
        int seatNumber,
        CancellationToken cancellationToken)
    {
        return await _seatRepository.GetStatusAsync(hallSchemeId, row, seatNumber, cancellationToken) ?? "Unknown";
    }

    public async Task BookSeatsAsync(
        long hallSchemeId,
        IEnumerable<(int Row, int SeatNumber)> seats,
        CancellationToken cancellationToken)
    {
        var seatList = seats.ToList();

        if (seatList.Count == 0)
            throw new ArgumentException("No seats provided");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        foreach ((int row, int seatNumber) in seatList)
        {
            if (!await SeatExistsAsync(hallSchemeId, row, seatNumber, cancellationToken))
                throw new ArgumentException($"Seat does not exist: row {row}, seat {seatNumber}");

            if (!await IsSeatAvailableAsync(hallSchemeId, row, seatNumber, cancellationToken))
                throw new InvalidOperationException($"Seat already booked: row {row}, seat {seatNumber}");
        }

        foreach ((int row, int seatNumber) in seatList)
        {
            await _seatRepository.SetStatusAsync(hallSchemeId, row, seatNumber, "Booked", cancellationToken);
        }

        scope.Complete();

        await _seatBookedPublisher.PublishAsync(
            new SeatBookedEvent(
                HallSchemeId: hallSchemeId,
                BookedSeats: seatList.Count),
            CancellationToken.None);
    }
}