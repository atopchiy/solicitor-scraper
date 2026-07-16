using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Application.Searches;

public interface ISearchService
{
    Task<SearchRunSummary> RunAsync(CancellationToken ct = default);
    Task<List<SearchRunSummary>> GetRunsAsync(CancellationToken ct = default);
    Task<SearchRun> GetRunAsync(int id, CancellationToken ct = default);
    Task<RunReport> GetReportAsync(int id, CancellationToken ct = default);
}
