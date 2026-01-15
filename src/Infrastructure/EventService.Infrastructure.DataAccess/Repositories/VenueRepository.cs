using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly string _connectionString;

    public VenueRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task AddAsync(Venue entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO venues (name, address) 
                  VALUES (@name, @address)
                  RETURNING id",
            conn);

        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("address", entity.Address);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not insert venue");

        long id = (long)result;
        typeof(Venue).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "DELETE FROM venues WHERE id=@id", conn);

        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<Venue>> GetAllAsync()
    {
        var result = new Collection<Venue>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name, address FROM venues", conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            long venueId = reader.GetInt64(0);
            string name = reader.GetString(1);
            string address = reader.GetString(2);

            result.Add(new Venue(venueId, name, address, new Collection<HallScheme>()));
        }

        return result;
    }

    public async Task<Venue?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name, address FROM venues WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        long venueId = reader.GetInt64(0);
        string name = reader.GetString(1);
        string address = reader.GetString(2);

        var hallSchemes = new Collection<HallScheme>();

        await reader.CloseAsync();

        await using var cmdSchemes = new NpgsqlCommand(
            "SELECT id, name, rows, columns, venue_id FROM hall_schemes WHERE venue_id=@venueId", conn);
        cmdSchemes.Parameters.AddWithValue("venueId", venueId);

        await using NpgsqlDataReader readerSchemes = await cmdSchemes.ExecuteReaderAsync();
        while (await readerSchemes.ReadAsync())
        {
            long schemeId = readerSchemes.GetInt64(0);
            string schemeName = readerSchemes.GetString(1);
            int rows = readerSchemes.GetInt32(2);
            int columns = readerSchemes.GetInt32(3);
            long venueIdFk = readerSchemes.GetInt64(4);

            hallSchemes.Add(new HallScheme(schemeId, schemeName, rows, columns, venueIdFk, new Venue(venueId, name, address, new Collection<HallScheme>())));
        }

        return new Venue(venueId, name, address, hallSchemes);
    }

    public async Task<bool> HasHallSchemesAsync(long venueId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT EXISTS(SELECT 1 FROM hall_schemes WHERE venue_id=@venueId)", conn);
        cmd.Parameters.AddWithValue("venueId", venueId);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not find hall scheme with id {venueId}");

        bool flag = (bool)result;

        return flag;
    }

    public async Task UpdateAsync(Venue entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "UPDATE venues SET name=@name, address=@address WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("address", entity.Address);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync();
    }
}