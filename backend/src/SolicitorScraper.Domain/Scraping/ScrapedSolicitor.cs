namespace SolicitorScraper.Domain.Scraping;

public record ScrapedSolicitor
{
    public required string Name { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Website { get; init; }
    public string? ProfileUrl { get; init; }
    public string? Description { get; init; }
    public string? QualityMarks { get; init; }
    public decimal? Rating { get; init; }
    public int? ReviewCount { get; init; }
    public bool IsFeatured { get; init; }
}
