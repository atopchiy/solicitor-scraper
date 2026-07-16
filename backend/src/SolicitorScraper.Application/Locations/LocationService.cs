using FluentValidation;
using SolicitorScraper.Application.Exceptions;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Application.Locations;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locations;
    private readonly IValidator<AddLocationRequest> _addValidator;

    public LocationService(ILocationRepository locations, IValidator<AddLocationRequest> addValidator)
    {
        _locations = locations;
        _addValidator = addValidator;
    }

    public Task<List<Location>> GetAllAsync(CancellationToken ct = default) =>
        _locations.GetAllAsync(ct);

    public async Task<Location> AddAsync(AddLocationRequest request, CancellationToken ct = default)
    {
        await _addValidator.ValidateAndThrowAsync(request, ct);

        var name = request.Name!.Trim();
        if (await _locations.GetByNameAsync(name, ct) is not null)
            throw new ConflictException($"Location '{name}' already exists.");

        return await _locations.AddAsync(name, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if (!await _locations.DeleteAsync(id, ct))
            throw new NotFoundException($"Location {id} was not found.");
    }

    public async Task<Location> SetEnabledAsync(int id, SetLocationEnabledRequest request, CancellationToken ct = default)
    {
        var location = await _locations.SetEnabledAsync(id, request.IsEnabled, ct);
        return location ?? throw new NotFoundException($"Location {id} was not found.");
    }
}
