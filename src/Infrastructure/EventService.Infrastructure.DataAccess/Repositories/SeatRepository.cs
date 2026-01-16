using EventService.Application.Abstractions.Repositories;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public SeatRepository(IOptions<DatabaseOptions> options)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();
    }

    public async Task<string?> GetStatusAsync(long hallSchemeId, int row, int seatNumber, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT status
FROM seats
WHERE hall_scheme_id=@schemeId
  AND row_number=@row
  AND seat_number=@seat;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("schemeId", hallSchemeId);
        cmd.Parameters.AddWithValue("row", row);
        cmd.Parameters.AddWithValue("seat", seatNumber);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result as string;
    }

    public async Task SetStatusAsync(long hallSchemeId, int row, int seatNumber, string status, CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE seats
SET status=@status
WHERE hall_scheme_id=@schemeId
  AND row_number=@row
  AND seat_number=@seat;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("schemeId", hallSchemeId);
        cmd.Parameters.AddWithValue("row", row);
        cmd.Parameters.AddWithValue("seat", seatNumber);
        cmd.Parameters.AddWithValue("status", status);

        int affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
        if (affected == 0)
            throw new InvalidOperationException("Seat not found");
    }
}