using System.Diagnostics.CodeAnalysis;

using HtmlAgilityPack;

namespace UrlFrontier;

internal static class Helpers
{
    private static object _lock = new();






    public static List<string> GetDocumentLinks(HtmlDocument document)
    {
        var absoluteLinkElements = document.DocumentNode
            ?.SelectNodes("//a")
            ?.Where(a => a.Attributes.Contains("href") && a.Attributes["href"].Value.StartsWith("http"))
            .Select(a => a.Attributes["href"].Value)
            .ToList();
        if (absoluteLinkElements is null)
            return new List<string>();
        return absoluteLinkElements;
    }






    /// <summary>
    /// Retrieves the base links from an HTML document based on the given host.
    /// </summary>
    /// <param name="document">The HTML document to extract links from.</param>
    /// <param name="host">The base host to compare the links against.</param>
    /// <returns>An enumerable collection of base links.</returns>
    internal static IEnumerable<string?> GetBaseLinks(HtmlDocument document, string? host)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or empty", nameof(host));

        var linkElements = document?.DocumentNode?
            .SelectNodes("//a")
            ?.Where(a => a.Attributes?.Contains("href") ?? false && !string.IsNullOrEmpty(a.Attributes["href"].Value))
            .Select(a => a.Attributes["href"].Value)
            .Distinct()
            .ToList();

        if (linkElements == null)
            yield break;

        Uri baseUri = new Uri(host);


        var links = ProcessUrls(linkElements, baseUri.Host);
        foreach (var link in links)
        {
            yield return link;
        }
    }






    public static IEnumerable<string?> ProcessUrls(IEnumerable<string> urls, string baseHost)
    {
        // Ensure links are unique
        var uniqueUrls = new HashSet<string?>();

        foreach (var url in urls)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                // Convert relative URLs to absolute
                var absoluteUrl = GetAbsoluteUrl(url, baseHost);
                if (!string.IsNullOrWhiteSpace(absoluteUrl))
                {
                    uniqueUrls.Add(absoluteUrl);
                }
            }
        }

        // Filter by host
        return uniqueUrls.Where(u => u != null && new Uri(u).Host == baseHost);
    }






    private static string? GetAbsoluteUrl(string url, string baseHost)
    {
        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            if (!uri.IsAbsoluteUri)
            {
                // Combine with base URL
                var baseUri = new UriBuilder("http", baseHost).Uri;
                uri = new Uri(baseUri, uri);
            }

            return uri.AbsoluteUri;
        }

        return null;
    }






    public static HtmlDocument CreateDocument(string src)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(src);
        return doc;
    }






    public static void AppendStringsToFile(List<string> strings, string filePath)
    {
        if (strings == null)
            throw new ArgumentNullException(nameof(strings));
        if (filePath == null)
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            foreach (var str in strings)
                if (!string.IsNullOrEmpty(str))
                {
                    lock (_lock)
                    {
                        File.AppendAllText(filePath, str + Environment.NewLine);
                    }
                }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex);
        }
    }






    public static IEnumerable<string> ExtractSourceUrls(HtmlNodeCollection nodes)
    {
        if (nodes != null)
            foreach (var video in nodes)
            {
                // Extract 'src' attribute from <video> element
                var srcAttribute = video.Attributes["src"];
                if (srcAttribute != null) yield return srcAttribute.Value;

                var sourceAttr = video.Attributes["source"];
                if (sourceAttr != null) yield return sourceAttr.Value;

                var dataAttribute = video.Attributes["data-mp4"];
                if (dataAttribute != null) yield return dataAttribute.Value;


                // Specialized handling for JW Player - known to be relative url
                if (video.HasClass("jw-video"))
                {
                    var jwsrcAttribute = video.Attributes["src"];
                    if (jwsrcAttribute != null)
                    {
                        //    var abs = ConvertToAbsoluteUrl(jwsrcAttribute.Value, _startingHost);
                        //   yield return abs;
                    }
                }


                // Search for child <source> elements within <video>
                var src = video.Descendants("source");
                if (src != null)
                {
                    foreach (var ssrc in src)
                    {
                        var srcatr = ssrc.Attributes["src"];
                        if (srcatr != null) yield return srcatr.Value;
                    }
                }





                // Iterate through <source> elements within <video>
                var sourceElements = video.Elements("source");
                if (sourceElements != null)
                    foreach (var source in sourceElements)
                    {
                        // Extract 'href' attribute from <source> element
                        var hrefAttribute = source.Attributes["href"];
                        if (hrefAttribute != null) yield return hrefAttribute.Value;
                    }
            }
    }






    /// <summary>
    /// Method attempts to create an absolute url out of a relative link
    /// </summary>
    /// <param name="url"></param>
    /// <param name="baseUrl"></param>
    /// <returns>Null if it fails to create an absolute url</returns>
    public static string ConvertToAbsoluteUrl([NotNull] string url, [NotNull] string baseUrl)
    {
        try
        {
            var baseUri = new Uri(baseUrl);
            var absoluteUri = new Uri(baseUri, url);

            return absoluteUri.AbsoluteUri;
        }
        catch (UriFormatException)
        {
            return string.Empty;
        }
    }






    public static string ExtractSourceUrl(HtmlNode video)
    {
        // Extract 'src' attribute from <video> element
        var srcAttribute = video.Attributes["src"];
        if (srcAttribute != null) return srcAttribute.Value;

        var sourceAttr = video.Attributes["source"];
        if (sourceAttr != null) return sourceAttr.Value;

        var dataAttribute = video.Attributes["data-mp4"];
        if (dataAttribute != null) return dataAttribute.Value;


        // Specialized handling for JW Player - known to be relative url
        if (video.HasClass("jw-video"))
        {
            var jwsrcAttribute = video.Attributes["src"];
            if (jwsrcAttribute != null)
            {
                //    var abs = ConvertToAbsoluteUrl(jwsrcAttribute.Value, _startingHost);
                return jwsrcAttribute.Value;
            }
        }



        // Search for child <source> elements within <video>
        // Iterate through <source> elements within <video>
        var sourceElements = video.Elements("source");
        if (sourceElements != null)
            foreach (var source in sourceElements)
            {
                // Extract 'href' attribute from <source> element
                var hrefAttribute = source.Attributes["href"];
                if (hrefAttribute != null) return hrefAttribute.Value;
            }





        return string.Empty;
    }






    public static void AppendStringsToFile(List<char> videoUrls, string crazyvideosTxt)
    {
        throw new NotImplementedException();
    }
}