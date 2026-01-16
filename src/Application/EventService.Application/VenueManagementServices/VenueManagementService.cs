using EventService.Application.Abstractions.Messaging;
using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.VenueManagementServices;
using EventService.Application.Models.Events;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;

namespace EventService.Application.VenueManagementServices;

public class VenueManagementService : IVenueManagementService
{
    private readonly IVenueRepository _venueRepository;
    private readonly IHallSchemeRepository _hallSchemeRepository;
    private readonly IVenueCreatedPublisher _venueCreatedPublisher;

    public VenueManagementService(
        IVenueRepository venueRepository,
        IHallSchemeRepository hallSchemeRepository,
        IVenueCreatedPublisher venueCreatedPublisher)
    {
        _venueRepository = venueRepository;
        _hallSchemeRepository = hallSchemeRepository;
        _venueCreatedPublisher = venueCreatedPublisher;
    }

    public async Task<Venue> CreateVenueAsync(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Venue name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Venue address is required", nameof(address));

        var venue = new Venue(
            Id: 0,
            Name: name,
            Address: address,
            HallSchemes: new List<HallScheme>());

        await _venueRepository.AddAsync(venue);

        return venue;
    }

    public async Task<Venue> UpdateVenueAsync(long venueId, string? name = null, string? address = null)
    {
        Venue? venue = await _venueRepository.GetByIdAsync(venueId);
        if (venue == null)
            throw new KeyNotFoundException($"Venue {venueId} not found");

        Venue updated = venue with
        {
            Name = name ?? venue.Name,
            Address = address ?? venue.Address,
        };

        await _venueRepository.UpdateAsync(updated);
        return updated;
    }

    public async Task<HallScheme> AddHallSchemeAsync(long venueId, string schemeName, int rows, int columns)
    {
        Venue? venue = await _venueRepository.GetByIdAsync(venueId);
        if (venue == null)
            throw new KeyNotFoundException($"Venue {venueId} not found");

        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Hall scheme must have positive size");

        long hallSchemeId = long.Parse(schemeName);

        var scheme = new HallScheme(
            Id: hallSchemeId,
            VenueId: venueId,
            Venue: venue,
            Name: schemeName,
            Rows: rows,
            Columns: columns);

        await _hallSchemeRepository.AddAsync(scheme);

        int totalSeats = rows * columns;
        await _venueCreatedPublisher.PublishAsync(
            new VenueCreatedEvent(
                VenueId: venue.Id,
                TotalSeats: totalSeats,
                Address: venue.Address,
                HallSchemeId: hallSchemeId),
            CancellationToken.None);

        return scheme;
    }

    public async Task RemoveHallSchemeAsync(long hallSchemeId)
    {
        HallScheme? scheme = await _hallSchemeRepository.GetByIdAsync(hallSchemeId);
        if (scheme == null) return;

        await _hallSchemeRepository.DeleteAsync(scheme.Id);
    }

    public async Task<bool> VenueHasAvailableSchemeAsync(long venueId)
    {
        Venue? venue = await _venueRepository.GetByIdAsync(venueId);
        if (venue == null) return false;

        IReadOnlyList<HallScheme> schemes = await _hallSchemeRepository.GetByVenueAsync(venueId);
        return schemes.Count > 0;
    }

    public Task<HallScheme?> GetSchemeAsync(long hallSchemeId) => _hallSchemeRepository.GetByIdAsync(hallSchemeId);

    public Task<IReadOnlyList<HallScheme>> GetVenueSchemesAsync(long venueId) => _hallSchemeRepository.GetByVenueAsync(venueId);
}