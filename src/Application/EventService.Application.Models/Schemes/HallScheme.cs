using EventService.Application.Models.Venues;

namespace EventService.Application.Models.Schemes;

public record HallScheme(long Id, string Name, int Rows, int Columns, long VenueId, Venue Venue);