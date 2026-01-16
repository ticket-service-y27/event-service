using EventService.Application.Contracts.EventManagerServices;
using EventService.Application.Models.EventEntities;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventService.Presentation.Grpc.Services;

public class EventManagerServiceGrpc : EventManagerGrpcService.EventManagerGrpcServiceBase
{
    private readonly IEventManagerService _eventManagerService;

    public EventManagerServiceGrpc(IEventManagerService eventManagerService)
    {
        _eventManagerService = eventManagerService;
    }

    public override async Task<EventResponse> CreateEvent(CreateEventRequest request, ServerCallContext context)
    {
        try
        {
            EventEntity created = await _eventManagerService.CreateEventAsync(
                organizerId: request.OrganizerId,
                title: request.Title,
                description: request.Description,
                startDate: request.StartDate.ToDateTime(),
                endDate: request.EndDate.ToDateTime(),
                categoryId: request.CategoryId,
                venueId: request.VenueId,
                cancellationToken: context.CancellationToken);

            return Map(created);
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
    }

    public override async Task<EventResponse> UpdateEvent(UpdateEventRequest request, ServerCallContext context)
    {
        try
        {
            EventEntity updated = await _eventManagerService.UpdateEventAsync(
                organizerId: request.OrganizerId,
                eventId: request.EventId,
                cancellationToken: context.CancellationToken,
                title: string.IsNullOrWhiteSpace(request.Title) ? null : request.Title,
                description: string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
                startDate: request.StartDate != null && request.StartDate.Seconds != 0 ? request.StartDate.ToDateTime() : null,
                endDate: request.EndDate != null && request.EndDate.Seconds != 0 ? request.EndDate.ToDateTime() : null,
                categoryId: request.CategoryId == 0 ? null : request.CategoryId,
                venueId: request.VenueId == 0 ? null : request.VenueId);

            return Map(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, ex.Message));
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

    private static EventResponse Map(EventEntity e)
    {
        return new EventResponse
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartDate = Timestamp.FromDateTime(e.StartDate.ToUniversalTime()),
            EndDate = Timestamp.FromDateTime(e.EndDate.ToUniversalTime()),
            CategoryId = e.CategoryId,
            VenueId = e.VenueId,
        };
    }
}