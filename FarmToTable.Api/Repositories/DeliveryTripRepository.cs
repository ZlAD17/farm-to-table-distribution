using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class DeliveryTripRepository(DatabaseHelper db)
{
    private const string SelectBase = """
        SELECT dt.TripId, dt.DriverId, d.FullName AS DriverName,
               dt.OrderId, dt.ScheduledAt, dt.CompletedAt, dt.Notes
        FROM DeliveryTrip dt
        INNER JOIN Driver d ON d.DriverId = dt.DriverId
        """;

    public async Task<IReadOnlyList<DeliveryTripResponse>> GetAllAsync()
    {
        var sql = SelectBase + " ORDER BY dt.ScheduledAt DESC;";
        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<DeliveryTripResponse>();
        while (await reader.ReadAsync()) list.Add(MapRow(reader));
        return list;
    }

    public async Task<DeliveryTripResponse?> GetByIdAsync(int tripId)
    {
        var sql = SelectBase + " WHERE dt.TripId = @TripId;";
        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@TripId", tripId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<DeliveryTripResponse> CreateAsync(CreateDeliveryTripRequest req)
    {
        const string sql = """
            INSERT INTO DeliveryTrip (DriverId, OrderId, ScheduledAt, Notes)
            OUTPUT INSERTED.TripId
            VALUES (@DriverId, @OrderId, @ScheduledAt, @Notes);
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@DriverId",    req.DriverId);
        cmd.Parameters.AddWithValue("@OrderId",     req.OrderId);
        cmd.Parameters.AddWithValue("@ScheduledAt", req.ScheduledAt);
        cmd.Parameters.AddWithValue("@Notes",       (object?)req.Notes ?? DBNull.Value);

        var newId = (int)(await cmd.ExecuteScalarAsync())!;
        return (await GetByIdAsync(newId))!;
    }

    public async Task<DeliveryTripResponse?> CompleteAsync(int tripId, CompleteDeliveryTripRequest req)
    {
        const string sql = """
            UPDATE DeliveryTrip
            SET CompletedAt = @CompletedAt, Notes = @Notes
            WHERE TripId = @TripId;
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CompletedAt", req.CompletedAt);
        cmd.Parameters.AddWithValue("@Notes",       (object?)req.Notes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TripId",      tripId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0 ? await GetByIdAsync(tripId) : null;
    }

    private static DeliveryTripResponse MapRow(SqlDataReader r) => new(
        r.GetInt32(0),
        r.GetInt32(1),
        r.GetString(2),
        r.GetInt32(3),
        r.GetDateTime(4),
        r.IsDBNull(5) ? null : r.GetDateTime(5),
        r.IsDBNull(6) ? null : r.GetString(6)
    );
}
