using Microsoft.AspNetCore.Mvc;
using SolicitorScraper.Application.Searches;

namespace SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/searches")]
public class SearchesController : ControllerBase
{
    private readonly ISearchService _searches;

    public SearchesController(ISearchService searches)
    {
        _searches = searches;
    }

    [HttpPost]
    public async Task<IActionResult> Run(CancellationToken ct) =>
        Ok(await _searches.RunAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _searches.GetRunsAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Ok(await _searches.GetRunAsync(id, ct));

    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> GetReport(int id, CancellationToken ct) =>
        Ok(await _searches.GetReportAsync(id, ct));
}
