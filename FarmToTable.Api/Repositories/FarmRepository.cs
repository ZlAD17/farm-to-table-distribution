using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class FarmRepository(DatabaseHelper db)
{
    public async Task<IReadOnlyList<FarmResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT FarmId, Name, Location, ContactEmail, ContactPhone, CreatedAt
            FROM Farm
            ORDER BY Name;
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<FarmResponse>();
        while (await reader.ReadAsync())
            list.Add(MapRow(reader));
        return list;
    }

    public async Task<FarmResponse?> GetByIdAsync(int farmId)
    {
        const string sql = """
            SELECT FarmId, Name, Location, ContactEmail, ContactPhone, CreatedAt
            FROM Farm WHERE FarmId = @FarmId;
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FarmId", farmId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<FarmResponse> CreateAsync(CreateFarmRequest req)
    {
        const string sql = """
            INSERT INTO Farm (Name, Location, ContactEmail, ContactPhone)
            OUTPUT INSERTED.FarmId, INSERTED.Name, INSERTED.Location,
                   INSERTED.ContactEmail, INSERTED.ContactPhone, INSERTED.CreatedAt
            VALUES (@Name, @Location, @ContactEmail, @ContactPhone);
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        AddFarmParams(cmd, req.Name, req.Location, req.ContactEmail, req.ContactPhone);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    public async Task<FarmResponse?> UpdateAsync(int farmId, UpdateFarmRequest req)
    {
        const string sql = """
            UPDATE Farm
            SET Name = @Name, Location = @Location,
                ContactEmail = @ContactEmail, ContactPhone = @ContactPhone
            OUTPUT INSERTED.FarmId, INSERTED.Name, INSERTED.Location,
                   INSERTED.ContactEmail, INSERTED.ContactPhone, INSERTED.CreatedAt
            WHERE FarmId = @FarmId;
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        AddFarmParams(cmd, req.Name, req.Location, req.ContactEmail, req.ContactPhone);
        cmd.Parameters.AddWithValue("@FarmId", farmId);

        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<bool> DeleteAsync(int farmId)
    {
        const string sql = "DELETE FROM Farm WHERE FarmId = @FarmId;";

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FarmId", farmId);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static FarmResponse MapRow(SqlDataReader r) => new(
        r.GetInt32(0),
        r.GetString(1),
        r.GetString(2),
        r.IsDBNull(3) ? null : r.GetString(3),
        r.IsDBNull(4) ? null : r.GetString(4),
        r.GetDateTime(5)
    );

    private static void AddFarmParams(SqlCommand cmd, string name, string location,
        string? email, string? phone)
    {
        cmd.Parameters.AddWithValue("@Name",         name);
        cmd.Parameters.AddWithValue("@Location",     location);
        cmd.Parameters.AddWithValue("@ContactEmail", (object?)email  ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ContactPhone", (object?)phone  ?? DBNull.Value);
    }
}
