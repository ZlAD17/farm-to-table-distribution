using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class DriverRepository
{
    private readonly DatabaseHelper _db;

    public DriverRepository(DatabaseHelper db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DriverResponse>> GetAllAsync()
    {
        const string sql = "SELECT DriverId, FullName, Phone, LicensePlate, CreatedAt FROM Driver ORDER BY FullName;";
        
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<DriverResponse>();
        while (await reader.ReadAsync())
        {
            list.Add(new DriverResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetDateTime(4)
            ));
        }
        return list;
    }

    public async Task<DriverResponse?> GetByIdAsync(int id)
    {
        const string sql = "SELECT DriverId, FullName, Phone, LicensePlate, CreatedAt FROM Driver WHERE DriverId = @Id;";
        
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new DriverResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetDateTime(4)
            );
        }
        return null;
    }

    public async Task<DriverResponse> CreateAsync(CreateDriverRequest req)
    {
        const string sql = """
            INSERT INTO Driver (FullName, Phone, LicensePlate)
            OUTPUT INSERTED.DriverId, INSERTED.FullName, INSERTED.Phone, INSERTED.LicensePlate, INSERTED.CreatedAt
            VALUES (@FullName, @Phone, @LicensePlate);
            """;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FullName", req.FullName);
        cmd.Parameters.AddWithValue("@Phone", (object?)req.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LicensePlate", (object?)req.LicensePlate ?? DBNull.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new DriverResponse(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetDateTime(4)
        );
    }

    public async Task<DriverResponse?> UpdateAsync(int id, UpdateDriverRequest req)
    {
        const string sql = """
            UPDATE Driver
            SET FullName = @FullName, Phone = @Phone, LicensePlate = @LicensePlate
            OUTPUT INSERTED.DriverId, INSERTED.FullName, INSERTED.Phone, INSERTED.LicensePlate, INSERTED.CreatedAt
            WHERE DriverId = @Id;
            """;

        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FullName", req.FullName);
        cmd.Parameters.AddWithValue("@Phone", (object?)req.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@LicensePlate", (object?)req.LicensePlate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", id);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new DriverResponse(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.GetDateTime(4)
            );
        }
        return null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Driver WHERE DriverId = @Id;";
        
        await using var conn = await _db.OpenConnectionAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}