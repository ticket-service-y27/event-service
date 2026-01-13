namespace EventService.Application.Models.Organizers;

public record Organizer(long Id, string Name, IEnumerable<EventOrganizer> Events);