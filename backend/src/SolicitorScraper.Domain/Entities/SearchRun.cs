namespace SolicitorScraper.Domain.Entities;

public class SearchRun
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<SolicitorResult> Results { get; set; } = new();
    public List<string> FailedLocations { get; set; } = new();
}
