using EventService.Application.Contracts.SeatValidationServices;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public sealed class SeatValidationServiceGrpc : SeatValidationGrpcService.SeatValidationGrpcServiceBase
{
    private readonly ISeatValidationService _seatValidationService;

    public SeatValidationServiceGrpc(ISeatValidationService seatValidationService)
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
            request.SeatNumber);

        return new SeatExistsResponse
        {
            Exists = exists,
        };
    }

    public override async Task<SeatAvailableResponse> IsSeatAvailable(
        IsSeatAvailableRequest request,
        ServerCallContext context)
    {
        bool available = await _seatValidationService.IsSeatAvailableAsync(
            request.EventId,
            request.Row,
            request.SeatNumber);

        return new SeatAvailableResponse
        {
            Available = available,
        };
    }

    public override async Task<SeatStatusResponse> GetSeatStatus(
        GetSeatStatusRequest request,
        ServerCallContext context)
    {
        string status = await _seatValidationService.GetSeatStatusAsync(
            request.EventId,
            request.Row,
            request.SeatNumber);

        return new SeatStatusResponse
        {
            Status = status,
        };
    }
}