using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;

namespace EventService.Application.Contracts.VenueManagementServices;

public interface IVenueManagementService
{
    Task<Venue> CreateVenueAsync(string name, string address, CancellationToken cancellationToken);

    Task<Venue> UpdateVenueAsync(long venueId, CancellationToken cancellationToken, string? name = null, string? address = null);

    Task<HallScheme> AddHallSchemeAsync(long venueId, string schemeName, int rows, int columns,  CancellationToken cancellationToken);

    Task RemoveHallSchemeAsync(long hallSchemeId,  CancellationToken cancellationToken);

    Task<bool> VenueHasAvailableSchemeAsync(long venueId,  CancellationToken cancellationToken);

    Task<HallScheme?> GetSchemeAsync(long hallSchemeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<HallScheme>> GetVenueSchemesAsync(long venueId,  CancellationToken cancellationToken);
}