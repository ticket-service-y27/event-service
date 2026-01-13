using EventService.Application.Models.Events;

namespace EventService.Application.Models.Categories;

public record Category(long Id, string Name, IEnumerable<EventEntity>  Events);