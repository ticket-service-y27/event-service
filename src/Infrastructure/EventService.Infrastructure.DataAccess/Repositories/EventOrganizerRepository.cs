using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.EventEntities;
using EventService.Application.Models.Organizers;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class EventOrganizerRepository : IEventOrganizerRepository
{
    private readonly string _connectionString;
    private readonly IEventRepository _eventRepository;
    private readonly IOrganizerRepository _organizerRepository;

    public EventOrganizerRepository(
        string connectionString,
        IEventRepository eventRepository,
        IOrganizerRepository organizerRepository)
    {
        _connectionString = connectionString;
        _eventRepository = eventRepository;
        _organizerRepository = organizerRepository;
    }

    public async Task AddAsync(EventOrganizer entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "INSERT INTO event_organizers (event_id, organizer_id) VALUES (@eventId, @organizerId) RETURNING id", conn);
        cmd.Parameters.AddWithValue("eventId", entity.EventId);
        cmd.Parameters.AddWithValue("organizerId", entity.OrganizerId);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not insert event organizer with id {entity.Id}");

        long id = (long)result;

        typeof(EventOrganizer).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task RemoveAsync(long eventId, long organizerId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "DELETE FROM event_organizers WHERE event_id = @eventId AND organizer_id = @organizerId", conn);
        cmd.Parameters.AddWithValue("eventId", eventId);
        cmd.Parameters.AddWithValue("organizerId", organizerId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<EventOrganizer>> GetByEventAsync(long eventId)
    {
        var result = new Collection<EventOrganizer>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, event_id, organizer_id FROM event_organizers WHERE event_id = @eventId", conn);
        cmd.Parameters.AddWithValue("eventId", eventId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            long id = reader.GetInt64(0);
            long eId = reader.GetInt64(1);
            long orgId = reader.GetInt64(2);

            EventEntity? ev = await _eventRepository.GetByIdAsync(eId);
            Organizer? orgs = await _organizerRepository.GetByIdAsync(orgId);

            if (ev != null && orgs != null)
            {
                result.Add(new EventOrganizer(
                    Id: id,
                    EventId: eId,
                    EventEntity: ev,
                    OrganizerId: orgId,
                    Organizer: orgs));
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<EventOrganizer>> GetByOrganizerAsync(long organizerId)
    {
        var result = new Collection<EventOrganizer>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, event_id, organizer_id FROM event_organizers WHERE organizer_id = @organizerId", conn);
        cmd.Parameters.AddWithValue("organizerId", organizerId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            long id = reader.GetInt64(0);
            long eId = reader.GetInt64(1);
            long orgId = reader.GetInt64(2);

            EventEntity? ev = await _eventRepository.GetByIdAsync(eId);
            Organizer? orgs = await _organizerRepository.GetByIdAsync(orgId);

            if (ev != null && orgs != null)
            {
                result.Add(new EventOrganizer(
                    Id: id,
                    EventId: eId,
                    EventEntity: ev,
                    OrganizerId: orgId,
                    Organizer: orgs));
            }
        }

        return result;
    }
}