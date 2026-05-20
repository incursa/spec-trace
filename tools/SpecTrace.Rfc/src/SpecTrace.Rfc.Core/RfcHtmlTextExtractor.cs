using System.Net;
using System.Text.RegularExpressions;

namespace SpecTrace.Rfc.Core;

public static class RfcHtmlTextExtractor
{
    private static readonly Regex Title = new(@"<title>\s*(?<value>.*?)\s*</title>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex RfcNumberMeta = new(@"<meta\s+content=""(?<value>\d+)""\s+name=""rfc\.number""\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex SavedFromUrl = new(@"saved\s+from\s+url=\(\d+\)(?<value>https?://[^ >]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex RemoveBlocks = new(@"<(?<tag>head|script|style|svg|nav|aside)[^>]*>.*?</\k<tag>>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex Heading = new(@"<h[1-6][^>]*>(?<value>.*?)</h[1-6]>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex Paragraph = new(@"<p[^>]*>(?<value>.*?)</p>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex ListItem = new(@"<li[^>]*>(?<value>.*?)</li>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex Preformatted = new(@"<pre[^>]*>(?<value>.*?)</pre>", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private static readonly Regex AnyTag = new(@"<[^>]+>", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex BlankLines = new(@"\n{3,}", RegexOptions.Compiled);
    private static readonly Regex HorizontalWhitespace = new(@"[ \t]{2,}", RegexOptions.Compiled);

    public static bool LooksLikeHtml(string source, string content)
    {
        return source.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
               source.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
               content.TrimStart().StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase) ||
               content.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase);
    }

    public static string? TryGetTitle(string html)
    {
        var match = Title.Match(html);
        if (!match.Success)
        {
            return null;
        }

        return CleanInlineText(match.Groups["value"].Value);
    }

    public static string? TryGetRfcNumber(string html)
    {
        var meta = RfcNumberMeta.Match(html);
        if (meta.Success)
        {
            return meta.Groups["value"].Value;
        }

        var title = TryGetTitle(html);
        var titleMatch = title is null ? Match.Empty : Regex.Match(title, @"RFC\s+(?<value>\d+)", RegexOptions.IgnoreCase);
        return titleMatch.Success ? titleMatch.Groups["value"].Value : null;
    }

    public static string? TryGetSavedFromUrl(string html)
    {
        var match = SavedFromUrl.Match(html);
        return match.Success ? match.Groups["value"].Value : null;
    }

    public static string ToPlainText(string html)
    {
        var body = RemoveBlocks.Replace(html, "\n\n");
        body = Preformatted.Replace(body, match => "\n\n" + CleanPreText(match.Groups["value"].Value) + "\n\n");
        body = Heading.Replace(body, match => "\n\n" + CleanInlineText(match.Groups["value"].Value) + "\n\n");
        body = Paragraph.Replace(body, match => "\n\n" + CleanInlineText(match.Groups["value"].Value) + "\n\n");
        body = ListItem.Replace(body, match => "\n- " + CleanInlineText(match.Groups["value"].Value) + "\n");
        body = Regex.Replace(body, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        body = Regex.Replace(body, @"</(?:tr|div|section|article)>", "\n\n", RegexOptions.IgnoreCase);
        body = Regex.Replace(body, @"</(?:td|th)>", " ", RegexOptions.IgnoreCase);
        body = AnyTag.Replace(body, " ");
        body = WebUtility.HtmlDecode(body);
        body = body.Replace('\u00a0', ' ');
        body = body.Replace("\u00b6", string.Empty, StringComparison.Ordinal);

        var lines = body.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Split('\n')
            .Select(NormalizeLine)
            .ToArray();

        return BlankLines.Replace(string.Join('\n', lines), "\n\n").Trim() + "\n";
    }

    private static string NormalizeLine(string line)
    {
        var trimmedEnd = line.TrimEnd();
        return trimmedEnd.StartsWith(" ", StringComparison.Ordinal) || trimmedEnd.StartsWith("\t", StringComparison.Ordinal)
            ? trimmedEnd
            : HorizontalWhitespace.Replace(trimmedEnd, " ").TrimEnd();
    }

    private static string CleanInlineText(string html)
    {
        var text = AnyTag.Replace(html, " ");
        text = WebUtility.HtmlDecode(text);
        text = text.Replace('\u00a0', ' ');
        text = text.Replace("\u00b6", string.Empty, StringComparison.Ordinal);
        return HorizontalWhitespace.Replace(text, " ").Trim();
    }

    private static string CleanPreText(string html)
    {
        var text = AnyTag.Replace(html, string.Empty);
        return WebUtility.HtmlDecode(text)
            .Replace('\u00a0', ' ')
            .Replace("\u00b6", string.Empty, StringComparison.Ordinal)
            .Trim('\n', '\r');
    }
}
