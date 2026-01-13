using EventService.Application.Models.Venues;

namespace EventService.Application.Abstractions.Repositories;

public interface IVenueRepository : IRepository<Venue>
{
    Task<bool> HasHallSchemesAsync(long venueId);
}