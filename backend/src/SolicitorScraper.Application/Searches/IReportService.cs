namespace SolicitorScraper.Application.Searches;

public interface IReportService
{
    Task<RunReport?> BuildReportAsync(int runId, CancellationToken ct = default);
}
