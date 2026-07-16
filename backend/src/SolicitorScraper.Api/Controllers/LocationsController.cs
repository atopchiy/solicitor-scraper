using Microsoft.AspNetCore.Mvc;
using SolicitorScraper.Application.Locations;

namespace SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locations;

    public LocationsController(ILocationService locations)
    {
        _locations = locations;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _locations.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddLocationRequest request, CancellationToken ct)
    {
        var location = await _locations.AddAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), new { location.Id }, location);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _locations.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> SetEnabled(int id, [FromBody] SetLocationEnabledRequest request, CancellationToken ct) =>
        Ok(await _locations.SetEnabledAsync(id, request, ct));
}
