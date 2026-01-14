using EventService.Application.Models.Venues;

namespace EventService.Application.Abstractions.Repositories;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(long id);

    Task<IReadOnlyList<Venue>> GetAllAsync();

    Task AddAsync(Venue entity);

    Task UpdateAsync(Venue entity);

    Task DeleteAsync(long id);

    Task<bool> HasHallSchemesAsync(long venueId);
}