using EventService.Application.Contracts.SeatValidationServices;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public sealed class SeatValidationServiceGrpc
    : SeatValidationGrpcService.SeatValidationGrpcServiceBase
{
    private readonly ISeatValidationService _seatValidationService;

    public SeatValidationServiceGrpc(
        ISeatValidationService seatValidationService)
    {
        _seatValidationService = seatValidationService;
    }

    public override async Task<SeatExistsResponse> SeatExists(
        SeatExistsRequest request,
        ServerCallContext context)
    {
        bool exists = await _seatValidationService.SeatExistsAsync(
            request.HallSchemeId,
            request.Row,
            request.SeatNumber,
            context.CancellationToken);

        return new SeatExistsResponse { Exists = exists };
    }

    public override async Task<SeatAvailableResponse> IsSeatAvailable(
        IsSeatAvailableRequest request,
        ServerCallContext context)
    {
        bool available = await _seatValidationService.IsSeatAvailableAsync(
            request.HallSchemeId,
            request.Row,
            request.SeatNumber,
            context.CancellationToken);

        return new SeatAvailableResponse { Available = available };
    }

    public override async Task<SeatStatusResponse> GetSeatStatus(
        GetSeatStatusRequest request,
        ServerCallContext context)
    {
        string status = await _seatValidationService.GetSeatStatusAsync(
            request.HallSchemeId,
            request.Row,
            request.SeatNumber,
            context.CancellationToken);

        return new SeatStatusResponse { Status = status };
    }

    public override async Task<BookSeatsResponse> BookSeats(
        BookSeatsRequest request,
        ServerCallContext context)
    {
        IEnumerable<(int Row, int SeatNumber)> seats = request.Seats.Select(s => (s.Row, s.SeatNumber));
        await _seatValidationService.BookSeatsAsync(
            request.HallSchemeId,
            seats,
            context.CancellationToken);

        return new BookSeatsResponse { Success = true };
    }

    public override async Task<ReturnSeatsResponse> ReturnSeats(
        ReturnSeatsRequest request,
        ServerCallContext context)
    {
        IEnumerable<(int Row, int SeatNumber)> seats = request.Seats.Select(s => (s.Row, s.SeatNumber));
        await _seatValidationService.ReturnSeatsAsync(
            request.HallSchemeId,
            seats,
            context.CancellationToken);

        return new ReturnSeatsResponse { Success = true };
    }
}