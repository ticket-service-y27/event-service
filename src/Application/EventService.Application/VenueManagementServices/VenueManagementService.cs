using EventService.Application.Abstractions.Messaging;
using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.VenueManagementServices;
using EventService.Application.Models.Events;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using System.Transactions;

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

    public async Task<Venue> CreateVenueAsync(string name, string address, CancellationToken cancellationToken)
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

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        venue = venue with { Id = await _venueRepository.AddAsync(venue, cancellationToken) };

        scope.Complete();

        return venue;
    }

    public async Task<Venue> UpdateVenueAsync(
        long venueId,
        CancellationToken cancellationToken,
        string? name = null,
        string? address = null)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (venue == null)
            throw new KeyNotFoundException($"Venue {venueId} not found");

        Venue updated = venue with
        {
            Name = name ?? venue.Name,
            Address = address ?? venue.Address,
        };

        await _venueRepository.UpdateAsync(updated, cancellationToken);

        scope.Complete();

        return updated;
    }

    public async Task<HallScheme> AddHallSchemeAsync(
        long venueId,
        string schemeName,
        int rows,
        int columns,
        CancellationToken cancellationToken)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Hall scheme must have positive size");

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (venue == null)
            throw new KeyNotFoundException($"Venue {venueId} not found");

        long hallSchemeId = long.Parse(schemeName);

        var scheme = new HallScheme(
            Id: hallSchemeId,
            VenueId: venueId,
            Venue: venue,
            Name: schemeName,
            Rows: rows,
            Columns: columns);

        scheme = scheme with { Id = await _hallSchemeRepository.AddAsync(scheme, cancellationToken) };

        scope.Complete();

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

    public async Task RemoveHallSchemeAsync(long hallSchemeId, CancellationToken cancellationToken)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        HallScheme? scheme = await _hallSchemeRepository.GetByIdAsync(hallSchemeId, cancellationToken);
        if (scheme != null)
            await _hallSchemeRepository.DeleteAsync(scheme.Id, cancellationToken);

        scope.Complete();
    }

    public async Task<bool> VenueHasAvailableSchemeAsync(long venueId, CancellationToken cancellationToken)
    {
        Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (venue == null) return false;

        var schemes = new List<HallScheme>();
        await foreach (HallScheme scheme in _hallSchemeRepository.GetByVenueAsync(venueId, cancellationToken))
        {
            schemes.Add(scheme);
        }

        return schemes.Count > 0;
    }

    public Task<HallScheme?> GetSchemeAsync(long hallSchemeId, CancellationToken cancellationToken) =>
        _hallSchemeRepository.GetByIdAsync(hallSchemeId, cancellationToken);

    public async Task<IReadOnlyList<HallScheme>> GetVenueSchemesAsync(long venueId, CancellationToken cancellationToken)
    {
        var list = new List<HallScheme>();
        await foreach (HallScheme scheme in _hallSchemeRepository.GetByVenueAsync(venueId, cancellationToken))
        {
            list.Add(scheme);
        }

        return list;
    }
}