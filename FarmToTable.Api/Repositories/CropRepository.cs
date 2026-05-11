using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class CropRepository(DatabaseHelper db)
{
    public async Task<IReadOnlyList<CropResponse>> GetAllAsync()
    {
        const string sql = "SELECT CropId, Name, Unit FROM Crop ORDER BY Name;";

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<CropResponse>();
        while (await reader.ReadAsync())
            list.Add(new CropResponse(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
        return list;
    }

    public async Task<CropResponse> CreateAsync(CreateCropRequest req)
    {
        const string sql = """
            INSERT INTO Crop (Name, Unit)
            OUTPUT INSERTED.CropId, INSERTED.Name, INSERTED.Unit
            VALUES (@Name, @Unit);
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", req.Name);
        cmd.Parameters.AddWithValue("@Unit", req.Unit);

        await using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new CropResponse(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
    }
}
