using EventService.Application.Contracts.VenueManagementServices;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public sealed class VenueManagementServiceGrpc : VenueGrpcService.VenueGrpcServiceBase
{
    private readonly IVenueManagementService _venueService;

    public VenueManagementServiceGrpc(
        IVenueManagementService venueService)
    {
        _venueService = venueService;
    }

    public override async Task<VenueResponse> CreateVenue(
        CreateVenueRequest request,
        ServerCallContext context)
    {
        Venue venue = await _venueService.CreateVenueAsync(
            request.Name,
            request.Address);

        return MapVenue(venue);
    }

    public override async Task<VenueResponse> UpdateVenue(
        UpdateVenueRequest request,
        ServerCallContext context)
    {
        Venue venue = await _venueService.UpdateVenueAsync(
            request.VenueId,
            string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
            string.IsNullOrWhiteSpace(request.Address) ? null : request.Address);

        return MapVenue(venue);
    }

    public override async Task<HallSchemeResponse> AddHallScheme(
        AddHallSchemeRequest request,
        ServerCallContext context)
    {
        HallScheme scheme = await _venueService.AddHallSchemeAsync(
            request.VenueId,
            request.SchemeName,
            request.Rows,
            request.Columns);

        return MapHallScheme(scheme);
    }

    public override async Task<Empty> RemoveHallScheme(
        RemoveHallSchemeRequest request,
        ServerCallContext context)
    {
        await _venueService.RemoveHallSchemeAsync(
            request.HallSchemeId);

        return new Empty();
    }

    public override async Task<HallSchemeResponse> GetHallSchemeById(
        GetHallSchemeByIdRequest request,
        ServerCallContext context)
    {
        HallScheme? scheme =
            await _venueService.GetSchemeAsync(request.HallSchemeId);

        if (scheme is null)
        {
            throw new RpcException(
                new Status(
                    StatusCode.NotFound,
                    $"Hall scheme with id={request.HallSchemeId} not found"));
        }

        return MapHallScheme(scheme);
    }

    public override async Task<HallSchemesList> GetVenueHallSchemes(
        GetVenueHallSchemesRequest request,
        ServerCallContext context)
    {
        IReadOnlyList<HallScheme> schemes =
            await _venueService.GetVenueSchemesAsync(
                request.VenueId);

        var response = new HallSchemesList();
        response.Schemes.AddRange(
            schemes.Select(MapHallScheme));

        return response;
    }

    public override async Task<VenueHasAvailableSchemeResponse>
        VenueHasAvailableScheme(
            VenueHasAvailableSchemeRequest request,
            ServerCallContext context)
    {
        bool hasAvailable =
            await _venueService.VenueHasAvailableSchemeAsync(
                request.VenueId);

        return new VenueHasAvailableSchemeResponse
        {
            HasAvailableScheme = hasAvailable,
        };
    }

    private static VenueResponse MapVenue(Venue venue)
        => new()
        {
            Id = venue.Id,
            Name = venue.Name,
            Address = venue.Address,
        };

    private static HallSchemeResponse MapHallScheme(HallScheme scheme)
        => new()
        {
            Id = scheme.Id,
            VenueId = scheme.VenueId,
            Name = scheme.Name,
            Rows = scheme.Rows,
            Columns = scheme.Columns,
        };
}