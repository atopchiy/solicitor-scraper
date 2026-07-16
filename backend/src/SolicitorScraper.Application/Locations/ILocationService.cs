using SolicitorScraper.Domain.Entities;

namespace SolicitorScraper.Application.Locations;

public interface ILocationService
{
    Task<List<Location>> GetAllAsync(CancellationToken ct = default);
    Task<Location> AddAsync(AddLocationRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<Location> SetEnabledAsync(int id, SetLocationEnabledRequest request, CancellationToken ct = default);
}
