using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public VenueRepository(IOptions<DatabaseOptions> options)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();
    }

    public async Task AddAsync(Venue entity, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO venues (name, address) 
VALUES (@name, @address)
RETURNING id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("address", entity.Address);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new ArgumentException("Could not insert venue");

        long id = (long)result;
        typeof(Venue).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM venues WHERE id=@id;";
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Venue>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, address FROM venues;";
        var result = new Collection<Venue>();

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            long id = reader.GetInt64(0);
            string name = reader.GetString(1);
            string address = reader.GetString(2);

            result.Add(new Venue(id, name, address, new Collection<HallScheme>()));
        }

        return result;
    }

    public async Task<Venue?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, address FROM venues WHERE id=@id;";
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        long venueId = reader.GetInt64(0);
        string name = reader.GetString(1);
        string address = reader.GetString(2);

        return new Venue(venueId, name, address, new Collection<HallScheme>());
    }

    public async Task<bool> HasHallSchemesAsync(long venueId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT EXISTS(SELECT 1 FROM hall_schemes WHERE venue_id=@venueId);";
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("venueId", venueId);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        if (result == null)
            throw new ArgumentException($"Could not check hall schemes for venue {venueId}");

        return (bool)result;
    }

    public async Task UpdateAsync(Venue entity, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE venues SET name=@name, address=@address WHERE id=@id;";
        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("address", entity.Address);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}