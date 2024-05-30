



using PuppeteerSharp;


namespace console;


public class PuppetMaster
{

    public static string ChromiumPath = "/home/apollo/Apps/Browser/chrome-linux64/chrome";
    public static string HeadlessChromiumPath = "/home/apollo/Apps/Browser/chrome-headless-shell-linux64/chrome";

    public static async Task GetBrowser()
    {
        var downloadPath = "/home/apollo/Apps/Browser/Chromium";
        var bo = new BrowserFetcherOptions { Path = downloadPath };

        var browserFetcher = new BrowserFetcher(bo);

        var browser = await browserFetcher.DownloadAsync();
        ChromiumPath = browser.GetExecutablePath();


        browserFetcher = null;
    }



    public static async Task<IPage> InitPuppeteerAsync(string url)
    {
        IPage? page = default;
        try
        {

            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                ExecutablePath = ChromiumPath,
            });


            Console.WriteLine("Puppeteer launched");

            page = await browser.NewPageAsync();

            await page.GoToAsync(url);
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


