using EventService.Application.Abstractions.Repositories;
using Npgsql;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly string _connectionString;

    public SeatRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string?> GetStatusAsync(long hallSchemeId, int row, int seatNumber)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"SELECT status FROM seats
              WHERE hall_scheme_id=@schemeId
                AND row_number=@row
                AND seat_number=@seat",
            conn);

        cmd.Parameters.AddWithValue("schemeId", hallSchemeId);
        cmd.Parameters.AddWithValue("row", row);
        cmd.Parameters.AddWithValue("seat", seatNumber);

        return await cmd.ExecuteScalarAsync() as string;
    }

    public async Task SetStatusAsync(long hallSchemeId, int row, int seatNumber, string status)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"UPDATE seats
              SET status=@status
              WHERE hall_scheme_id=@schemeId
                AND row_number=@row
                AND seat_number=@seat",
            conn);

        cmd.Parameters.AddWithValue("schemeId", hallSchemeId);
        cmd.Parameters.AddWithValue("row", row);
        cmd.Parameters.AddWithValue("seat", seatNumber);
        cmd.Parameters.AddWithValue("status", status);

        int affected = await cmd.ExecuteNonQueryAsync();
        if (affected == 0)
            throw new InvalidOperationException("Seat not found");
    }
}