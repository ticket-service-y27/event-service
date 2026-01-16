using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Artists;
using EventService.Application.Models.EventEntities;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class ArtistRepository : IArtistRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public ArtistRepository(IOptions<DatabaseOptions> options)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();
    }

    public async Task<Artist?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, bio FROM artists WHERE id = @id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new Artist(
            Id: reader.GetInt64(0),
            Name: reader.GetString(1),
            Bio: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Events: new Collection<EventEntity>());
    }

    public async IAsyncEnumerable<Artist> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, bio FROM artists";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Artist(
                Id: reader.GetInt64(0),
                Name: reader.GetString(1),
                Bio: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Events: new Collection<EventEntity>());
        }
    }

    public async Task AddAsync(Artist entity, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO artists (name, bio) VALUES (@name, @bio) RETURNING id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("bio", entity.Bio);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new InvalidOperationException("Failed to insert artist");

        long id = (long)result;
        typeof(Artist).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task UpdateAsync(Artist entity, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE artists SET name = @name, bio = @bio WHERE id = @id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", entity.Id);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("bio", entity.Bio);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM artists WHERE id = @id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<Artist> GetByEventAsync(
        long eventId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT a.id, a.name, a.bio
            FROM artists a
            INNER JOIN event_artists ea ON a.id = ea.artist_id
            WHERE ea.event_id = @eventId";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("eventId", eventId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Artist(
                Id: reader.GetInt64(0),
                Name: reader.GetString(1),
                Bio: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Events: new Collection<EventEntity>());
        }
    }
}