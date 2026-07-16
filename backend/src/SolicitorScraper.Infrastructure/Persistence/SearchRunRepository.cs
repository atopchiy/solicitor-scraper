using Microsoft.EntityFrameworkCore;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Infrastructure.Persistence;

public class SearchRunRepository : ISearchRunRepository
{
    private readonly AppDbContext _db;

    public SearchRunRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SearchRun> AddAsync(SearchRun run, CancellationToken ct = default)
    {
        _db.SearchRuns.Add(run);
        await _db.SaveChangesAsync(ct);
        return run;
    }

    public Task<List<SearchRunSummary>> GetSummariesAsync(CancellationToken ct = default) =>
        _db.SearchRuns
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new SearchRunSummary(
                r.Id, r.StartedAt, r.CompletedAt, r.Results.Count, r.FailedLocations))
            .ToListAsync(ct);

    public Task<SearchRun?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.SearchRuns.Include(r => r.Results).FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<HashSet<(string Location, string Name)>> GetKnownSolicitorsAsync(int beforeRunId, CancellationToken ct = default)
    {
        var pairs = await _db.SolicitorResults
            .Where(s => s.SearchRunId < beforeRunId)
            .Select(s => new { s.Location, s.Name })
            .Distinct()
            .ToListAsync(ct);

        return pairs.Select(p => (p.Location, p.Name)).ToHashSet();
    }
}
