

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;

namespace UrlFrontier;

public interface IDownloader
{
    void StartDownloads();
    Task DownloadFileAsync(string isAny);
    Task ProcessDownloadQueue();
}

public class Downloader : IDownloader
{
    
    
    public Downloader()
    {
        var list = GetDownloadLinks();
        list.ForEach(x=> _downloadBag.Add(x));
        DownloadFileComplete += (sender, args) => Console.WriteLine("Download complete");
    }
    
    
    
    
    public event EventHandler DownloadFileComplete;

    private void OnDownloadFileComplete(EventArgs e)
    {
        EventHandler handler = DownloadFileComplete;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    private static readonly HttpClient _client = new HttpClient(new HttpClientHandler
    {
        MaxConnectionsPerServer = 5,
        AutomaticDecompression = DecompressionMethods.All,
    });


    private SemaphoreSlim _semaphore = new SemaphoreSlim(12);


    /// <summary>
    /// The asynchronous method 'StartDownloadsAsync' initializes the downloading of files from provided links.
    /// </summary>
    /// <remarks>
    /// The function collects download links via 'GetDownloadLinks' method and initiates the download simultaneously for these files. It uses
    /// semaphore to control the concurrent execution of these download tasks. In case of any failure during the download process, the function
    /// catches and logs the exceptions without terminating the execution. On completion of all downloads, the function releases and disposes 
    /// the semaphore and logs a confirmation message indicating the completion of all downloads.
    /// </remarks>
    public void StartDownloads()
    {
        //We don't want our tasks to abort so give them plenty of time.
        _client.Timeout = TimeSpan.FromHours(1);

        // Get list of distinct video links
        var linkList = GetDownloadLinks();

        List<Task> tasks = new List<Task>();
        try
        {
         
        }
        catch (AggregateException a)
        {
            foreach (var e in a.InnerExceptions)
                Console.WriteLine(e.Message);
        }
        Console.WriteLine("Finished with all downloads");
    }

    
    
    
    
    
    
    private ConcurrentBag<string> _downloadBag = new ConcurrentBag<string>();
    
    

    public async Task ProcessDownloadQueue()
    {
        List<Task> tasks = new List<Task>();

        while (_downloadBag.Count > 0 || tasks.Count > 0)
        {
            while (tasks.Count < 5 && _downloadBag.TryTake(out var downloadLink))
            {
                tasks.Add(DownloadFileAsync(downloadLink));
            }

            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);
           var psi =new ProcessStartInfo{
            FileName="wget",
            Arguments=$"--user-agent={ScraperOptions.DefaultOptions.UserAgent}"
           };
        }
    }
    
    
    
    
    
    
    

    ///
    ///
    ///
    ///
    public async Task DownloadFileAsync(string site)
    {
        try
        {
            var path = Path.Combine("/home/apollo/RiderProjects/console/outputFiles",
                Path.GetRandomFileName() + ".mp4");
            await using (var response = await _client.GetStreamAsync(site))
            {
                await response.CopyToAsync(new FileStream(path, FileMode.CreateNew));
                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    ///
    ///
    ///
    ///
    ///
    internal List<string> GetDownloadLinks()
    {
        Dictionary<string, byte> links = new Dictionary<string, byte>();
        var list = File.ReadAllLines("/home/apollo/RiderProjects/console/videos.txt");
        var singles = list.Distinct();
        return singles.ToList();
    }
}

public class DownloadEventArgs : EventArgs
{
    public DownloadEventArgs(string path) => this.FilePath = path;
    public string FilePath { get; }
}