using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmToTable.Api.Controllers;

[ApiController]
[Route("api/restaurants")]
public class RestaurantsController(RestaurantService service) : ControllerBase
{
    /// <summary>Returns all restaurants.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Returns a single restaurant by ID.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        try   { return Ok(await service.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>Creates a new restaurant.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRestaurantRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await service.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.RestaurantId }, created);
    }

    /// <summary>Updates an existing restaurant.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRestaurantRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try   { return Ok(await service.UpdateAsync(id, req)); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>Deletes a restaurant.</summary>
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
