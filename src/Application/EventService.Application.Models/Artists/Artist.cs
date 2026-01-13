using EventService.Application.Models.Events;

namespace EventService.Application.Models.Artists;

public record Artist(long Id, string Name, string Bio, IEnumerable<EventEntity> Events);