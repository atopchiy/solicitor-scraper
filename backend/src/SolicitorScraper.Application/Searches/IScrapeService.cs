using SolicitorScraper.Domain.Entities;

namespace SolicitorScraper.Application.Searches;

public interface IScrapeService
{
    Task<SearchRun> RunAsync(CancellationToken ct = default);
}
