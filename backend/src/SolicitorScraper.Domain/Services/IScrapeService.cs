using SolicitorScraper.Domain.Entities;

namespace SolicitorScraper.Domain.Services;

public interface IScrapeService
{
    Task<SearchRun> RunAsync(CancellationToken ct = default);
}
