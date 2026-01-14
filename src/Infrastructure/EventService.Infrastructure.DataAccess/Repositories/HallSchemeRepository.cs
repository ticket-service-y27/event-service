using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class HallSchemeRepository : IHallSchemeRepository
{
    private readonly string _connectionString;
    private readonly IVenueRepository _venueRepository;

    public HallSchemeRepository(string connectionString, IVenueRepository venueRepository)
    {
        _connectionString = connectionString;
        _venueRepository = venueRepository;
    }

    public async Task AddAsync(HallScheme entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO hall_schemes 
                  (name, rows, columns, venue_id)
                  VALUES (@name, @rows, @cols, @venueId)
                  RETURNING id",
            conn);

        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("rows", entity.Rows);
        cmd.Parameters.AddWithValue("cols", entity.Columns);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not insert hall scheme with id {entity.Id}");

        long id = (long)result;

        typeof(HallScheme).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM hall_schemes WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<HallScheme>> GetAllAsync()
    {
        var result = new Collection<HallScheme>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, rows, columns, venue_id FROM hall_schemes", conn);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            long id = reader.GetInt64(0);
            string name = reader.GetString(1);
            int rows = reader.GetInt32(2);
            int cols = reader.GetInt32(3);
            long venueId = reader.GetInt64(4);

            Venue? venue = await _venueRepository.GetByIdAsync(venueId);
            if (venue == null) continue;

            result.Add(new HallScheme(
                Id: id,
                Name: name,
                Rows: rows,
                Columns: cols,
                VenueId: venueId,
                Venue: venue));
        }

        return result;
    }

    public async Task<HallScheme?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, rows, columns, venue_id FROM hall_schemes WHERE id=@id", conn);

        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        long hallId = reader.GetInt64(0);
        string name = reader.GetString(1);
        int rows = reader.GetInt32(2);
        int cols = reader.GetInt32(3);
        long venueId = reader.GetInt64(4);

        Venue? venue = await _venueRepository.GetByIdAsync(venueId);
        if (venue == null) return null;

        return new HallScheme(
            Id: hallId,
            Name: name,
            Rows: rows,
            Columns: cols,
            VenueId: venueId,
            Venue: venue);
    }

    public async Task<IReadOnlyList<HallScheme>> GetByVenueAsync(long venueId)
    {
        IReadOnlyList<HallScheme> allSchemes = await GetAllAsync();
        return allSchemes.Where(h => h.VenueId == venueId).ToList();
    }

    public async Task UpdateAsync(HallScheme entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"UPDATE hall_schemes 
                  SET name=@name, rows=@rows, columns=@cols, venue_id=@venueId
                  WHERE id=@id",
            conn);

        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("rows", entity.Rows);
        cmd.Parameters.AddWithValue("cols", entity.Columns);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync();
    }
}