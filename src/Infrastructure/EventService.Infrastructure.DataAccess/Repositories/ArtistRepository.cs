using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Artists;
using EventService.Application.Models.Events;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class ArtistRepository : IArtistRepository
{
    private readonly string _connectionString;

    public ArtistRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Artist?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, bio FROM artists WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new Artist(
            Id: reader.GetInt64(0),
            Name: reader.GetString(1),
            Bio: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            Events: new Collection<EventEntity>());
    }

    public async Task<IReadOnlyList<Artist>> GetAllAsync()
    {
        var result = new Collection<Artist>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name, bio FROM artists", conn);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Artist(
                Id: reader.GetInt64(0),
                Name: reader.GetString(1),
                Bio: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Events: new Collection<EventEntity>()));
        }

        return result;
    }

    public async Task AddAsync(Artist entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "INSERT INTO artists (name, bio) VALUES (@name, @bio) RETURNING id", conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("bio", entity.Bio ?? string.Empty);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
           throw new ArgumentException($"Could not insert artist with id {entity.Id}");

        long id = (long)result;

        typeof(Artist).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task UpdateAsync(Artist entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "UPDATE artists SET name = @name, bio = @bio WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", entity.Id);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("bio", entity.Bio ?? string.Empty);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM artists WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<Artist>> GetByEventAsync(long eventId)
    {
        var result = new Collection<Artist>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"
                SELECT a.id, a.name, a.bio
                FROM artists a
                INNER JOIN event_artists ea ON a.id = ea.artist_id
                WHERE ea.event_id = @eventId",
            conn);

        cmd.Parameters.AddWithValue("eventId", eventId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Artist(
                Id: reader.GetInt64(0),
                Name: reader.GetString(1),
                Bio: reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Events: new Collection<EventEntity>()));
        }

        return result;
    }
}