using FarmToTable.Api.Data;
using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Repositories;

// ── Shared report row shapes ──────────────────────────────────────────────────

public record TopCropResult(int CropId, string CropName, int OrderCount);
public record InactiveFarmResult(int FarmId, string FarmName, string Location);
public record TopDriverResult(int DriverId, string DriverName, int TripCount);
public record InactiveRestaurantResult(int RestaurantId, string RestaurantName);
public record RestaurantBatchResult(int RestaurantId, string RestaurantName,
    int BatchId, string CropName, string FarmName, decimal QuantityOrdered);
public record FarmRevenueResult(int FarmId, string FarmName, decimal TotalRevenue);

// ─────────────────────────────────────────────────────────────────────────────

public class ReportRepository(DatabaseHelper db)
{
    /// <summary>
    /// Crop type with the maximum number of restaurant orders in a date range.
    /// </summary>
    public async Task<IReadOnlyList<TopCropResult>> GetTopCropByOrdersAsync(
        DateOnly from, DateOnly to)
    {
        const string sql = """
            SELECT TOP 1 WITH TIES
                c.CropId,
                c.Name AS CropName,
                COUNT(DISTINCT ob.OrderId) AS OrderCount
            FROM OrderBatch ob
            INNER JOIN HarvestBatch hb ON hb.BatchId       = ob.BatchId
            INNER JOIN Crop         c  ON c.CropId         = hb.CropId
            INNER JOIN PurchaseOrder o ON o.OrderId        = ob.OrderId
            WHERE CAST(o.OrderedAt AS DATE) BETWEEN @From AND @To
            GROUP BY c.CropId, c.Name
            ORDER BY OrderCount DESC;
            """;

        return await RunAsync(sql,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@From", from.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@To",   to.ToString("yyyy-MM-dd"));
            },
            r => new TopCropResult(r.GetInt32(0), r.GetString(1), r.GetInt32(2)));
    }

    /// <summary>
    /// Farms with no harvest batches listed or sold in a given month/year.
    /// </summary>
    public async Task<IReadOnlyList<InactiveFarmResult>> GetInactiveFarmsAsync(int year, int month)
    {
        const string sql = """
            SELECT f.FarmId, f.Name, f.Location
            FROM Farm f
            WHERE NOT EXISTS (
                SELECT 1
                FROM HarvestBatch hb
                WHERE hb.FarmId = f.FarmId
                  AND YEAR(hb.HarvestDate)  = @Year
                  AND MONTH(hb.HarvestDate) = @Month
            )
            ORDER BY f.Name;
            """;

        return await RunAsync(sql,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@Year",  year);
                cmd.Parameters.AddWithValue("@Month", month);
            },
            r => new InactiveFarmResult(r.GetInt32(0), r.GetString(1), r.GetString(2)));
    }

    /// <summary>
    /// Driver with the highest number of delivery trips in a given month/year.
    /// </summary>
    public async Task<IReadOnlyList<TopDriverResult>> GetTopDriverAsync(int year, int month)
    {
        const string sql = """
            SELECT TOP 1 WITH TIES
                d.DriverId,
                d.FullName,
                COUNT(*) AS TripCount
            FROM DeliveryTrip dt
            INNER JOIN Driver d ON d.DriverId = dt.DriverId
            WHERE YEAR(dt.ScheduledAt)  = @Year
              AND MONTH(dt.ScheduledAt) = @Month
            GROUP BY d.DriverId, d.FullName
            ORDER BY TripCount DESC;
            """;

        return await RunAsync(sql,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@Year",  year);
                cmd.Parameters.AddWithValue("@Month", month);
            },
            r => new TopDriverResult(r.GetInt32(0), r.GetString(1), r.GetInt32(2)));
    }

    /// <summary>
    /// Restaurants that did not place any orders in a given month/year.
    /// </summary>
    public async Task<IReadOnlyList<InactiveRestaurantResult>> GetInactiveRestaurantsAsync(
        int year, int month)
    {
        const string sql = """
            SELECT r.RestaurantId, r.Name
            FROM Restaurant r
            WHERE NOT EXISTS (
                SELECT 1
                FROM PurchaseOrder o
                WHERE o.RestaurantId = r.RestaurantId
                  AND YEAR(o.OrderedAt)  = @Year
                  AND MONTH(o.OrderedAt) = @Month
            )
            ORDER BY r.Name;
            """;

        return await RunAsync(sql,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@Year",  year);
                cmd.Parameters.AddWithValue("@Month", month);
            },
            r => new InactiveRestaurantResult(r.GetInt32(0), r.GetString(1)));
    }

    /// <summary>
    /// All harvest batches delivered to each restaurant in a given month/year.
    /// </summary>
    public async Task<IReadOnlyList<RestaurantBatchResult>> GetBatchesDeliveredPerRestaurantAsync(
        int year, int month)
    {
        const string sql = """
            SELECT r.RestaurantId, r.Name AS RestaurantName,
                   ob.BatchId,
                   c.Name  AS CropName,
                   f.Name  AS FarmName,
                   ob.QuantityOrdered
            FROM PurchaseOrder o
            INNER JOIN Restaurant   r  ON r.RestaurantId = o.RestaurantId
            INNER JOIN OrderBatch   ob ON ob.OrderId     = o.OrderId
            INNER JOIN HarvestBatch hb ON hb.BatchId     = ob.BatchId
            INNER JOIN Crop         c  ON c.CropId       = hb.CropId
            INNER JOIN Farm         f  ON f.FarmId       = hb.FarmId
            WHERE o.Status = 'Delivered'
              AND YEAR(o.DeliveredAt)  = @Year
              AND MONTH(o.DeliveredAt) = @Month
            ORDER BY r.Name, c.Name;
            """;

        return await RunAsync(sql,
            cmd =>
            {
                cmd.Parameters.AddWithValue("@Year",  year);
                cmd.Parameters.AddWithValue("@Month", month);
            },
            r => new RestaurantBatchResult(
                r.GetInt32(0), r.GetString(1),
                r.GetInt32(2), r.GetString(3),
                r.GetString(4), r.GetDecimal(5)));
    }

    /// <summary>
    /// Total revenue per farm from sold harvest batches (quantity ordered × unit price).
    /// </summary>
    public async Task<IReadOnlyList<FarmRevenueResult>> GetFarmRevenueAsync()
    {
        const string sql = """
            SELECT f.FarmId,
                   f.Name AS FarmName,
                   COALESCE(SUM(ob.QuantityOrdered * ob.UnitPrice), 0) AS TotalRevenue
            FROM Farm f
            LEFT JOIN HarvestBatch hb ON hb.FarmId  = f.FarmId
            LEFT JOIN OrderBatch   ob ON ob.BatchId  = hb.BatchId
            LEFT JOIN PurchaseOrder o ON o.OrderId   = ob.OrderId
                                     AND o.Status   <> 'Cancelled'
            GROUP BY f.FarmId, f.Name
            ORDER BY TotalRevenue DESC;
            """;

        return await RunAsync(sql,
            _ => { },
            r => new FarmRevenueResult(r.GetInt32(0), r.GetString(1), r.GetDecimal(2)));
    }

    // ── Generic execution helper ──────────────────────────────────────────────

    private async Task<IReadOnlyList<T>> RunAsync<T>(
        string sql,
        Action<SqlCommand> parameterize,
        Func<SqlDataReader, T> map)
    {
        await using var conn   = await db.OpenConnectionAsync();
        await using var cmd    = new SqlCommand(sql, conn);
        parameterize(cmd);
        await using var reader = await cmd.ExecuteReaderAsync();

        var list = new List<T>();
        while (await reader.ReadAsync()) list.Add(map(reader));
        return list;
    }
}
