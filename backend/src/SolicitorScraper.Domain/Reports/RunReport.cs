namespace SolicitorScraper.Domain.Reports;

public record RunReport(
    int RunId,
    DateTime StartedAt,
    int TotalSolicitors,
    int LocationsCovered,
    decimal? AverageRating,
    int WithQualityMarks,
    int FeaturedCount,
    IReadOnlyList<LocationSummary> Locations,
    IReadOnlyList<RankedFirm> TopRated,
    IReadOnlyList<NewSolicitor> NewSolicitors,
    IReadOnlyList<string> FailedLocations);

public record LocationSummary(
    string Location,
    int Count,
    decimal? AverageRating,
    int FeaturedCount,
    int WithQualityMarks);

public record RankedFirm(
    string Name,
    int LocationCount,
    decimal Rating,
    int ReviewCount,
    double Score);

public record NewSolicitor(string Name, string Location);
