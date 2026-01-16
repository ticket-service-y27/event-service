using EventService.Application.Models.Artists;
using EventService.Application.Models.Categories;
using EventService.Application.Models.Organizers;
using EventService.Application.Models.Venues;
using System.Collections.ObjectModel;

namespace EventService.Application.Models.EventEntities;

public record EventEntity(
    long Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    long CategoryId,
    Category Category,
    long VenueId,
    Venue Venue,
    Collection<EventOrganizer> Organizers,
    Collection<Artist> Artists);