using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.EventManagerServices;
using EventService.Application.Models.Artists;
using EventService.Application.Models.Events;
using EventService.Application.Models.Organizers;
using System.Collections.ObjectModel;

namespace EventService.Application.EventManagerServices;

public class EventManagerService : IEventManagerService
{
    private readonly IEventRepository _eventRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IEventOrganizerRepository _eventOrganizerRepository;
    private readonly IOrganizerRepository _organizerRepository;

    public EventManagerService(
        IEventRepository eventRepository,
        ICategoryRepository categoryRepository,
        IVenueRepository venueRepository,
        IEventOrganizerRepository eventOrganizerRepository,
        IOrganizerRepository organizerRepository)
    {
        _eventRepository = eventRepository;
        _categoryRepository = categoryRepository;
        _venueRepository = venueRepository;
        _eventOrganizerRepository = eventOrganizerRepository;
        _organizerRepository = organizerRepository;
    }

    public async Task<EventEntity> CreateEventAsync(
        long organizerId,
        string title,
        string description,
        DateTime startDate,
        DateTime endDate,
        long categoryId,
        long venueId)
    {
        Models.Categories.Category category = await _categoryRepository.GetByIdAsync(categoryId)
                       ?? throw new Exception("Category not found");

        Models.Venues.Venue venue = await _venueRepository.GetByIdAsync(venueId)
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

        await _eventRepository.AddAsync(entity);

        Organizer organizer = await _organizerRepository.GetByIdAsync(organizerId)
                        ?? throw new Exception("Organizer not found");

        var link = new EventOrganizer(
            Id: 0,
            EventId: entity.Id,
            EventEntity: entity,
            OrganizerId: organizerId,
            Organizer: organizer);

        await _eventOrganizerRepository.AddAsync(link);
        organizers.Add(link);

        return entity;
    }

    public async Task<EventEntity> UpdateEventAsync(
        long organizerId,
        long eventId,
        string? title = null,
        string? description = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        long? categoryId = null,
        long? venueId = null)
    {
        EventEntity ev = await _eventRepository.GetByIdAsync(eventId)
            ?? throw new Exception("Event not found");

        if (!await CanEditEventAsync(organizerId, eventId))
            throw new UnauthorizedAccessException("User cannot edit this event");

        EventEntity updated = ev;

        if (categoryId.HasValue)
        {
            Models.Categories.Category category = await _categoryRepository.GetByIdAsync(categoryId.Value)
                ?? throw new Exception("Category not found");

            updated = updated with
            {
                CategoryId = category.Id,
                Category = category,
            };
        }

        if (venueId.HasValue)
        {
            Models.Venues.Venue venue = await _venueRepository.GetByIdAsync(venueId.Value)
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

        await _eventRepository.UpdateAsync(updated);

        return updated;
    }

    public async Task<bool> CanEditEventAsync(long organizerId, long eventId)
    {
        if (await IsAdminAsync(organizerId))
            return true;

        IReadOnlyList<EventOrganizer> organizers = await _eventOrganizerRepository.GetByEventAsync(eventId);
        return organizers.Any(o => o.OrganizerId == organizerId);
    }

    public Task<bool> IsAdminAsync(long userId)
    {
        throw new NotImplementedException("Метод IsAdminAsync еще не реализован");
    }
}