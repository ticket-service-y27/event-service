using EventService.Application.Contracts.VenueManagementServices;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public sealed class VenueManagementServiceGrpc : VenueManagementGrpcService.VenueManagementGrpcServiceBase
{
    private readonly IVenueManagementService _venueManagementService;

    public VenueManagementServiceGrpc(IVenueManagementService venueManagementService)
    {
        _venueManagementService = venueManagementService;
    }

    public override async Task<VenueResponse> CreateVenue(
        CreateVenueRequest request,
        ServerCallContext context)
    {
        Venue venue = await _venueManagementService.CreateVenueAsync(
            request.Name,
            request.Address);

        return MapVenue(venue);
    }

    public override async Task<VenueResponse> UpdateVenue(
        UpdateVenueRequest request,
        ServerCallContext context)
    {
        Venue venue = await _venueManagementService.UpdateVenueAsync(
            request.VenueId,
            string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
            string.IsNullOrWhiteSpace(request.Address) ? null : request.Address);

        return MapVenue(venue);
    }

    public override async Task<VenueHallSchemeResponse> AddHallScheme(
        AddHallSchemeRequest request,
        ServerCallContext context)
    {
        HallScheme scheme = await _venueManagementService.AddHallSchemeAsync(
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
        await _venueManagementService.RemoveHallSchemeAsync(request.HallSchemeId);
        return new Empty();
    }

    public override async Task<VenueHasAvailableSchemeResponse> VenueHasAvailableScheme(
        VenueHasAvailableSchemeRequest request,
        ServerCallContext context)
    {
        bool hasAvailable = await _venueManagementService
            .VenueHasAvailableSchemeAsync(request.VenueId);

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

    private static VenueHallSchemeResponse MapHallScheme(HallScheme scheme)
        => new()
        {
            Id = scheme.Id,
            VenueId = scheme.VenueId,
            Name = scheme.Name,
            Rows = scheme.Rows,
            Columns = scheme.Columns,
        };
}