using EventService.Application.Contracts.HallSchemeServices;
using EventService.Application.Models.Schemes;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public sealed class HallSchemeServiceGrpc : HallSchemeGrpcService.HallSchemeGrpcServiceBase
{
    private readonly IHallSchemeService _hallSchemeService;

    public HallSchemeServiceGrpc(IHallSchemeService hallSchemeService)
    {
        _hallSchemeService = hallSchemeService;
    }

    public override async Task<HallSchemeResponse> GetHallSchemeById(
        GetHallSchemeByIdRequest request,
        ServerCallContext context)
    {
        HallScheme? scheme =
            await _hallSchemeService.GetSchemeAsync(request.HallSchemeId);

        if (scheme is null)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    $"Hall scheme with id={request.HallSchemeId} not found"));
        }

        return MapToResponse(scheme);
    }

    public override async Task<HallSchemesList> GetVenueHallSchemes(
        GetVenueHallSchemesRequest request,
        ServerCallContext context)
    {
        IReadOnlyList<HallScheme> schemes =
            await _hallSchemeService.GetVenueSchemesAsync(request.VenueId);

        var response = new HallSchemesList();
        response.Schemes.AddRange(schemes.Select(MapToResponse));

        return response;
    }

    private static HallSchemeResponse MapToResponse(HallScheme scheme)
        => new()
        {
            Id = scheme.Id,
            VenueId = scheme.VenueId,
            Name = scheme.Name,
            Rows = scheme.Rows,
            Columns = scheme.Columns,
        };
}