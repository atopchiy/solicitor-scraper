using SolicitorScraper.Domain.Entities;

namespace SolicitorScraper.Domain.Repositories;

public interface ILocationRepository
{
    Task<List<Location>> GetAllAsync(CancellationToken ct = default);
    Task<Location?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Location> AddAsync(string name, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<Location?> SetEnabledAsync(int id, bool isEnabled, CancellationToken ct = default);
}
