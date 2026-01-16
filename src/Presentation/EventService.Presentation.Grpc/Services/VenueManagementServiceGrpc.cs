using EventService.Application.Contracts.VenueManagementServices;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public sealed class VenueManagementServiceGrpc : VenueGrpcService.VenueGrpcServiceBase
{
    private readonly IVenueManagementService _venueService;

    public VenueManagementServiceGrpc(IVenueManagementService venueService)
    {
        _venueService = venueService;
    }

    public override async Task<VenueResponse> CreateVenue(CreateVenueRequest request, ServerCallContext context)
    {
        try
        {
            Venue venue = await _venueService.CreateVenueAsync(
                request.Name,
                request.Address,
                context.CancellationToken);

            return MapVenue(venue);
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<VenueResponse> UpdateVenue(UpdateVenueRequest request, ServerCallContext context)
    {
        try
        {
            Venue venue = await _venueService.UpdateVenueAsync(
                request.VenueId,
                context.CancellationToken,
                string.IsNullOrWhiteSpace(request.Name) ? null : request.Name,
                string.IsNullOrWhiteSpace(request.Address) ? null : request.Address);

            return MapVenue(venue);
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<HallSchemeResponse> AddHallScheme(AddHallSchemeRequest request, ServerCallContext context)
    {
        try
        {
            HallScheme scheme = await _venueService.AddHallSchemeAsync(
                request.VenueId,
                request.SchemeName,
                request.Rows,
                request.Columns,
                context.CancellationToken);

            return MapHallScheme(scheme);
        }
        catch (ArgumentException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<Empty> RemoveHallScheme(RemoveHallSchemeRequest request, ServerCallContext context)
    {
        try
        {
            await _venueService.RemoveHallSchemeAsync(request.HallSchemeId, context.CancellationToken);
            return new Empty();
        }
        catch (KeyNotFoundException ex)
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<HallSchemeResponse> GetHallSchemeById(GetHallSchemeByIdRequest request, ServerCallContext context)
    {
        HallScheme? scheme = await _venueService.GetSchemeAsync(request.HallSchemeId, context.CancellationToken);
        if (scheme == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Hall scheme {request.HallSchemeId} not found"));
        }

        return MapHallScheme(scheme);
    }

    public override async Task<HallSchemesList> GetVenueHallSchemes(GetVenueHallSchemesRequest request, ServerCallContext context)
    {
        IReadOnlyList<HallScheme> schemes = await _venueService.GetVenueSchemesAsync(request.VenueId, context.CancellationToken);

        var response = new HallSchemesList();
        response.Schemes.AddRange(schemes.Select(MapHallScheme));
        return response;
    }

    public override async Task<VenueHasAvailableSchemeResponse> VenueHasAvailableScheme(VenueHasAvailableSchemeRequest request, ServerCallContext context)
    {
        bool hasAvailable = await _venueService.VenueHasAvailableSchemeAsync(request.VenueId, context.CancellationToken);
        return new VenueHasAvailableSchemeResponse { HasAvailableScheme = hasAvailable };
    }

    private static VenueResponse MapVenue(Venue venue) => new()
    {
        Id = venue.Id,
        Name = venue.Name,
        Address = venue.Address,
    };

    private static HallSchemeResponse MapHallScheme(HallScheme scheme) => new()
    {
        Id = scheme.Id,
        VenueId = scheme.VenueId,
        Name = scheme.Name,
        Rows = scheme.Rows,
        Columns = scheme.Columns,
    };
}