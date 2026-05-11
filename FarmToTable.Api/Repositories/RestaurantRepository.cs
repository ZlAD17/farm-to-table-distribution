using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class RestaurantRepository(DatabaseHelper db)
{
    public async Task<IReadOnlyList<RestaurantResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT RestaurantId, Name, Address, ContactEmail, ContactPhone, CreatedAt
            FROM Restaurant ORDER BY Name;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<RestaurantResponse>();
        while (await reader.ReadAsync()) list.Add(MapRow(reader));
        return list;
    }

    public async Task<RestaurantResponse?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT RestaurantId, Name, Address, ContactEmail, ContactPhone, CreatedAt
            FROM Restaurant WHERE RestaurantId = @Id;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<RestaurantResponse> CreateAsync(CreateRestaurantRequest req)
    {
        const string sql = """
            INSERT INTO Restaurant (Name, Address, ContactEmail, ContactPhone)
            OUTPUT INSERTED.RestaurantId, INSERTED.Name, INSERTED.Address,
                   INSERTED.ContactEmail, INSERTED.ContactPhone, INSERTED.CreatedAt
            VALUES (@Name, @Address, @Email, @Phone);
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        AddParams(cmd, req.Name, req.Address, req.ContactEmail, req.ContactPhone);
        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return MapRow(reader);
    }

    public async Task<RestaurantResponse?> UpdateAsync(int id, UpdateRestaurantRequest req)
    {
        const string sql = """
            UPDATE Restaurant
            SET Name = @Name, Address = @Address, ContactEmail = @Email, ContactPhone = @Phone
            OUTPUT INSERTED.RestaurantId, INSERTED.Name, INSERTED.Address,
                   INSERTED.ContactEmail, INSERTED.ContactPhone, INSERTED.CreatedAt
            WHERE RestaurantId = @Id;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        AddParams(cmd, req.Name, req.Address, req.ContactEmail, req.ContactPhone);
        cmd.Parameters.AddWithValue("@Id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Restaurant WHERE RestaurantId = @Id;";
        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private static RestaurantResponse MapRow(SqlDataReader r) => new(
        r.GetInt32(0), r.GetString(1), r.GetString(2),
        r.IsDBNull(3) ? null : r.GetString(3),
        r.IsDBNull(4) ? null : r.GetString(4),
        r.GetDateTime(5));

    private static void AddParams(SqlCommand cmd, string name, string address,
        string? email, string? phone)
    {
        cmd.Parameters.AddWithValue("@Name",    name);
        cmd.Parameters.AddWithValue("@Address", address);
        cmd.Parameters.AddWithValue("@Email",   (object?)email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Phone",   (object?)phone ?? DBNull.Value);
    }
}
