using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;
using SolicitorScraper.Infrastructure.Services;

namespace SolicitorScraper.Tests;

public class ReportServiceTests
{
    [Fact]
    public async Task A_firm_is_new_only_if_never_seen_in_any_earlier_run()
    {
        // Run 1 saw firm A, run 2 didn't (the site rotates listings),
        // run 3 shows A again plus a genuinely new firm B.
        var runs = new FakeRepository(
            Run(1, "Firm A"),
            Run(2, "Firm C"),
            Run(3, "Firm A", "Firm B", "Firm C"));

        var report = await new ReportService(runs).BuildReportAsync(3);

        var newFirm = Assert.Single(report!.NewSolicitors);
        Assert.Equal("Firm B", newFirm.Name);
    }

    [Fact]
    public async Task The_first_run_reports_nothing_as_new()
    {
        var runs = new FakeRepository(Run(1, "Firm A", "Firm B"));

        var report = await new ReportService(runs).BuildReportAsync(1);

        Assert.Empty(report!.NewSolicitors);
    }

    [Fact]
    public async Task A_firm_listed_twice_in_the_same_location_is_reported_once()
    {
        // The site sometimes lists the same firm under several branch addresses.
        var runs = new FakeRepository(Run(1, "Firm A"), Run(2, "Firm A", "Firm B", "Firm B"));

        var report = await new ReportService(runs).BuildReportAsync(2);

        var newFirm = Assert.Single(report!.NewSolicitors);
        Assert.Equal("Firm B", newFirm.Name);
    }

    [Fact]
    public async Task Same_name_in_a_different_location_counts_as_new()
    {
        var run2 = Run(2, "Firm A");
        run2.Results[0].Location = "Leeds";

        var runs = new FakeRepository(Run(1, "Firm A"), run2);

        var report = await new ReportService(runs).BuildReportAsync(2);

        var newFirm = Assert.Single(report!.NewSolicitors);
        Assert.Equal("Leeds", newFirm.Location);
    }

    private static SearchRun Run(int id, params string[] firmNames) =>
        new()
        {
            Id = id,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            Results = firmNames
                .Select(name => new SolicitorResult { SearchRunId = id, Name = name, Location = "London" })
                .ToList()
        };

    private class FakeRepository(params SearchRun[] runs) : ISearchRunRepository
    {
        public Task<SearchRun> AddAsync(SearchRun run, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<List<SearchRunSummary>> GetSummariesAsync(CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<SearchRun?> GetByIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(runs.FirstOrDefault(r => r.Id == id));

        public Task<HashSet<(string Location, string Name)>> GetKnownSolicitorsAsync(int beforeRunId, CancellationToken ct = default) =>
            Task.FromResult(runs
                .Where(r => r.Id < beforeRunId)
                .SelectMany(r => r.Results)
                .Select(s => (s.Location, s.Name))
                .ToHashSet());
    }
}
