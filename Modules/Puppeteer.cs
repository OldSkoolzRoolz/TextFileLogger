



using PuppeteerSharp;


namespace UrlFrontier;


public class PuppetMaster
{

    public static string ChromiumPath = "/home/apollo/Apps/Browser/chrome-linux64/chrome";
    public static string HeadlessChromiumPath = "/home/apollo/Apps/Browser/chrome-headless-shell-linux64/chrome";





    ///
    public static async Task GetBrowser()
    {
        var downloadPath = "/home/apollo/Apps/Browser/Chromium";
        var bo = new BrowserFetcherOptions { Path = downloadPath };

        var browserFetcher = new BrowserFetcher(bo);

        var browser = await browserFetcher.DownloadAsync();
        ChromiumPath = browser.GetExecutablePath();


        browserFetcher = null;
    }






    /// <summary>
    /// Initializes a new instance of the PuppetMaster class.
    ///
    public static async Task<IPage> InitPuppeteerAsync()
    {
        IPage? page = default;
        try
        {

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = ChromiumPath,
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" },
                Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds,
                IgnoreHTTPSErrors = true,

            });


            Console.WriteLine("Puppeteer launched");

            page = browser.NewPageAsync().Result;
            //page = browser.G



            Console.WriteLine();
            Console.WriteLine("Page loaded");


        }
        catch (Exception ex)
        {
            Console.WriteLine("Error initializing puppeteer");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }


#pragma warning disable CS8603 // Possible null reference return.

        return page;
#pragma warning restore CS8603 // Possible null reference return.

    }



}


