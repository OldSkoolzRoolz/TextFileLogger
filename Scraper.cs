using System.Diagnostics;

using HtmlAgilityPack;

using PuppeteerSharp;


namespace console;

public class Scraper : IDisposable
{
    private readonly ScraperOptions _options = new();
    private readonly HtmlWeb _web;

    private readonly UrlFrontier _urlFrontier;
    //= new UrlFrontier(TimeSpan.FromSeconds(5), null, 100);






    public Scraper()
    {
        _web = new HtmlWeb
        {
            UserAgent = _options.UserAgent,
            Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds,
        };

        _urlFrontier = new UrlFrontier
        {
            QueueCapacity = 1000,
            SearchTerm = "fetish",
        };
    }






    public void ScrapeSite(string site)
    {
        _urlFrontier.AddUrl(new Uri(site));
        //  Task.Factory.StartNew(() => _urlFrontier.AdjustFrontierPeriodically(TimeSpan.FromSeconds(30)));



        do
        {
            var nextUrl = _urlFrontier.GetNextUrl();
            Console.WriteLine($"Scraping url; {nextUrl}");
            HtmlDocument doc = new HtmlDocument();
            doc.DisableServerSideCode = true;
            Console.WriteLine($"Frontier contains {_urlFrontier.Count} url's ");

            try
            {
                doc = _web.Load(nextUrl);
            }
            catch (System.Exception)
            {
                continue;
            }

            SearchVideoTags(doc);

            if (_urlFrontier.IsQueueOpen)
            {
                ExtractAnchors(doc, site);
            }
        } while (_urlFrontier.IsEmpty() == false);


        Console.WriteLine("Finished Scraping");
    }






    private void ExtractAnchors(HtmlDocument doc, string address)
    {
        List<string> links;
        if (_options.FollowExternalLinks)
        {
            links = ExtractAllAnchors(doc);
        }
        else
        {
            links = Helpers.GetBaseLinks(doc, address).ToList();
        }

        foreach (string link in links)
        {
            _urlFrontier.AddUrl(new Uri(link));
        }

        System.Console.WriteLine($"Adding {_urlFrontier.Count} links to frontier");

        Console.WriteLine($"Adding {links.Count} to Frontier");
    }






    private void SearchVideoTags(HtmlDocument doc)
    {
        try
        {
            var node = doc.DocumentNode.SelectSingleNode("//*[@id='kt_player']");
            if (node is HtmlNode videonode)
            {
                var vid = videonode.Descendants("video") as HtmlNodeCollection;

                if (vid != null && vid?.Count > 0)
                {
                    var links = Helpers.ExtractSourceUrls(vid).ToList();
                    Console.WriteLine();
                    Console.WriteLine($"Video urls found {links.Count()}");
                    Console.WriteLine();
                    Helpers.AppendStringsToFile(links, "crazyvideos.txt");
                }
            }

            var srcnode = doc.DocumentNode.SelectNodes("//source");
            if (srcnode != null)
            {
                var links = Helpers.ExtractSourceUrls(srcnode).ToList();
                Console.WriteLine();
                Console.WriteLine($"Video urls found {links.Count()}");
                Console.WriteLine();
                Helpers.AppendStringsToFile(links, "crazyvideos.txt");
            }

            var vidnode = doc.DocumentNode.SelectNodes("//video");
            if (vidnode != null)
            {
                var links = Helpers.ExtractSourceUrls(vidnode).ToList();
                Helpers.AppendStringsToFile(links, "crazyvideos.txt");
            }
        }
        catch (ArgumentNullException ane)
        {
            System.Console.WriteLine(ane.Message);
            throw;
        }
    }






    private void SpawnBashProcess(string address)
    {
        var spi = new ProcessStartInfo()
        {
            FileName = "wget",
            WorkingDirectory = "~/outputFiles",
            Arguments = $" -b {address}"
        };
        Process.Start(spi);
    }






    private string ExtractSourceAttribute(HtmlNode node)
    {
        var sourceAttribute = node.GetAttributeValue("src", string.Empty);
        if (!string.IsNullOrEmpty(sourceAttribute) && Uri.IsWellFormedUriString(sourceAttribute, UriKind.Absolute))
        {
            return sourceAttribute;
        }

        return string.Empty;
    }






    public List<string> ExtractAllAnchors(HtmlDocument document)
    {
        var anchors = new List<string>();
        var anchorNodes = document.DocumentNode.SelectNodes("//a[@href]");

        if (anchorNodes != null)
        {
            foreach (var node in anchorNodes)
            {
                var hrefValue = node.GetAttributeValue("href", string.Empty);
                if (!string.IsNullOrEmpty(hrefValue) && Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
                {
                    anchors.Add(hrefValue);
                }
            }
        }

        return anchors;
    }






    public void Dispose()
    {
    }






    internal async Task ReadPageAsync(string site)
    {
        IPage page = await PuppetMaster.InitPuppeteerAsync(site);

        if (!page.IsClosed)
        {
            var source = await page.GetContentAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(source);
            await page.CloseAsync();

            SearchVideoTags(doc);
        }
    }
}
