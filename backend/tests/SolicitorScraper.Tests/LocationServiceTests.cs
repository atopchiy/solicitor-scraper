using FluentValidation;
using SolicitorScraper.Application.Exceptions;
using SolicitorScraper.Application.Locations;
using SolicitorScraper.Domain.Entities;
using SolicitorScraper.Domain.Repositories;

namespace SolicitorScraper.Tests;

public class LocationServiceTests
{
    [Fact]
    public async Task Adds_a_valid_location_with_trimmed_name()
    {
        var repository = new FakeRepository();
        var service = CreateService(repository);

        var location = await service.AddAsync(new AddLocationRequest("  York "));

        Assert.Equal("York", location.Name);
        Assert.Single(repository.Locations);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("York123")]
    public async Task Rejects_invalid_names(string? name)
    {
        var service = CreateService(new FakeRepository());

        await Assert.ThrowsAsync<ValidationException>(() => service.AddAsync(new AddLocationRequest(name)));
    }

    [Fact]
    public async Task Rejects_a_duplicate_location()
    {
        var repository = new FakeRepository(new Location { Id = 1, Name = "York" });
        var service = CreateService(repository);

        await Assert.ThrowsAsync<ConflictException>(() => service.AddAsync(new AddLocationRequest("york")));
    }

    [Fact]
    public async Task Deleting_an_unknown_location_reports_not_found()
    {
        var service = CreateService(new FakeRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(42));
    }

    [Fact]
    public async Task Toggling_an_unknown_location_reports_not_found()
    {
        var service = CreateService(new FakeRepository());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SetEnabledAsync(42, new SetLocationEnabledRequest(false)));
    }

    private static LocationService CreateService(FakeRepository repository) =>
        new(repository, new AddLocationRequestValidator());

    private class FakeRepository(params Location[] seed) : ILocationRepository
    {
        public List<Location> Locations { get; } = [.. seed];

        public Task<List<Location>> GetAllAsync(CancellationToken ct = default) =>
            Task.FromResult(Locations);

        public Task<Location?> GetByNameAsync(string name, CancellationToken ct = default) =>
            Task.FromResult(Locations.FirstOrDefault(l =>
                string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase)));

        public Task<Location> AddAsync(string name, CancellationToken ct = default)
        {
            var location = new Location { Id = Locations.Count + 1, Name = name };
            Locations.Add(location);
            return Task.FromResult(location);
        }

        public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
            Task.FromResult(Locations.RemoveAll(l => l.Id == id) > 0);

        public Task<Location?> SetEnabledAsync(int id, bool isEnabled, CancellationToken ct = default)
        {
            var location = Locations.FirstOrDefault(l => l.Id == id);
            if (location is not null) location.IsEnabled = isEnabled;
            return Task.FromResult(location);
        }
    }
}
