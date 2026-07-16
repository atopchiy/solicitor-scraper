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

    public Task<SearchRun?> GetPreviousRunAsync(int beforeId, CancellationToken ct = default) =>
        _db.SearchRuns.Include(r => r.Results)
            .Where(r => r.Id < beforeId)
            .OrderByDescending(r => r.Id)
            .FirstOrDefaultAsync(ct);
}
