using EventService.Application.Models.Schemes;

namespace EventService.Application.Models.Venues;

public record Venue(long Id, string Name, string Address, IEnumerable<HallScheme> HallSchemes);