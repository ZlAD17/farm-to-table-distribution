using FarmToTable.Api.Data;
using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Models.Responses;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

public class OrderRepository(DatabaseHelper db, HarvestBatchRepository batchRepo)
{
    // ── Read ─────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<OrderResponse>> GetAllAsync()
    {
        const string sql = """
            SELECT o.OrderId, o.RestaurantId, r.Name AS RestaurantName,
                   o.DriverId, d.FullName AS DriverName,
                   o.Status, o.OrderedAt, o.DeliveredAt, o.Notes
            FROM PurchaseOrder o
            INNER JOIN Restaurant r ON r.RestaurantId = o.RestaurantId
            LEFT  JOIN Driver     d ON d.DriverId     = o.DriverId
            ORDER BY o.OrderedAt DESC;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        var orders = new List<(OrderResponse Header, int OrderId)>();
        while (await reader.ReadAsync())
            orders.Add((MapHeader(reader, []), reader.GetInt32(0)));

        // Load batches per order
        var result = new List<OrderResponse>();
        foreach (var (header, orderId) in orders)
        {
            var batches = await GetBatchesForOrderAsync(conn, orderId);
            var total   = batches.Sum(b => b.LineTotal);
            result.Add(header with { Batches = batches, TotalAmount = total });
        }
        return result;
    }

    public async Task<OrderResponse?> GetByIdAsync(int orderId)
    {
        const string sql = """
            SELECT o.OrderId, o.RestaurantId, r.Name AS RestaurantName,
                   o.DriverId, d.FullName AS DriverName,
                   o.Status, o.OrderedAt, o.DeliveredAt, o.Notes
            FROM PurchaseOrder o
            INNER JOIN Restaurant r ON r.RestaurantId = o.RestaurantId
            LEFT  JOIN Driver     d ON d.DriverId     = o.DriverId
            WHERE o.OrderId = @OrderId;
            """;

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;

        var batches = await GetBatchesForOrderAsync(conn, orderId);
        var header  = MapHeader(reader, batches);
        return header with { TotalAmount = batches.Sum(b => b.LineTotal) };
    }

    // ── Create (transactional) ────────────────────────────────────────────────

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest req)
    {
        await using var conn = await db.OpenConnectionAsync();
        await using var tx   = conn.BeginTransaction();

        try
        {
            // 1. Insert order header
            const string insertOrder = """
                INSERT INTO PurchaseOrder (RestaurantId, DriverId, Notes)
                OUTPUT INSERTED.OrderId
                VALUES (@RestaurantId, @DriverId, @Notes);
                """;

            await using var orderCmd = new SqlCommand(insertOrder, conn, tx);
            orderCmd.Parameters.AddWithValue("@RestaurantId", req.RestaurantId);
            orderCmd.Parameters.AddWithValue("@DriverId",     (object?)req.DriverId ?? DBNull.Value);
            orderCmd.Parameters.AddWithValue("@Notes",        (object?)req.Notes    ?? DBNull.Value);

            var orderId = (int)(await orderCmd.ExecuteScalarAsync())!;

            // 2. For each batch line: validate stock, snapshot price, insert OrderBatch, decrement stock
            const string insertBatch = """
                INSERT INTO OrderBatch (OrderId, BatchId, QuantityOrdered, UnitPrice)
                VALUES (@OrderId, @BatchId, @Qty, @Price);
                """;

            foreach (var line in req.Batches)
            {
                var price = await batchRepo.GetPricePerUnitAsync(conn, tx, line.BatchId);
                await batchRepo.DecrementQuantityAsync(conn, tx, line.BatchId, line.QuantityOrdered);

                await using var batchCmd = new SqlCommand(insertBatch, conn, tx);
                batchCmd.Parameters.AddWithValue("@OrderId", orderId);
                batchCmd.Parameters.AddWithValue("@BatchId", line.BatchId);
                batchCmd.Parameters.AddWithValue("@Qty",     line.QuantityOrdered);
                batchCmd.Parameters.AddWithValue("@Price",   price);
                await batchCmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            return (await GetByIdAsync(orderId))!;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Update ────────────────────────────────────────────────────────────────

    public async Task<OrderResponse?> UpdateAsync(int orderId, UpdateOrderRequest req)
    {
        const string sql = """
            UPDATE PurchaseOrder
            SET Status      = @Status,
                DriverId    = @DriverId,
                Notes       = @Notes,
                DeliveredAt = CASE WHEN @Status = 'Delivered' THEN SYSUTCDATETIME() ELSE DeliveredAt END
            WHERE OrderId = @OrderId;
            """;

        await using var conn = await db.OpenConnectionAsync();
        await using var cmd  = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Status",   req.Status);
        cmd.Parameters.AddWithValue("@DriverId", (object?)req.DriverId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Notes",    (object?)req.Notes    ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@OrderId",  orderId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0 ? await GetByIdAsync(orderId) : null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<IReadOnlyList<OrderBatchResponse>> GetBatchesForOrderAsync(
        SqlConnection conn, int orderId)
    {
        const string sql = """
            SELECT ob.OrderBatchId, ob.BatchId,
                   c.Name  AS CropName,
                   f.Name  AS FarmName,
                   ob.QuantityOrdered, ob.UnitPrice
            FROM OrderBatch ob
            INNER JOIN HarvestBatch hb ON hb.BatchId = ob.BatchId
            INNER JOIN Crop         c  ON c.CropId   = hb.CropId
            INNER JOIN Farm         f  ON f.FarmId   = hb.FarmId
            WHERE ob.OrderId = @OrderId;
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@OrderId", orderId);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<OrderBatchResponse>();
        while (await reader.ReadAsync())
        {
            var qty   = reader.GetDecimal(4);
            var price = reader.GetDecimal(5);
            list.Add(new OrderBatchResponse(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetString(3),
                qty, price,
                Math.Round(qty * price, 2)));
        }
        return list;
    }

    private static OrderResponse MapHeader(SqlDataReader r,
        IReadOnlyList<OrderBatchResponse> batches) => new(
        r.GetInt32(0),
        r.GetInt32(1),
        r.GetString(2),
        r.IsDBNull(3) ? null : r.GetInt32(3),
        r.IsDBNull(4) ? null : r.GetString(4),
        r.GetString(5),
        r.GetDateTime(6),
        r.IsDBNull(7) ? null : r.GetDateTime(7),
        r.IsDBNull(8) ? null : r.GetString(8),
        0m,   // placeholder; will be replaced with actual sum after batches load
        batches
    );
}
