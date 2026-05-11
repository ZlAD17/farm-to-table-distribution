using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class DriverRepository(DatabaseHelper db)
{
    public async Task<IReadOnlyList<DriverResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT DriverId, FullName, Phone, LicensePlate, CreatedAt
            FROM Driver ORDER BY FullName;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<DriverResponse>();
        while (await reader.ReadAsync()) list.Add(MapRow(reader));
        return list;
    }

    public async Task<DriverResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT DriverId, FullName, Phone, LicensePlate, CreatedAt
            FROM Driver WHERE DriverId = @Id;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<DriverResponse> CreateAsync(CreateDriverRequest req)
    {
        const string sql = """
            INSERT INTO Driver (FullName, Phone, LicensePlate)
            OUTPUT INSERTED.DriverId, INSERTED.FullName, INSERTED.Phone,
                   INSERTED.LicensePlate, INSERTED.CreatedAt
            VALUES (@FullName, @Phone, @LicensePlate);
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        AddParams(cmd, req.FullName, req.Phone, req.LicensePlate);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    public async Task<DriverResponse?> UpdateAsync(int id, UpdateDriverRequest req)
    {
        const string sql = """
            UPDATE Driver
            SET FullName = @FullName, Phone = @Phone, LicensePlate = @LicensePlate
            OUTPUT INSERTED.DriverId, INSERTED.FullName, INSERTED.Phone,
                   INSERTED.LicensePlate, INSERTED.CreatedAt
            WHERE DriverId = @Id;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        AddParams(cmd, req.FullName, req.Phone, req.LicensePlate);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Driver WHERE DriverId = @Id;";
        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static DriverResponse MapRow(SqlDataReader r) => new(
        r.GetInt32(0), r.GetString(1),
        r.IsDBNull(2) ? null : r.GetString(2),
        r.IsDBNull(3) ? null : r.GetString(3),
        r.GetDateTime(4));

    private static void AddParams(SqlCommand cmd, string fullName, string? phone, string? plate)
    {
        cmd.Parameters.AddWithValue("@FullName",     fullName);
        cmd.Parameters.AddWithValue("@Phone",        (object?)phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LicensePlate", (object?)plate ?? DBNull.Value);
    }
}
