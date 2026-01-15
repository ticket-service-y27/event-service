using EventService.Application.Abstractions.Repositories;
using EventService.Application.Contracts.SeatValidationServices;
using EventService.Application.Models.Schemes;

namespace EventService.Application.SeatValidationServices;

public class SeatValidationService : ISeatValidationService
{
    private readonly IHallSchemeRepository _hallSchemeRepository;
    private readonly IEventRepository _eventRepository;

    public SeatValidationService(
        IHallSchemeRepository hallSchemeRepository,
        IEventRepository eventRepository)
    {
        _hallSchemeRepository = hallSchemeRepository;
        _eventRepository = eventRepository;
    }

    public async Task<bool> SeatExistsAsync(long hallSchemeId, int row, int seatNumber)
    {
        HallScheme? scheme = await _hallSchemeRepository.GetByIdAsync(hallSchemeId);

        if (scheme == null)
            return false;

        return row > 0 &&
               seatNumber > 0 &&
               row <= scheme.Rows &&
               seatNumber <= scheme.Columns;
    }

    public async Task<bool> IsSeatAvailableAsync(long eventId, int row, int seatNumber)
    {
        bool eventExists = await _eventRepository.ExistsAsync(eventId);

        if (!eventExists)
            return false;

        return true;
    }

    public async Task<string> GetSeatStatusAsync(long eventId, int row, int seatNumber)
    {
        bool eventExists = await _eventRepository.ExistsAsync(eventId);

        if (!eventExists)
            return "unknown";

        return "free";
    }
}