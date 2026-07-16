using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SolicitorScraper.Application.Locations;
using SolicitorScraper.Application.Searches;

namespace SolicitorScraper.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IScrapeService, ScrapeService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddSingleton<IValidator<AddLocationRequest>, AddLocationRequestValidator>();

        return services;
    }
}
