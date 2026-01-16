using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Schemes;
using EventService.Application.Models.Venues;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Runtime.CompilerServices;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class HallSchemeRepository : IHallSchemeRepository
{
    private readonly IVenueRepository _venueRepository;
    private readonly NpgsqlDataSource _dataSource;

    public HallSchemeRepository(
        IOptions<DatabaseOptions> options,
        IVenueRepository venueRepository)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();

        _venueRepository = venueRepository;
    }

    public async Task<long> AddAsync(HallScheme entity, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO hall_schemes (name, rows, columns, venue_id)
VALUES (@name, @rows, @cols, @venueId) RETURNING id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("rows", entity.Rows);
        cmd.Parameters.AddWithValue("cols", entity.Columns);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);

        long id = (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);

        return id;
    }

    public async Task UpdateAsync(HallScheme entity, CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE hall_schemes 
SET name=@name, rows=@rows, columns=@cols, venue_id=@venueId
WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);
        cmd.Parameters.AddWithValue("rows", entity.Rows);
        cmd.Parameters.AddWithValue("cols", entity.Columns);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM hall_schemes WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<HallScheme?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, rows, columns, venue_id FROM hall_schemes WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        long hallId = reader.GetInt64(0);
        string name = reader.GetString(1);
        int rows = reader.GetInt32(2);
        int cols = reader.GetInt32(3);
        long venueId = reader.GetInt64(4);

        Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);
        if (venue == null)
            return null;

        return new HallScheme(
            Id: hallId,
            Name: name,
            Rows: rows,
            Columns: cols,
            VenueId: venueId,
            Venue: venue);
    }

    public async IAsyncEnumerable<HallScheme> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name, rows, columns, venue_id FROM hall_schemes;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            long hallId = reader.GetInt64(0);
            string name = reader.GetString(1);
            int rows = reader.GetInt32(2);
            int cols = reader.GetInt32(3);
            long venueId = reader.GetInt64(4);

            Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);
            if (venue == null)
                continue;

            yield return new HallScheme(
                Id: hallId,
                Name: name,
                Rows: rows,
                Columns: cols,
                VenueId: venueId,
                Venue: venue);
        }
    }

    public async IAsyncEnumerable<HallScheme> GetByVenueAsync(long venueId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (HallScheme scheme in GetAllAsync(cancellationToken))
        {
            if (scheme.VenueId == venueId)
                yield return scheme;
        }
    }
}