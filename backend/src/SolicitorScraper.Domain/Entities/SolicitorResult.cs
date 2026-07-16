namespace SolicitorScraper.Domain.Entities;

public class SolicitorResult
{
    public int Id { get; set; }
    public int SearchRunId { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Website { get; set; }
    public string? ProfileUrl { get; set; }
    public string? Description { get; set; }
    public string? QualityMarks { get; set; }
    public decimal? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public bool IsFeatured { get; set; }
}
