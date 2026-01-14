using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Organizers;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class OrganizerRepository : IOrganizerRepository
{
    private readonly string _connectionString;

    public OrganizerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(Organizer entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO organizers (name) 
                  VALUES (@name)
                  RETURNING id",
            conn);

        cmd.Parameters.AddWithValue("name", entity.Name);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not insert organizer with id {entity.Id}");

        long id = (long)result;
        typeof(Organizer).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM organizers WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<Organizer>> GetAllAsync()
    {
        var result = new Collection<Organizer>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name FROM organizers", conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            long id = reader.GetInt64(0);
            string name = reader.GetString(1);

            result.Add(new Organizer(id, name, new Collection<EventOrganizer>()));
        }

        return result;
    }

    public async Task<Organizer?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name FROM organizers WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        long organizerId = reader.GetInt64(0);
        string name = reader.GetString(1);

        return new Organizer(organizerId, name, new Collection<EventOrganizer>());
    }

    public async Task<IReadOnlyList<Organizer>> GetByEventAsync(long eventId)
    {
        var result = new Collection<Organizer>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT o.id, o.name 
                  FROM organizers o
                  INNER JOIN event_organizers eo ON eo.organizer_id = o.id
                  WHERE eo.event_id=@eventId",
            conn);

        cmd.Parameters.AddWithValue("eventId", eventId);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            long id = reader.GetInt64(0);
            string name = reader.GetString(1);

            result.Add(new Organizer(id, name, new Collection<EventOrganizer>()));
        }

        return result;
    }

    public async Task UpdateAsync(Organizer entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "UPDATE organizers SET name=@name WHERE id=@id", conn);

        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync();
    }
}