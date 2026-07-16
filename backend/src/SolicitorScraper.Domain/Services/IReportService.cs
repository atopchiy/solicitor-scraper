using SolicitorScraper.Domain.Reports;

namespace SolicitorScraper.Domain.Services;

public interface IReportService
{
    Task<RunReport?> BuildReportAsync(int runId, CancellationToken ct = default);
}
