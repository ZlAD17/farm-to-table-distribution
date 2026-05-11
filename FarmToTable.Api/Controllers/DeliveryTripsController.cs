using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmToTable.Api.Controllers;

[ApiController]
[Route("api/delivery-trips")]
public class DeliveryTripsController(DeliveryTripService service) : ControllerBase
{
    /// <summary>Returns all delivery trips.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Returns a single delivery trip by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try   { return Ok(await service.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>Schedules a new delivery trip.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDeliveryTripRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.TripId }, created);
    }

    /// <summary>Marks a delivery trip as completed.</summary>
    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteDeliveryTripRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try   { return Ok(await service.CompleteAsync(id, req)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
