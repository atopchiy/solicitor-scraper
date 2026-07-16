using SolicitorScraper.Infrastructure.Scraping;

namespace SolicitorScraper.Tests;

public class SolicitorsComParserTests
{
    private static readonly string LondonPage =
        File.ReadAllText(Path.Combine("Fixtures", "conveyancing-london.html"));

    private readonly SolicitorsComParser _parser = new();

    [Fact]
    public void Parses_all_listings_from_a_real_results_page()
    {
        var results = _parser.Parse(LondonPage);

        Assert.Equal(75, results.Count);
        Assert.Equal(37, results.Count(r => r.IsFeatured));
        Assert.Equal(38, results.Count(r => !r.IsFeatured));
        Assert.All(results, r => Assert.False(string.IsNullOrWhiteSpace(r.Name)));
    }

    [Fact]
    public void Extracts_full_details_from_a_featured_listing()
    {
        var firm = _parser.Parse(LondonPage).First(r => r.Name == "AlexanderJLO");

        Assert.True(firm.IsFeatured);
        Assert.Equal("(0) 20 7537 7000", firm.Phone);
        Assert.Equal("83 Victoria Street, London, SW1H 0HW", firm.Address);
        Assert.Equal("https://www.solicitors.com/alexanderjlo.html", firm.ProfileUrl);
        Assert.Equal("http://www.london-law.co.uk", firm.Website);
        Assert.Equal(4.5m, firm.Rating);
        Assert.Equal(1049, firm.ReviewCount);
        Assert.Equal("The Law Society Conveyancing Quality Scheme", firm.QualityMarks);
        Assert.Equal("Contact us for all your conveyancing needs", firm.Description);
    }

    [Fact]
    public void Extracts_details_from_a_small_listing()
    {
        var firm = _parser.Parse(LondonPage).First(r => r.Name == "Lee Bolton Monier-Williams");

        Assert.False(firm.IsFeatured);
        Assert.Equal("0207 222 5381", firm.Phone);
        Assert.Equal("1 The Sanctuary, Westminster, London SW1P 3JT", firm.Address);
        Assert.Equal(4.5m, firm.Rating);
        Assert.Equal(21, firm.ReviewCount);
        Assert.Null(firm.Description);
        Assert.Null(firm.Website);
    }

    [Fact]
    public void Listing_without_reviews_has_no_rating()
    {
        const string html = """
            <div class="result-item">
                <span class="h2">Quiet Firm LLP</span>
                <a href="/quiet-firm.html" class="link-map"><address>1 Some Road, Leeds</address></a>
            </div>
            """;

        var firm = Assert.Single(_parser.Parse(html));

        Assert.Equal("Quiet Firm LLP", firm.Name);
        Assert.Null(firm.Rating);
        Assert.Null(firm.ReviewCount);
        Assert.Null(firm.Phone);
        Assert.Null(firm.Website);
        Assert.Equal("1 Some Road, Leeds", firm.Address);
    }

    [Fact]
    public void Listing_without_a_name_is_skipped()
    {
        const string html = """<div class="result-item"><address>Somewhere</address></div>""";

        Assert.Empty(_parser.Parse(html));
    }

    [Fact]
    public void Content_after_a_banner_does_not_leak_into_the_previous_listing()
    {
        const string html = """
            <div class="result-item">
                <span class="h2">Real Firm</span>
            </div>
            <div class="banner-block">
                <a href="https://advertiser.example"><i class="fa fa-globe"></i>Ad</a>
                <a href="tel:000">000</a>
            </div>
            """;

        var firm = Assert.Single(_parser.Parse(html));

        Assert.Null(firm.Website);
        Assert.Null(firm.Phone);
    }

    [Fact]
    public void Decodes_html_entities_in_names()
    {
        const string html = """<div class="result-item"><span class="h2">Smith &amp; Jones</span></div>""";

        Assert.Equal("Smith & Jones", Assert.Single(_parser.Parse(html)).Name);
    }
}
