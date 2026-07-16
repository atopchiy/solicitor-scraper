using System.Net;
using System.Text;

namespace SolicitorScraper.Infrastructure.Scraping;

internal static class HtmlText
{
    public static string? Between(string source, string start, string end, int from = 0)
    {
        var i = source.IndexOf(start, from, StringComparison.Ordinal);
        if (i < 0) return null;

        i += start.Length;
        var j = source.IndexOf(end, i, StringComparison.Ordinal);
        return j < 0 ? null : source[i..j];
    }

    public static string StripTags(string source)
    {
        var sb = new StringBuilder(source.Length);
        var insideTag = false;

        foreach (var c in source)
        {
            if (c == '<') insideTag = true;
            else if (c == '>') insideTag = false;
            else if (!insideTag) sb.Append(c);
        }

        return sb.ToString();
    }

    public static string Clean(string? source)
    {
        if (string.IsNullOrEmpty(source)) return string.Empty;

        var text = WebUtility.HtmlDecode(source);
        var sb = new StringBuilder(text.Length);
        var lastWasSpace = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace) sb.Append(' ');
                lastWasSpace = true;
            }
            else
            {
                sb.Append(c);
                lastWasSpace = false;
            }
        }

        return sb.ToString().Trim(' ', ',');
    }

    public static int Count(string source, string value)
    {
        var count = 0;
        var i = source.IndexOf(value, StringComparison.Ordinal);
        while (i >= 0)
        {
            count++;
            i = source.IndexOf(value, i + value.Length, StringComparison.Ordinal);
        }
        return count;
    }
}
