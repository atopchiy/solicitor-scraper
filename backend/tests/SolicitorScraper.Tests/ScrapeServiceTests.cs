using Microsoft.Extensions.Logging.Abstractions;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;
using SolicitorScraper.Domain.Scraping;
using SolicitorScraper.Infrastructure.Services;

namespace SolicitorScraper.Tests;

public class ScrapeServiceTests
{
    [Fact]
    public async Task Scrapes_every_enabled_location_and_saves_a_run()
    {
        var service = CreateService(
            locations: [new Location { Id = 1, Name = "Leeds" }, new Location { Id = 2, Name = "Bristol" }],
            fetch: _ => SamplePage("Firm A"));

        var run = await service.RunAsync();

        Assert.Equal(2, run.Results.Count);
        Assert.Contains(run.Results, r => r.Location == "Leeds");
        Assert.Contains(run.Results, r => r.Location == "Bristol");
        Assert.Empty(run.FailedLocations);
        Assert.NotNull(run.CompletedAt);
    }

    [Fact]
    public async Task Skips_disabled_locations()
    {
        var service = CreateService(
            locations:
            [
                new Location { Id = 1, Name = "Leeds" },
                new Location { Id = 2, Name = "Bradford", IsEnabled = false }
            ],
            fetch: _ => SamplePage("Firm A"));

        var run = await service.RunAsync();

        Assert.All(run.Results, r => Assert.Equal("Leeds", r.Location));
    }

    [Fact]
    public async Task A_failing_location_is_recorded_without_losing_the_others()
    {
        var service = CreateService(
            locations: [new Location { Id = 1, Name = "Leeds" }, new Location { Id = 2, Name = "Bristol" }],
            fetch: location => location == "Bristol"
                ? throw new HttpRequestException("boom")
                : SamplePage("Firm A"));

        var run = await service.RunAsync();

        Assert.Single(run.Results);
        Assert.Equal("Leeds", run.Results[0].Location);
        Assert.Equal(["Bristol"], run.FailedLocations);
    }

    [Fact]
    public async Task Throws_when_no_locations_are_enabled()
    {
        var service = CreateService(locations: [], fetch: _ => "");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RunAsync());
    }

    private static ScrapeService CreateService(List<Location> locations, Func<string, string> fetch) =>
        new(
            new FakeClient(fetch),
            new Infrastructure.Scraping.SolicitorsComParser(),
            new FakeLocationRepository(locations),
            new FakeSearchRunRepository(),
            NullLogger<ScrapeService>.Instance);

    private static string SamplePage(string firmName) =>
        $"""<div class="result-item"><span class="h2">{firmName}</span></div>""";

    private class FakeClient(Func<string, string> fetch) : IResultsPageClient
    {
        public Task<string> GetResultsPageAsync(string location, CancellationToken ct = default) =>
            Task.FromResult(fetch(location));
    }

    private class FakeLocationRepository(List<Location> locations) : ILocationRepository
    {
        public Task<List<Location>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(locations);

        public Task<Location?> GetByNameAsync(string name, CancellationToken ct = default) =>
            Task.FromResult(locations.FirstOrDefault(l => l.Name == name));

        public Task<Location> AddAsync(string name, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<Location?> SetEnabledAsync(int id, bool isEnabled, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private class FakeSearchRunRepository : ISearchRunRepository
    {
        public Task<SearchRun> AddAsync(SearchRun run, CancellationToken ct = default) =>
            Task.FromResult(run);

        public Task<List<SearchRunSummary>> GetSummariesAsync(CancellationToken ct = default) =>
            Task.FromResult(new List<SearchRunSummary>());

        public Task<SearchRun?> GetByIdAsync(int id, CancellationToken ct = default) =>
            Task.FromResult<SearchRun?>(null);

        public Task<HashSet<(string Location, string Name)>> GetKnownSolicitorsAsync(int beforeRunId, CancellationToken ct = default) =>
            Task.FromResult(new HashSet<(string, string)>());
    }
}
