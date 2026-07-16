using Microsoft.Extensions.Logging;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;
using SolicitorScraper.Domain.Scraping;
using SolicitorScraper.Domain.Services;

namespace SolicitorScraper.Infrastructure.Services;

public class ScrapeService : IScrapeService
{
    private const int MaxConcurrentRequests = 3;

    private readonly IResultsPageClient _client;
    private readonly IResultsPageParser _parser;
    private readonly ILocationRepository _locations;
    private readonly ISearchRunRepository _runs;
    private readonly ILogger<ScrapeService> _logger;

    public ScrapeService(
        IResultsPageClient client,
        IResultsPageParser parser,
        ILocationRepository locations,
        ISearchRunRepository runs,
        ILogger<ScrapeService> logger)
    {
        _client = client;
        _parser = parser;
        _locations = locations;
        _runs = runs;
        _logger = logger;
    }

    public async Task<SearchRun> RunAsync(CancellationToken ct = default)
    {
        var enabled = (await _locations.GetAllAsync(ct)).Where(l => l.IsEnabled).ToList();
        if (enabled.Count == 0)
            throw new InvalidOperationException("No locations are enabled for scraping.");

        var run = new SearchRun { StartedAt = DateTime.UtcNow };

        using var throttle = new SemaphoreSlim(MaxConcurrentRequests);
        var tasks = enabled.Select(location => ScrapeLocationAsync(location.Name, throttle, ct));
        var outcomes = await Task.WhenAll(tasks);

        foreach (var (location, solicitors) in outcomes)
        {
            if (solicitors is null)
            {
                run.FailedLocations.Add(location);
                continue;
            }

            run.Results.AddRange(solicitors.Select(s => new SolicitorResult
            {
                Location = location,
                Name = s.Name,
                Phone = s.Phone,
                Address = s.Address,
                Website = s.Website,
                ProfileUrl = s.ProfileUrl,
                Description = s.Description,
                QualityMarks = s.QualityMarks,
                Rating = s.Rating,
                ReviewCount = s.ReviewCount,
                IsFeatured = s.IsFeatured
            }));
        }

        run.CompletedAt = DateTime.UtcNow;
        return await _runs.AddAsync(run, ct);
    }

    private async Task<(string Location, IReadOnlyList<ScrapedSolicitor>? Solicitors)> ScrapeLocationAsync(
        string location, SemaphoreSlim throttle, CancellationToken ct)
    {
        await throttle.WaitAsync(ct);
        try
        {
            var html = await _client.GetResultsPageAsync(location, ct);
            return (location, _parser.Parse(html));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to scrape results for {Location}", location);
            return (location, null);
        }
        finally
        {
            throttle.Release();
        }
    }
}
