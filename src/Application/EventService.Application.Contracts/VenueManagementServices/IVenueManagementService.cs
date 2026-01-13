using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;

namespace EventService.Application.Contracts.VenueManagementServices;

public interface IVenueManagementService
{
    Task<Venue> CreateVenueAsync(string name, string address);
    
    Task<Venue> UpdateVenueAsync(Guid venueId, string? name = null, string? address = null);

    Task<HallScheme> AddHallSchemeAsync(
        Guid venueId,
        string schemeName,
        int rows,
        int columns);

    Task RemoveHallSchemeAsync(Guid hallSchemeId);

    Task<bool> VenueHasAvailableSchemeAsync(Guid venueId);
}