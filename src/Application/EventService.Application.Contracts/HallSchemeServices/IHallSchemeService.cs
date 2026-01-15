using EventService.Application.Models.Schemes;

namespace EventService.Application.Contracts.HallSchemeServices;

public interface IHallSchemeService
{
    Task<HallScheme?> GetSchemeAsync(long hallSchemeId);

    Task<IReadOnlyList<HallScheme>> GetVenueSchemesAsync(long venueId);
}