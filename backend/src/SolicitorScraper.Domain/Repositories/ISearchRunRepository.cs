using SolicitorScraper.Domain.Entities;

namespace SolicitorScraper.Domain.Repositories;

public record SearchRunSummary(
    int Id,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int ResultCount,
    List<string> FailedLocations);

public interface ISearchRunRepository
{
    Task<SearchRun> AddAsync(SearchRun run, CancellationToken ct = default);
    Task<List<SearchRunSummary>> GetSummariesAsync(CancellationToken ct = default);
    Task<SearchRun?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<HashSet<(string Location, string Name)>> GetKnownSolicitorsAsync(int beforeRunId, CancellationToken ct = default);
}
