using EventService.Application.Models.EventEntities;

namespace EventService.Application.Models.Categories;

public record Category(long Id, string Name, IEnumerable<EventEntity> Events);