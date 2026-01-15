using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;

namespace EventService.Application.Contracts.VenueManagementServices;

public interface IVenueManagementService
{
    Task<Venue> CreateVenueAsync(string name, string address);

    Task<Venue> UpdateVenueAsync(long venueId, string? name = null, string? address = null);

    Task<HallScheme> AddHallSchemeAsync(long venueId, string schemeName, int rows, int columns);

    Task RemoveHallSchemeAsync(long hallSchemeId);

    Task<bool> VenueHasAvailableSchemeAsync(long venueId);
}