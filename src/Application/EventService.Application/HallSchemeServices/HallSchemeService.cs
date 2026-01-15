using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.HallSchemeServices;
using EventService.Application.Models.Schemes;

namespace EventService.Application.HallSchemeServices;

public class HallSchemeService : IHallSchemeService
{
    private readonly IHallSchemeRepository _hallSchemeRepository;

    public HallSchemeService(IHallSchemeRepository hallSchemeRepository)
    {
        _hallSchemeRepository = hallSchemeRepository;
    }

    public Task<HallScheme?> GetSchemeAsync(long hallSchemeId)
        => _hallSchemeRepository.GetByIdAsync(hallSchemeId);

    public Task<IReadOnlyList<HallScheme>> GetVenueSchemesAsync(long venueId)
        => _hallSchemeRepository.GetByVenueAsync(venueId);
}