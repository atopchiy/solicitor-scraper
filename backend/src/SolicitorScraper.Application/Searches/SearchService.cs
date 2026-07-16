using SolicitorScraper.Application.Exceptions;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Application.Searches;

public class SearchService : ISearchService
{
    private readonly IScrapeService _scraper;
    private readonly IReportService _reports;
    private readonly ISearchRunRepository _runs;

    public SearchService(IScrapeService scraper, IReportService reports, ISearchRunRepository runs)
    {
        _scraper = scraper;
        _reports = reports;
        _runs = runs;
    }

    public async Task<SearchRunSummary> RunAsync(CancellationToken ct = default)
    {
        var run = await _scraper.RunAsync(ct);
        return new SearchRunSummary(
            run.Id, run.StartedAt, run.CompletedAt, run.Results.Count, run.FailedLocations);
    }

    public Task<List<SearchRunSummary>> GetRunsAsync(CancellationToken ct = default) =>
        _runs.GetSummariesAsync(ct);

    public async Task<SearchRun> GetRunAsync(int id, CancellationToken ct = default)
    {
        var run = await _runs.GetByIdAsync(id, ct);
        return run ?? throw new NotFoundException($"Search run {id} was not found.");
    }

    public async Task<RunReport> GetReportAsync(int id, CancellationToken ct = default)
    {
        var report = await _reports.BuildReportAsync(id, ct);
        return report ?? throw new NotFoundException($"Search run {id} was not found.");
    }
}
