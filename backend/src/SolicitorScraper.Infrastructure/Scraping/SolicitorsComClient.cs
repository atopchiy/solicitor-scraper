using SolicitorScraper.Domain.Scraping;

namespace SolicitorScraper.Infrastructure.Scraping;

public class SolicitorsComClient : IResultsPageClient
{
    private readonly HttpClient _http;

    public SolicitorsComClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<string> GetResultsPageAsync(string location, CancellationToken ct = default)
    {
        // The site's search form just redirects to a static page per location,
        // e.g. "conveyancing+london.html", so we can skip the form submission.
        var slug = location.Trim().ToLowerInvariant().Replace(' ', '+');
        return await _http.GetStringAsync($"conveyancing+{slug}.html", ct);
    }
}
