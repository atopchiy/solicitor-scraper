using Microsoft.AspNetCore.Mvc;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationRepository _locations;

    public LocationsController(ILocationRepository locations)
    {
        _locations = locations;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _locations.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddLocationRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
            return BadRequest(new { error = "Location name is required." });

        if (await _locations.GetByNameAsync(name, ct) is not null)
            return Conflict(new { error = $"Location '{name}' already exists." });

        var location = await _locations.AddAsync(name, ct);
        return CreatedAtAction(nameof(GetAll), new { location.Id }, location);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct) =>
        await _locations.DeleteAsync(id, ct) ? NoContent() : NotFound();

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> SetEnabled(int id, [FromBody] SetEnabledRequest request, CancellationToken ct)
    {
        var location = await _locations.SetEnabledAsync(id, request.IsEnabled, ct);
        return location is null ? NotFound() : Ok(location);
    }

    public record AddLocationRequest(string? Name);

    public record SetEnabledRequest(bool IsEnabled);
}
