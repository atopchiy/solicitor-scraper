using Microsoft.EntityFrameworkCore;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Infrastructure.Persistence;

public class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _db;

    public LocationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Location>> GetAllAsync(CancellationToken ct = default) =>
        _db.Locations.OrderBy(l => l.Name).ToListAsync(ct);

    public Task<Location?> GetByNameAsync(string name, CancellationToken ct = default) =>
        _db.Locations.FirstOrDefaultAsync(l => l.Name.ToLower() == name.ToLower(), ct);

    public async Task<Location> AddAsync(string name, CancellationToken ct = default)
    {
        var location = new Location { Name = name };
        _db.Locations.Add(location);
        await _db.SaveChangesAsync(ct);
        return location;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var location = await _db.Locations.FindAsync([id], ct);
        if (location is null) return false;

        _db.Locations.Remove(location);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<Location?> SetEnabledAsync(int id, bool isEnabled, CancellationToken ct = default)
    {
        var location = await _db.Locations.FindAsync([id], ct);
        if (location is null) return null;

        location.IsEnabled = isEnabled;
        await _db.SaveChangesAsync(ct);
        return location;
    }
}
