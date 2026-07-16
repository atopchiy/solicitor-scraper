namespace SolicitorScraper.Domain.Scraping;

public interface IResultsPageClient
{
    Task<string> GetResultsPageAsync(string location, CancellationToken ct = default);
}
