using FarmToTable.Api.Models.Requests;
using FarmToTable.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FarmToTable.Api.Controllers;

[ApiController]
[Route("api/crops")]
public class CropsController(CropService service) : ControllerBase
{
    /// <summary>Returns all crop types.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Creates a new crop type.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCropRequest req)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await service.CreateAsync(req);
        return StatusCode(201, created);
    }
}
