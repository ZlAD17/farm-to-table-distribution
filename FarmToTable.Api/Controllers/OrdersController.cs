using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmToTable.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderService service) : ControllerBase
{
    /// <summary>Returns all orders (with their batch lines).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Returns a single order with all its batch lines.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try   { return Ok(await service.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>
    /// Creates a new purchase order with one or more harvest batch lines.
    /// Stock is decremented atomically within a transaction.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var created = await service.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
        }
        catch (InvalidOperationException ex)
        {
            // Insufficient stock
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            // Batch or restaurant not found
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Updates the status, driver, or notes of an existing order.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try   { return Ok(await service.UpdateAsync(id, req)); }
        catch (KeyNotFoundException ex)  { return NotFound(new { message = ex.Message }); }
        catch (ArgumentException ex)     { return BadRequest(new { message = ex.Message }); }
    }
}
