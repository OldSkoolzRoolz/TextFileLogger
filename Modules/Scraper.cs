using System.Diagnostics;
using System.Net.Http.Headers;
using System.Xml;

using UrlFrontier.Properties;

using HtmlAgilityPack;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.IE;

using PuppeteerSharp;


namespace UrlFrontier.Modules;

public class Scraper : IDisposable
{
    private readonly ScraperOptions _options = new();
    private readonly FrontierOptions _frontierOptions = FrontierOptions.DefaultOptions;
    private readonly HtmlWeb _web;

    private readonly FrontierToo _urlFrontier;






    public Scraper()
    {
        _web = new HtmlWeb
        {
            UserAgent = _options.UserAgent, Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds,
        };

        _urlFrontier = new FrontierToo();
    }






    /// <summary>
    /// Processes a given URL by scraping it for video tags and extracting anchors.
    /// </summary>
    /// <param name="url">The URL to process.</param>
    /// <returns>A task that completes when the URL has been processed.</returns>
    public async Task ProcessUrlAsync(string url)
    {
        // Print the URL being processed
        Console.WriteLine($"Scraping url: {url}");

        // Print the number of URLs in the frontier
        Console.WriteLine($"Frontier contains {_urlFrontier.GetQueueSize()} url's ");

        HtmlDocument doc = new HtmlDocument();

        try
        {
            // Load the HTML document from the URL
            doc = await _web.LoadFromWebAsync(url!);
        }
        catch (Exception)
        {
            // If there is an exception while loading the document, ignore it
        }

        // Search for video tags in the document
        SearchVideoTags(doc);

        // Extract anchors from the document
        ExtractAnchors(doc, url);
    }






    /// <summary>
    /// Extracts anchors from the given HTML document and adds them to the frontier.
    /// </summary>
    /// <param name="doc">The HTML document to extract anchors from.</param>
    /// <param name="address">The base URL of the document.</param>
    private async void ExtractAnchors(HtmlDocument doc, string address)
    {
        // Extract all anchors from the document
        List<string?>? links;
        if (_options.FollowExternalLinks)
        {
            links = ExtractAllAnchors(doc);
        }
        else
        {
            // Get only the base links of the document
            links = Helpers.GetBaseLinks(doc, address)?.ToList();
        }

        // If there are any links to add to the frontier
        if (links != null)
        {
            // Enqueue each link in the frontier
            foreach (string? link in links)
            {
                if (link != null)
                {
                    await _urlFrontier.EnqueueUrlAsync(link);
                }
            }

            // Get the current size of the frontier and print the number of links added
            var count = _urlFrontier.GetQueueSize();
            Console.WriteLine($"Adding {count} links to frontier");

            Console.WriteLine($"Adding {links.Count} to Frontier");
        }
    }






    private void SearchVideoTags(HtmlDocument document)
    {
        TrySearchVideoTags(document, "//*[@id='kt_player']//video");
        TrySearchVideoTags(document, "//source");
        TrySearchVideoTags(document, "//video");
    }






    private void TrySearchVideoTags(HtmlDocument document, string xPath)
    {
        var videoNodes = document.DocumentNode.SelectNodes(xPath);
        if (videoNodes == null)
        {
            return;
        }

        var videoUrls = videoNodes.SelectMany(node => Helpers.ExtractSourceUrl(node)).ToList();


        if (videoUrls.Any())
        {
            Console.WriteLine($"Video urls found: {videoUrls.Count}");
            Helpers.AppendStringsToFile(videoUrls, "crazyvideos.txt");
        }
    }






    private void SpawnBashProcess(string address)
    {
        var spi = new ProcessStartInfo()
        {
            FileName = "wget", WorkingDirectory = "~/outputFiles", Arguments = $" -b {address}"
        };
        Process.Start(spi);
    }






    public List<string?> ExtractAllAnchors(HtmlDocument document)
    {
        var anchors = new List<string?>();
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






    public async Task ScrapeFrontierwithPuppeteer(string site)
    {
        int puppetRecoveries = 0;

        IPage page;

        string source = string.Empty;

        try
        {
            page = await PuppetMaster.InitPuppeteerAsync();
        }
        catch (Exception)
        {
            puppetRecoveries++;
            //Attempt to restart puppeteer to recover from any errors
            page = await PuppetMaster.InitPuppeteerAsync();
        }




        do
        {
            // Get Url from Frontier
            var nextUrl = await _urlFrontier.DequeueUrl();

            try
            {
                // Navigate to new page
                await page.GoToAsync(nextUrl, WaitUntilNavigation.Networkidle0);

                // Get Page Source
                source = await page.GetContentAsync();
            }
            catch (Exception ex) when (ex.Message.Contains("PuppeteerSharp"))
            {
                if (puppetRecoveries++ < 3)
                {
                    //Attempt to restart puppeteer to recover from any errors
                    page = await PuppetMaster.InitPuppeteerAsync();
                }
            }



            // Load source into htmldoc
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(source);

            //searcg for video sources
            SearchVideoTags(doc);

            // Get All Anchor links
            ExtractAnchors(doc, nextUrl);
        } while (_urlFrontier.GetQueueSize() > 0);
    }






    public async Task ScrapeUsingSeleniumAsync(string address)
    {
        IWebDriver driver = new ChromeDriver();

        driver.Navigate().GoToUrl(address);

        var vids = driver.FindElements(By.TagName("video"));
        var src = driver.FindElements(By.TagName("source"));

        Debugger.Break();
    }






    private readonly HttpClient _client = new HttpClient();






    public async Task StartScrapingWithHttpClientAsync(string address)
    {
        if (string.IsNullOrEmpty(address))
        {
            throw new ArgumentException("Address cannot be null or empty.", nameof(address));
        }

        var req = new HttpRequestMessage(HttpMethod.Get, new Uri(address));
        req.Headers.UserAgent.ParseAdd(ScraperOptions.DefaultOptions.UserAgent);
        req.Headers.Referrer = new Uri("https://www.crazyporn.xxx");

        using var response = await _client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var doc = new HtmlDocument();
        doc.Load(reader);

        SearchVideoTags(doc);
        ExtractAnchors(doc, address);
    }
}

