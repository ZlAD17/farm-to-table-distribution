using FarmToTable.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmToTable.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ReportService service) : ControllerBase
{
    /// <summary>
    /// Crop type with the maximum number of restaurant orders in a date range.
    /// Query params: from (yyyy-MM-dd), to (yyyy-MM-dd).
    /// </summary>
    [HttpGet("top-crop")]
    public async Task<IActionResult> TopCrop(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        if (from > to)
            return BadRequest(new { message = "'from' must be before or equal to 'to'." });

        return Ok(await service.GetTopCropAsync(from, to));
    }

    /// <summary>
    /// Farms with no harvest batches listed or sold in a given month.
    /// Query params: year, month (1-12).
    /// </summary>
    [HttpGet("inactive-farms")]
    public async Task<IActionResult> InactiveFarms(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (!IsValidYearMonth(year, month, out var err))
            return BadRequest(new { message = err });

        return Ok(await service.GetInactiveFarmsAsync(year, month));
    }

    /// <summary>
    /// Driver with the highest number of delivery trips in a given month.
    /// Query params: year, month (1-12).
    /// </summary>
    [HttpGet("top-driver")]
    public async Task<IActionResult> TopDriver(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (!IsValidYearMonth(year, month, out var err))
            return BadRequest(new { message = err });

        return Ok(await service.GetTopDriverAsync(year, month));
    }

    /// <summary>
    /// Restaurants that did not place any produce orders in a given month.
    /// Query params: year, month (1-12).
    /// </summary>
    [HttpGet("inactive-restaurants")]
    public async Task<IActionResult> InactiveRestaurants(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (!IsValidYearMonth(year, month, out var err))
            return BadRequest(new { message = err });

        return Ok(await service.GetInactiveRestaurantsAsync(year, month));
    }

    /// <summary>
    /// All harvest batches delivered to each restaurant in a given month.
    /// Query params: year, month (1-12).
    /// </summary>
    [HttpGet("restaurant-batches")]
    public async Task<IActionResult> RestaurantBatches(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (!IsValidYearMonth(year, month, out var err))
            return BadRequest(new { message = err });

        return Ok(await service.GetBatchesDeliveredPerRestaurantAsync(year, month));
    }

    /// <summary>
    /// Total revenue per farm from sold (non-cancelled) harvest batches.
    /// </summary>
    [HttpGet("farm-revenue")]
    public async Task<IActionResult> FarmRevenue() =>
        Ok(await service.GetFarmRevenueAsync());

    // ── helpers ───────────────────────────────────────────────────────────────

    private static bool IsValidYearMonth(int year, int month, out string error)
    {
        if (year < 2000 || year > 2100)
        {
            error = "Year must be between 2000 and 2100.";
            return false;
        }
        if (month < 1 || month > 12)
        {
            error = "Month must be between 1 and 12.";
            return false;
        }
        error = string.Empty;
        return true;
    }
}
