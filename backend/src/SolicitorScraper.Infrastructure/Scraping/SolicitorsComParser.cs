using SolicitorScraper.Domain.Scraping;

namespace SolicitorScraper.Infrastructure.Scraping;

public class SolicitorsComParser : IResultsPageParser
{
    private const string BaseUrl = "https://www.solicitors.com";
    private const string ItemMarker = "<div class=\"result-item";

    // Ad banners and the page footer share the tail of the last result block,
    // so each block gets cut at the first of these.
    private static readonly string[] BlockTerminators =
    {
        "<div class=\"banner-block\"",
        "<footer",
        "</main>"
    };

    public IReadOnlyList<ScrapedSolicitor> Parse(string html)
    {
        var results = new List<ScrapedSolicitor>();
        var index = html.IndexOf(ItemMarker, StringComparison.Ordinal);

        while (index >= 0)
        {
            var next = html.IndexOf(ItemMarker, index + ItemMarker.Length, StringComparison.Ordinal);
            var block = next >= 0 ? html[index..next] : html[index..];

            var solicitor = ParseBlock(Truncate(block));
            if (solicitor is not null)
                results.Add(solicitor);

            index = next;
        }

        return results;
    }

    private static string Truncate(string block)
    {
        foreach (var terminator in BlockTerminators)
        {
            var i = block.IndexOf(terminator, StringComparison.Ordinal);
            if (i >= 0) block = block[..i];
        }
        return block;
    }

    private static ScrapedSolicitor? ParseBlock(string block)
    {
        var name = HtmlText.Clean(HtmlText.Between(block, "<span class=\"h2\">", "<"));
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var isFeatured = !block.StartsWith(ItemMarker + " item-small", StringComparison.Ordinal);

        return new ScrapedSolicitor
        {
            Name = name,
            IsFeatured = isFeatured,
            Phone = ParsePhone(block),
            Address = ParseAddress(block),
            ProfileUrl = ParseProfileUrl(block),
            Website = ParseWebsite(block),
            Description = isFeatured ? ParseDescription(block) : null,
            QualityMarks = ParseQualityMarks(block),
            Rating = ParseRating(block),
            ReviewCount = ParseReviewCount(block)
        };
    }

    private static string? ParsePhone(string block)
    {
        var telIndex = block.IndexOf("href=\"tel:", StringComparison.Ordinal);
        if (telIndex < 0) return null;

        var text = HtmlText.Between(block, ">", "</a>", telIndex);
        return NullIfEmpty(HtmlText.Clean(text));
    }

    private static string? ParseAddress(string block)
    {
        var inner = HtmlText.Between(block, "<address>", "</address>");
        return inner is null ? null : NullIfEmpty(HtmlText.Clean(HtmlText.StripTags(inner)));
    }

    private static string? ParseProfileUrl(string block)
    {
        var mapIndex = block.IndexOf("class=\"link-map\"", StringComparison.Ordinal);
        if (mapIndex < 0) return null;

        var href = HrefBefore(block, mapIndex);
        if (string.IsNullOrEmpty(href) || href == "#") return null;

        return href.StartsWith('/') ? BaseUrl + href : href;
    }

    private static string? ParseWebsite(string block)
    {
        var globeIndex = block.IndexOf("fa-globe", StringComparison.Ordinal);
        return globeIndex < 0 ? null : NullIfEmpty(HrefBefore(block, globeIndex));
    }

    private static string? ParseDescription(string block)
    {
        // The firm's blurb is the first paragraph after the address link.
        var from = block.IndexOf("</address>", StringComparison.Ordinal);
        var text = HtmlText.Between(block, "<p>", "</p>", from < 0 ? 0 : from);
        return text is null ? null : NullIfEmpty(HtmlText.Clean(HtmlText.StripTags(text)));
    }

    private static string? ParseQualityMarks(string block)
    {
        var title = HtmlText.Between(block, "class=\"greentick\" title=\"", "\"");
        if (title is null) return null;

        // Title reads "<Firm> hold the following quality marks: X Y Z."
        var colon = title.IndexOf("quality marks:", StringComparison.OrdinalIgnoreCase);
        if (colon >= 0)
            title = title[(colon + "quality marks:".Length)..];

        return NullIfEmpty(HtmlText.Clean(title).TrimEnd('.'));
    }

    private static decimal? ParseRating(string block)
    {
        var full = HtmlText.Count(block, "star-full");
        var half = HtmlText.Count(block, "star-half");

        if (full == 0 && half == 0) return null;
        return full + half * 0.5m;
    }

    private static int? ParseReviewCount(string block)
    {
        var revIndex = block.IndexOf("rev-results", StringComparison.Ordinal);
        if (revIndex < 0) return null;

        var span = HtmlText.Between(block, "rev-results", "</span>");
        if (span is null) return null;

        var raw = HtmlText.Between(span, "(", ")");
        return int.TryParse(raw, out var count) ? count : null;
    }

    private static string? HrefBefore(string block, int index)
    {
        var hrefIndex = block.LastIndexOf("href=\"", index, StringComparison.Ordinal);
        if (hrefIndex < 0) return null;

        return HtmlText.Between(block, "href=\"", "\"", hrefIndex);
    }

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
