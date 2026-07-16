using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SolicitorScraper.Domain.Repositories;
using SolicitorScraper.Domain.Scraping;
using SolicitorScraper.Domain.Services;
using SolicitorScraper.Infrastructure.Persistence;
using SolicitorScraper.Infrastructure.Scraping;
using SolicitorScraper.Infrastructure.Services;

namespace SolicitorScraper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<ISearchRunRepository, SearchRunRepository>();
        services.AddScoped<IScrapeService, ScrapeService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddSingleton<IResultsPageParser, SolicitorsComParser>();

        services.AddHttpClient<IResultsPageClient, SolicitorsComClient>(client =>
        {
            client.BaseAddress = new Uri("https://www.solicitors.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0 Safari/537.36");
        });

        return services;
    }
}
