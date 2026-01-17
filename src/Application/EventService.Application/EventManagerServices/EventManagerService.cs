using EventService.Application.Abstractions.Messaging;
using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.EventManagerServices;
using EventService.Application.Models.Artists;
using EventService.Application.Models.Categories;
using EventService.Application.Models.EventEntities;
using EventService.Application.Models.Events;
using EventService.Application.Models.Organizers;
using EventService.Application.Models.Venues;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Transactions;

namespace EventService.Application.EventManagerServices;

public class EventManagerService : IEventManagerService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IEventOrganizerRepository _eventOrganizerRepository;
    private readonly IOrganizerRepository _organizerRepository;
    private readonly IEventCreatedPublisher _eventCreatedPublisher;

    public EventManagerService(
        IEventRepository eventRepository,
        ICategoryRepository categoryRepository,
        IVenueRepository venueRepository,
        IEventOrganizerRepository eventOrganizerRepository,
        IOrganizerRepository organizerRepository,
        IEventCreatedPublisher eventCreatedPublisher)
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
        _venueRepository = venueRepository;
        _eventOrganizerRepository = eventOrganizerRepository;
        _organizerRepository = organizerRepository;
        _eventCreatedPublisher = eventCreatedPublisher;
    }

    public async Task<EventEntity> CreateEventAsync(
        long organizerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        long categoryId,
        long venueId,
        CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(
            TransactionScopeAsyncFlowOption.Enabled);

        Category category =
            await _categoryRepository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new Exception("Category not found");

        Venue venue =
            await _venueRepository.GetByIdAsync(venueId, cancellationToken)
            ?? throw new Exception("Venue not found");

        if (startDate >= endDate)
            throw new Exception("StartDate must be earlier than EndDate");

        var organizers = new Collection<EventOrganizer>();
        var artists = new Collection<Artist>();

        var entity = new EventEntity(
            Id: 0,
            Title: title,
            Description: description,
            StartDate: startDate,
            EndDate: endDate,
            CategoryId: categoryId,
            Category: category,
            VenueId: venueId,
            Venue: venue,
            Organizers: organizers,
            Artists: artists);

        entity = entity with { Id = await _eventRepository.AddAsync(entity, cancellationToken) };

        Organizer organizer =
            await _organizerRepository.GetByIdAsync(organizerId, cancellationToken)
            ?? throw new Exception("Organizer not found");

        var link = new EventOrganizer(
            Id: 0,
            EventId: entity.Id,
            EventEntity: entity,
            OrganizerId: organizerId,
            Organizer: organizer);

        link = link with { Id = await _eventOrganizerRepository.AddAsync(link, cancellationToken) };
        organizers.Add(link);

        scope.Complete();

        await _eventCreatedPublisher.PublishAsync(
            new EventCreatedEvent(
                EventId: entity.Id,
                ArtistId: link.OrganizerId,
                TotalSeats: entity.Venue.HallSchemes.Sum(h => h.Rows * h.Columns),
                EventDate: entity.StartDate,
                VenueId: venueId),
            CancellationToken.None);

        return entity;
    }

    public async Task<EventEntity> UpdateEventAsync(
        long organizerId,
        long eventId,
        CancellationToken cancellationToken,
        string? title = null,
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        long? categoryId = null,
        long? venueId = null)
    {
        using var scope = new TransactionScope(
            TransactionScopeAsyncFlowOption.Enabled);

        EventEntity ev =
            await _eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new Exception("Event not found");

        if (!await CanEditEventAsync(organizerId, eventId, cancellationToken))
            throw new UnauthorizedAccessException("User cannot edit this event");

        EventEntity updated = ev;

        if (categoryId.HasValue)
        {
            Category category =
                await _categoryRepository.GetByIdAsync(categoryId.Value, cancellationToken)
                ?? throw new Exception("Category not found");

            updated = updated with
            {
                CategoryId = category.Id,
                Category = category,
            };
        }

        if (venueId.HasValue)
        {
            Venue venue =
                await _venueRepository.GetByIdAsync(venueId.Value, cancellationToken)
                ?? throw new Exception("Venue not found");

            updated = updated with
            {
                VenueId = venue.Id,
                Venue = venue,
            };
        }

        if (startDate.HasValue && endDate.HasValue && startDate >= endDate)
            throw new Exception("StartDate must be earlier than EndDate");

        if (title is not null)
            updated = updated with { Title = title };

        if (description is not null)
            updated = updated with { Description = description };

        if (startDate.HasValue)
            updated = updated with { StartDate = startDate.Value };

        if (endDate.HasValue)
            updated = updated with { EndDate = endDate.Value };

        await _eventRepository.UpdateAsync(updated, cancellationToken);

        scope.Complete();

        return updated;
    }

    public async Task<bool> CanEditEventAsync(
        long organizerId,
        long eventId,
        CancellationToken cancellationToken)
    {
        if (await IsAdminAsync(organizerId, cancellationToken))
            return true;

        var organizers = new List<EventOrganizer>();
        await foreach (EventOrganizer o in _eventOrganizerRepository.GetByEventAsync(eventId, cancellationToken))
        {
            organizers.Add(o);
        }

        return organizers.Any(o => o.OrganizerId == organizerId);
    }

    public Task<bool> IsAdminAsync(long userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Метод IsAdminAsync еще не реализован");
    }

    public async IAsyncEnumerable<EventEntity> GetAllEventsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (EventEntity ev in _eventRepository.GetAllAsync(cancellationToken))
        {
            yield return ev;
        }
    }
}