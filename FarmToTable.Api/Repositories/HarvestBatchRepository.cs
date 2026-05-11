using System.Text;
using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class HarvestBatchRepository(DatabaseHelper db)
{
    private const string SelectBase = """
        SELECT hb.BatchId, hb.FarmId, f.Name AS FarmName,
               hb.CropId, c.Name AS CropName, c.Unit AS CropUnit,
               hb.QuantityAvailable, hb.QuantityRemaining,
               hb.HarvestDate, hb.PricePerUnit, hb.Status, hb.CreatedAt
        FROM HarvestBatch hb
        INNER JOIN Farm f ON f.FarmId = hb.FarmId
        INNER JOIN Crop c ON c.CropId = hb.CropId
        """;

    public async Task<IReadOnlyList<HarvestBatchResponse>> GetAllAsync(HarvestBatchFilterRequest filter)
    {
        var sql = new StringBuilder(SelectBase);
        var conditions = new List<string>();

        if (filter.FarmId.HasValue)   conditions.Add("hb.FarmId = @FarmId");
        if (filter.CropId.HasValue)   conditions.Add("hb.CropId = @CropId");
        if (filter.FromDate.HasValue) conditions.Add("hb.HarvestDate >= @FromDate");
        if (filter.ToDate.HasValue)   conditions.Add("hb.HarvestDate <= @ToDate");
        if (!string.IsNullOrWhiteSpace(filter.Status)) conditions.Add("hb.Status = @Status");

        if (conditions.Count > 0)
            sql.Append(" WHERE ").Append(string.Join(" AND ", conditions));

        sql.Append(" ORDER BY hb.HarvestDate DESC;");

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql.ToString(), conn);

        if (filter.FarmId.HasValue)   cmd.Parameters.AddWithValue("@FarmId",   filter.FarmId.Value);
        if (filter.CropId.HasValue)   cmd.Parameters.AddWithValue("@CropId",   filter.CropId.Value);
        if (filter.FromDate.HasValue) cmd.Parameters.AddWithValue("@FromDate",  filter.FromDate.Value.ToString("yyyy-MM-dd"));
        if (filter.ToDate.HasValue)   cmd.Parameters.AddWithValue("@ToDate",    filter.ToDate.Value.ToString("yyyy-MM-dd"));
        if (!string.IsNullOrWhiteSpace(filter.Status)) cmd.Parameters.AddWithValue("@Status", filter.Status);

        await using var reader = await cmd.ExecuteReaderAsync();
        var list = new List<HarvestBatchResponse>();
        while (await reader.ReadAsync()) list.Add(MapRow(reader));
        return list;
    }

    public async Task<HarvestBatchResponse?> GetByIdAsync(int batchId)
    {
        var sql = SelectBase + " WHERE hb.BatchId = @BatchId;";

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@BatchId", batchId);
        await using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapRow(reader) : null;
    }

    public async Task<HarvestBatchResponse> CreateAsync(CreateHarvestBatchRequest req)
    {
        const string sql = """
            INSERT INTO HarvestBatch
                (FarmId, CropId, QuantityAvailable, QuantityRemaining, HarvestDate, PricePerUnit)
            OUTPUT INSERTED.BatchId
            VALUES (@FarmId, @CropId, @Qty, @Qty, @HarvestDate, @Price);
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@FarmId",      req.FarmId);
        cmd.Parameters.AddWithValue("@CropId",      req.CropId);
        cmd.Parameters.AddWithValue("@Qty",         req.QuantityAvailable);
        cmd.Parameters.AddWithValue("@HarvestDate", req.HarvestDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@Price",       req.PricePerUnit);

        var newId = (int)(await cmd.ExecuteScalarAsync())!;
        return (await GetByIdAsync(newId))!;
    }

    public async Task<HarvestBatchResponse?> UpdateAsync(int batchId, UpdateHarvestBatchRequest req)
    {
        const string sql = """
            UPDATE HarvestBatch
            SET QuantityAvailable = @Qty, PricePerUnit = @Price, Status = @Status
            WHERE BatchId = @BatchId;
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Qty",     req.QuantityAvailable);
        cmd.Parameters.AddWithValue("@Price",   req.PricePerUnit);
        cmd.Parameters.AddWithValue("@Status",  req.Status);
        cmd.Parameters.AddWithValue("@BatchId", batchId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0 ? await GetByIdAsync(batchId) : null;
    }

    public async Task<bool> DeleteAsync(int batchId)
    {
        const string sql = "DELETE FROM HarvestBatch WHERE BatchId = @BatchId;";
        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@BatchId", batchId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    // Called within a transaction when an order is being created
    public async Task<decimal> GetPricePerUnitAsync(SqlConnection conn, SqlTransaction tx, int batchId)
    {
        const string sql = "SELECT PricePerUnit FROM HarvestBatch WHERE BatchId = @BatchId;";
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@BatchId", batchId);
        var result = await cmd.ExecuteScalarAsync();
        return result == null ? throw new KeyNotFoundException($"Batch {batchId} not found.") : (decimal)result;
    }

    public async Task DecrementQuantityAsync(SqlConnection conn, SqlTransaction tx, int batchId, decimal qty)
    {
        const string sql = """
            UPDATE HarvestBatch
            SET QuantityRemaining = QuantityRemaining - @Qty
            WHERE BatchId = @BatchId AND QuantityRemaining >= @Qty;
            """;
        await using var cmd = new SqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("@Qty",     qty);
        cmd.Parameters.AddWithValue("@BatchId", batchId);

        var rows = await cmd.ExecuteNonQueryAsync();
        if (rows == 0)
            throw new InvalidOperationException(
                $"Insufficient quantity remaining in batch {batchId}.");
    }

    private static HarvestBatchResponse MapRow(SqlDataReader r) => new(
        r.GetInt32(0),
        r.GetInt32(1),
        r.GetString(2),
        r.GetInt32(3),
        r.GetString(4),
        r.GetString(5),
        r.GetDecimal(6),
        r.GetDecimal(7),
        DateOnly.FromDateTime(r.GetDateTime(8)),
        r.GetDecimal(9),
        r.GetString(10),
        r.GetDateTime(11)
    );
}
