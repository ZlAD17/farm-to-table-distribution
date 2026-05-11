using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmToTable.Api.Controllers;

[ApiController]
[Route("api/harvest-batches")]
public class HarvestBatchesController(HarvestBatchService service) : ControllerBase
{
    /// <summary>
    /// Returns all harvest batches. Supports optional query filters:
    /// farmId, cropId, fromDate (yyyy-MM-dd), toDate (yyyy-MM-dd), status.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int?      farmId,
        [FromQuery] int?      cropId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] string?   status)
    {
        var filter = new HarvestBatchFilterRequest(farmId, cropId, fromDate, toDate, status);
        return Ok(await service.GetAllAsync(filter));
    }

    /// <summary>Returns a single harvest batch by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try   { return Ok(await service.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>Creates a new harvest batch.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHarvestBatchRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.BatchId }, created);
    }

    /// <summary>Updates price, available quantity, or status of a harvest batch.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHarvestBatchRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var allowed = new[] { "Available", "Sold", "Expired" };
        if (!allowed.Contains(req.Status))
            return BadRequest(new { message = $"Status must be one of: {string.Join(", ", allowed)}" });

        try   { return Ok(await service.UpdateAsync(id, req)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>Deletes a harvest batch.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
