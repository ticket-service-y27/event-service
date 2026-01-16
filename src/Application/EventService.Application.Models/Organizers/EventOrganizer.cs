using EventService.Application.Models.EventEntities;

namespace EventService.Application.Models.Organizers;

public record EventOrganizer(long Id, long EventId, EventEntity EventEntity, long OrganizerId, Organizer Organizer);