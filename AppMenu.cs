
using PuppeteerSharp;

namespace console;

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
            Console.WriteLine();
            Console.WriteLine("X. Exit");
            Console.Write("Enter your choice: ");
            string? input = Console.ReadLine().ToUpperInvariant();

            switch (input)
            {
                case "A":
                    Console.WriteLine("Starting scraping");
                    System.Console.WriteLine("Enter url to scrape: ");
                    string? siteurl =
                        "https://www.drtuber.com/categories/fetish";

                    // = System.Console.ReadLine();

                    Scraper scrap = new();
                    scrap.ScrapeSite(siteurl);
                    scrap.Dispose();
                    break;
                case "B":
                    Console.WriteLine("Viewing queue");
                    break;

                case "C":
                    Console.WriteLine("Loading puppet master");
                    System.Console.WriteLine("Enter url to scrape: ");
                    //var site1 = Console.ReadLine();
                    var site1 = "https://crazyporn.xxx/videos/83202/big-boobed-mom-trainee-fuck-by-long-cock/";
                    if (!string.IsNullOrEmpty(site1))
                    {
                        using var scrape = new Scraper();

                        await scrape.ReadPageAsync(site1);

                        System.Console.WriteLine("Scrape complete.. Press any key to Continue...");
                        Console.ReadKey();
                        scrape.Dispose();
                    }

                    break;
                case "D":
                    await PuppetMaster.GetBrowser();

                    System.Console.WriteLine("Download complete.. Press any key to Continue...");
                    Console.ReadKey();
                    break;
                case "X":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option, please try again");
                    System.Console.WriteLine("Download complete.. Press any key to Continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}
