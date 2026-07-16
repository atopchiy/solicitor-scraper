using Microsoft.AspNetCore.Mvc;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;
using SolicitorScraper.Domain.Services;

namespace SolicitorScraper.Api.Controllers;

[ApiController]
[Route("api/searches")]
public class SearchesController : ControllerBase
{
    private readonly IScrapeService _scraper;
    private readonly IReportService _reports;
    private readonly ISearchRunRepository _runs;

    public SearchesController(IScrapeService scraper, IReportService reports, ISearchRunRepository runs)
    {
        _scraper = scraper;
        _reports = reports;
        _runs = runs;
    }

    [HttpPost]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        SearchRun run;
        try
        {
            run = await _scraper.RunAsync(ct);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return Ok(new SearchRunSummary(
            run.Id, run.StartedAt, run.CompletedAt, run.Results.Count, run.FailedLocations));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _runs.GetSummariesAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var run = await _runs.GetByIdAsync(id, ct);
        return run is null ? NotFound() : Ok(run);
    }

    [HttpGet("{id:int}/report")]
    public async Task<IActionResult> GetReport(int id, CancellationToken ct)
    {
        var report = await _reports.BuildReportAsync(id, ct);
        return report is null ? NotFound() : Ok(report);
    }
}
