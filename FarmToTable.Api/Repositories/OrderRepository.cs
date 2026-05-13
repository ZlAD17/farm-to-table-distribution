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

        // Step 1: collect header rows — reader must be fully consumed and closed
        // before we can issue the secondary batch-line query on the same connection.
        var headers = new List<(int OrderId, int RestaurantId, string RestaurantName,
            int? DriverId, string? DriverName, string Status,
            DateTime OrderedAt, DateTime? DeliveredAt, string? Notes)>();

        await using (var conn = await db.OpenConnectionAsync())
        await using (var cmd  = new SqlCommand(sql, conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                headers.Add((
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    reader.IsDBNull(4) ? null : reader.GetString(4),
                    reader.GetString(5),
                    reader.GetDateTime(6),
                    reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                    reader.IsDBNull(8) ? null : reader.GetString(8)
                ));
            }
        } // reader and conn disposed here

        // Step 2: load batches for each order (separate connection per order is fine
        // because ADO.NET connection pooling makes this cheap).
        var result = new List<OrderResponse>();
        foreach (var h in headers)
        {
            var batches = await GetBatchesForOrderAsync(h.OrderId);
            result.Add(new OrderResponse(
                h.OrderId, h.RestaurantId, h.RestaurantName,
                h.DriverId, h.DriverName, h.Status,
                h.OrderedAt, h.DeliveredAt, h.Notes,
                batches.Sum(b => b.LineTotal),
                batches));
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

        // Read header into local variables, then close reader before secondary query.
        int restId; string restName; int? driverId; string? driverName;
        string status; DateTime orderedAt; DateTime? deliveredAt; string? notes;
        bool found;

        await using (var conn   = await db.OpenConnectionAsync())
        await using (var cmd    = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@OrderId", orderId);
            await using var reader = await cmd.ExecuteReaderAsync();
            found = await reader.ReadAsync();
            if (!found) return null;

            restId      = reader.GetInt32(1);
            restName    = reader.GetString(2);
            driverId    = reader.IsDBNull(3) ? null : reader.GetInt32(3);
            driverName  = reader.IsDBNull(4) ? null : reader.GetString(4);
            status      = reader.GetString(5);
            orderedAt   = reader.GetDateTime(6);
            deliveredAt = reader.IsDBNull(7) ? null : reader.GetDateTime(7);
            notes       = reader.IsDBNull(8) ? null : reader.GetString(8);
        } // reader and conn disposed here

        var batches = await GetBatchesForOrderAsync(orderId);
        return new OrderResponse(orderId, restId, restName, driverId, driverName,
            status, orderedAt, deliveredAt, notes,
            batches.Sum(b => b.LineTotal), batches);
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

    private async Task<IReadOnlyList<OrderBatchResponse>> GetBatchesForOrderAsync(int orderId)
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

        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
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
}
