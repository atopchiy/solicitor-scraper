namespace SolicitorScraper.Domain.Scraping;

public interface IResultsPageParser
{
    IReadOnlyList<ScrapedSolicitor> Parse(string html);
}
