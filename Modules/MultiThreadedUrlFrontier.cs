using UrlFrontier.Modules;

namespace UrlFrontier;

public class MultiThreadedUrlFrontier
{
    private readonly IUrlFrontier _frontier;
    private readonly int _numThreads;






    public MultiThreadedUrlFrontier(string startingUrl, int numThreads)
    {
        _frontier = new FrontierToo();
        _numThreads = numThreads;
        EnqueueUrl(startingUrl).Wait();
    }






    public async Task EnqueueUrl(string url)
    {
        var priority = await _frontier.CalculatePriorityAsync(url);
        await _frontier.EnqueueUrlAsync(url);
    }






    public async Task<string> DequeueUrl()
    {
        return await _frontier.DequeueUrl();
    }






    public int GetQueueSize()
    {
        return _frontier.GetQueueSize();
    }






    /// <summary>
    /// Starts the multi-threaded URL processing.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Start(CancellationToken cancellationToken)
    {
        // Create a list of tasks to represent the worker threads
        var tasks = new List<Task>();

        // Instantiate the Scraper outside the worker threads
        var scraper = new Scraper();

        // Start the specified number of worker threads
        for (int i = 0; i < _numThreads; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        string url = await DequeueUrl();
                        // Process the URL using the shared Scraper instance
                        await scraper.ProcessUrlAsync(url);
                    }
                    catch (OperationCanceledException)
                    {
                        // Handle cancellation
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions
                        Console.WriteLine($"Error processing URL: {ex.Message}");
                    }
                }
            }, cancellationToken));
        }

        // Wait for all worker threads to complete
        await Task.WhenAll(tasks);
    } //****
}
// EOF

