using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Reports;
using SolicitorScraper.Domain.Repositories;
using SolicitorScraper.Domain.Services;

namespace SolicitorScraper.Infrastructure.Services;

public class ReportService : IReportService
{
    private const int TopRatedLimit = 10;

    private readonly ISearchRunRepository _runs;

    public ReportService(ISearchRunRepository runs)
    {
        _runs = runs;
    }

    public async Task<RunReport?> BuildReportAsync(int runId, CancellationToken ct = default)
    {
        var run = await _runs.GetByIdAsync(runId, ct);
        if (run is null) return null;

        var previous = await _runs.GetPreviousRunAsync(run.Id, ct);

        return new RunReport(
            run.Id,
            run.StartedAt,
            run.Results.Count,
            run.Results.Select(r => r.Location).Distinct().Count(),
            AverageRating(run.Results),
            run.Results.Count(r => r.QualityMarks is not null),
            run.Results.Count(r => r.IsFeatured),
            BuildLocationSummaries(run),
            BuildTopRated(run),
            FindNewSolicitors(run, previous),
            run.FailedLocations);
    }

    private static decimal? AverageRating(IEnumerable<SolicitorResult> results)
    {
        var rated = results.Where(r => r.Rating.HasValue).Select(r => r.Rating!.Value).ToList();
        return rated.Count == 0 ? null : Math.Round(rated.Average(), 2);
    }

    private static List<LocationSummary> BuildLocationSummaries(SearchRun run) =>
        run.Results
            .GroupBy(r => r.Location)
            .Select(g => new LocationSummary(
                g.Key,
                g.Count(),
                AverageRating(g),
                g.Count(r => r.IsFeatured),
                g.Count(r => r.QualityMarks is not null)))
            .OrderByDescending(s => s.Count)
            .ToList();

    private static List<RankedFirm> BuildTopRated(SearchRun run) =>
        run.Results
            .Where(r => r.Rating.HasValue && r.ReviewCount is > 0)
            .GroupBy(r => r.Name)
            .Select(g =>
            {
                var rating = g.Max(r => r.Rating!.Value);
                var reviews = g.Max(r => r.ReviewCount!.Value);
                return new RankedFirm(
                    g.Key,
                    g.Select(r => r.Location).Distinct().Count(),
                    rating,
                    reviews,
                    // Weight the rating by review volume so a 5.0 with 3 reviews
                    // doesn't outrank a 4.8 with 900.
                    Math.Round((double)rating * Math.Log10(reviews + 1), 2));
            })
            .OrderByDescending(f => f.Score)
            .Take(TopRatedLimit)
            .ToList();

    private static List<NewSolicitor> FindNewSolicitors(SearchRun current, SearchRun? previous)
    {
        if (previous is null) return [];

        var known = previous.Results
            .Select(r => (r.Location, r.Name))
            .ToHashSet();

        return current.Results
            .Where(r => !known.Contains((r.Location, r.Name)))
            .Select(r => new NewSolicitor(r.Name, r.Location))
            .OrderBy(n => n.Location).ThenBy(n => n.Name)
            .ToList();
    }
}
