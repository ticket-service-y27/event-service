using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Organizers;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class OrganizerRepository : IOrganizerRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public OrganizerRepository(IOptions<DatabaseOptions> options)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();
    }

    public async Task<long> AddAsync(Organizer entity, CancellationToken cancellationToken)
    {
        const string sql = @"INSERT INTO organizers (name) VALUES (@name) RETURNING id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);

        long id = (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);

        return id;
    }

    public async Task UpdateAsync(Organizer entity, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE organizers SET name=@name WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM organizers WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Organizer?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name FROM organizers WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new Organizer(
            reader.GetInt64(0),
            reader.GetString(1),
            new Collection<EventOrganizer>());
    }

    public async Task<IReadOnlyList<Organizer>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name FROM organizers;";
        var result = new Collection<Organizer>();

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new Organizer(
                reader.GetInt64(0),
                reader.GetString(1),
                new Collection<EventOrganizer>()));
        }

        return result;
    }

    public async IAsyncEnumerable<Organizer> GetByEventAsync(
        long eventId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT o.id, o.name
FROM organizers o
INNER JOIN event_organizers eo ON eo.organizer_id = o.id
WHERE eo.event_id=@eventId;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("eventId", eventId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Organizer(
                reader.GetInt64(0),
                reader.GetString(1),
                new Collection<EventOrganizer>());
        }
    }
}