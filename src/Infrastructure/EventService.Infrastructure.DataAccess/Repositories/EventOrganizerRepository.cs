using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.EventEntities;
using EventService.Application.Models.Organizers;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class EventOrganizerRepository : IEventOrganizerRepository
{
    private readonly IEventRepository _eventRepository;
    private readonly IOrganizerRepository _organizerRepository;
    private readonly NpgsqlDataSource _dataSource;

    public EventOrganizerRepository(
        IOptions<DatabaseOptions> options,
        IEventRepository eventRepository,
        IOrganizerRepository organizerRepository)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();
        _eventRepository = eventRepository;
        _organizerRepository = organizerRepository;
    }

    public async Task AddAsync(EventOrganizer entity, CancellationToken cancellationToken = default)
    {
        const string sql = @"
INSERT INTO event_organizers (event_id, organizer_id) 
VALUES (@eventId, @organizerId) 
RETURNING id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("eventId", entity.EventId);
        cmd.Parameters.AddWithValue("organizerId", entity.OrganizerId);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new InvalidOperationException("Failed to insert event organizer");

        long id = (long)result;
        typeof(EventOrganizer).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task RemoveAsync(long eventId, long organizerId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
DELETE FROM event_organizers 
WHERE event_id = @eventId AND organizer_id = @organizerId;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("eventId", eventId);
        cmd.Parameters.AddWithValue("organizerId", organizerId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<EventOrganizer> GetByEventAsync(
        long eventId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT id, event_id, organizer_id 
FROM event_organizers 
WHERE event_id = @eventId;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("eventId", eventId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            long eId = reader.GetInt64(1);
            long orgId = reader.GetInt64(2);

            EventEntity? ev = await _eventRepository.GetByIdAsync(eId, cancellationToken);
            Organizer? org = await _organizerRepository.GetByIdAsync(orgId, cancellationToken);

            if (ev != null && org != null)
            {
                yield return new EventOrganizer(
                    Id: id,
                    EventId: eId,
                    EventEntity: ev,
                    OrganizerId: orgId,
                    Organizer: org);
            }
        }
    }

    public async Task<IReadOnlyList<EventOrganizer>> GetByOrganizerAsync(
        long organizerId,
        CancellationToken cancellationToken = default)
    {
        var result = new Collection<EventOrganizer>();

        const string sql = @"
SELECT id, event_id, organizer_id 
FROM event_organizers 
WHERE organizer_id = @organizerId;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("organizerId", organizerId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            long eId = reader.GetInt64(1);
            long orgId = reader.GetInt64(2);

            EventEntity? ev = await _eventRepository.GetByIdAsync(eId, cancellationToken);
            Organizer? org = await _organizerRepository.GetByIdAsync(orgId, cancellationToken);

            if (ev != null && org != null)
            {
                result.Add(new EventOrganizer(
                    Id: id,
                    EventId: eId,
                    EventEntity: ev,
                    OrganizerId: orgId,
                    Organizer: org));
            }
        }

        return result;
    }
}