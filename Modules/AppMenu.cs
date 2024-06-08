
using PuppeteerSharp;

using UrlFrontier.Modules;

namespace UrlFrontier;

public class AppMenu
{
    public static async Task StartMenu()
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("Welcome to the app");
            Console.WriteLine("A. Scrape a site for videos");
            Console.WriteLine("B. Download captured video links");
            Console.WriteLine("C. Open Site using puppeteer");
            Console.WriteLine("D. Download browser");
            Console.WriteLine("E. Scrape using HttpClient");
            Console.WriteLine();
            Console.WriteLine("X. Exit");
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine()?.ToUpperInvariant();

            switch (input)
            {
                case "A":
                    Console.WriteLine("Starting scraping");
                    Console.WriteLine("Enter url to scrape: ");
                    string? siteurl =
                        "https://crazyporn.xxx/videos/83202/big-boobed-mom-trainee-fuck-by-long-cock/";

                    // = System.Console.ReadLine();

                    MultiThreadedUrlFrontier multiThreadedFrontier = new MultiThreadedUrlFrontier(siteurl, 5);

                    CancellationTokenSource cts = new CancellationTokenSource();
                    await multiThreadedFrontier.Start(cts.Token);



                    break;
                case "B":
                    Console.WriteLine("Viewing queue");
                    break;

                case "C":
                    Console.WriteLine("Loading puppet master");
                    Console.WriteLine("Enter url to scrape: ");
                    //var site1 = Console.ReadLine();
                    var site1 = "https://crazyporn.xxx/videos/83202/big-boobed-mom-trainee-fuck-by-long-cock/";
                    if (!string.IsNullOrEmpty(site1))
                    {
                        using var scrape = new Scraper();

                        await scrape.ScrapeFrontierwithPuppeteer(site1);

                        Console.WriteLine("Scrape complete.. Press any key to Continue...");
                        Console.ReadKey();
                    }

                    break;
                case "D":
                    await PuppetMaster.GetBrowser();

                    Console.WriteLine("Download complete.. Press any key to Continue...");
                    Console.ReadKey();
                    break;
                case "E":

                    Console.WriteLine("Starting Http scraping");

                    var site2 = "https://crazyporn.xxx/videos/83202/big-boobed-mom-trainee-fuck-by-long-cock/";
                    var scraper = new Scraper();
                    await scraper.ScrapeUsingSeleniumAsync(site2);

                    scraper.Dispose();
                    break;
                case "X":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again");
                    Console.WriteLine("Download complete.. Press any key to Continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}
