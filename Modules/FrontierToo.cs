

using UrlFrontier.Properties;

namespace UrlFrontier.Modules;

/// <summary>
/// Interface for a URL frontier.
/// </summary>
public interface IUrlFrontier
{
    /// <summary>
    /// Enqueues a URL to the frontier.
    /// </summary>
    /// <param name="url">The URL to enqueue.</param>
    /// <returns>A task that completes when the URL is enqueued.</returns>
    Task EnqueueUrlAsync(string url);






    /// <summary>
    /// Dequeues a URL from the frontier.
    /// </summary>
    /// <returns>A task that completes with the dequeued URL.</returns>
    Task<string> DequeueUrl();






    /// <summary>
    /// Gets the current size of the frontier.
    /// </summary>
    /// <returns>A task that completes with the size of the frontier.</returns>
    int GetQueueSize();






    Task<int> CalculatePriorityAsync(string url);
}

/// <summary>
/// A URL frontier implementation that uses a concurrent queue and a semaphore for thread-safe operations.
/// </summary>
public sealed class FrontierToo : IUrlFrontier, IDisposable
{
    private readonly PriorityQueue<string, int> _queue = new PriorityQueue<string, int>();
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    private readonly HashSet<string> _processedUrls = new HashSet<string>();
    private readonly FrontierOptions _frontierOptions = FrontierOptions.DefaultOptions;
    private bool _disposedValue;






    /// <summary>
    /// Enqueues a URL to the frontier.
    /// </summary>
    /// <param name="url">The URL to enqueue.</param>
    /// <returns>A task that completes when the URL is enqueued.</returns>
    public async Task EnqueueUrlAsync(string url)
    {
        if (_processedUrls.Contains(url)) return; // already processed
        if (_queue.TryPeek(out string? firstUrl, out _) && firstUrl == url) return;

        var priority = await CalculatePriorityAsync(url);

        _queue.Enqueue(url, priority);
        _semaphore.Release();
    }






    public Task<int> CalculatePriorityAsync(string url)
    {
        // we want to have enough granularity in the priority calculation
        // so the priority scale is from 0 to 100 and we start with 50

        var priority = 50;


        // Parse the URL to extract the host, path, query parameters, and fragment identifiers
        Uri uri = new Uri(url);
        string host = uri.Host;
        string path = uri.AbsolutePath;
        string query = uri.Query;
        string fragment = uri.Fragment;

        // Group URLs by host and path patterns
        // Groups with higher counts have higher priorities
        // Comparisons include path, query parameters, and fragment identifiers
        string key = $"{host}{path}{query}{fragment}";

        // Calculate the priority based on the grouping
        int count = GetUrlCount(key);
        priority += count * 10;

        // Check if the URL is a spider trap
        if (IsSpiderTrap(url))
        {
            priority -= 20; // Lowest priority
        }
        else { priority += 10; }

        // Politeness: Avoid repeated requests to the same host within a short period of time
        if (IsRecentlyAccessed(host))
        {
            priority -= 10; // Lowest priority
        }
        else { priority += 10; }

        // Group URLs by host and path patterns 
        //groups with higher counts have higher priorities
        // comparsons include path, query parameters, and fragment identifiers


        // Combine priorities

        return Task.FromResult(priority);
    }






    private int GetUrlCount(string key)
    {
        // Example implementation: Assume a dictionary to store the number of URLs for each key
        Dictionary<string, int> urlCounts = new Dictionary<string, int>();

        // Calculate the count based on the key
        int count = urlCounts.GetValueOrDefault(key, 0);

        return count;
    }






    private bool IsSpiderTrap(string url)
    {
        // An example of a spider trap is if a group of URLs only difference may only be query parameters
        // Return true if the URL is a spider trap, otherwise false
        return false;
    }






    private static readonly Dictionary<string, DateTime> lastAccessTimes = new Dictionary<string, DateTime>();






    private bool IsRecentlyAccessed(string host)
    {
        // Check if the host has been recently accessed within the FrontierOptions.HostFetchInterval timespan
        if (lastAccessTimes.TryGetValue(host, out DateTime lastAccessTime) &&
            (DateTime.Now - lastAccessTime).TotalMilliseconds < _frontierOptions.HostFetchInterval)
        {
            return true;
        }

        // Update the last access time for the host
        lastAccessTimes[host] = DateTime.Now;

        return false;
    }






    private int CalculateHostPriority(string host)
    {
        // Example implementation: Sort the hosts so a host just contacted will have the lowest priority
        // and the host contacted with the greatest timespan will have the highest priority
        // Return the calculated priority
        // You can customize this logic based on your specific requirements

        // Example: Assume a list to store the hosts and their last access times
        List<(string host, DateTime lastAccessTime)> hostList = new List<(string host, DateTime lastAccessTime)>();

        // Calculate the priority based on the last access time
        int priority = hostList.FindIndex(h => h.host == host);

        return priority;
    }






    private int CalculateUrlCountPriority(string host)
    {
        // Example implementation: Group URLs by host and assign higher priority to hosts with more URLs
        // Return the calculated priority
        // You can customize this logic based on your specific requirements

        // Example: Assume a dictionary to store the number of URLs for each host
        Dictionary<string, int> urlCounts = new Dictionary<string, int>();

        // Calculate the priority based on the number of URLs for the host
        int priority = urlCounts.GetValueOrDefault(host, 0);

        return priority;
    }






    /// <summary>
    /// Dequeues a URL from the frontier.
    /// </summary>
    /// <returns>A task that completes with the dequeued URL.</returns>
    public async Task<string> DequeueUrl()
    {
        await _semaphore.WaitAsync();
        _ = _queue.TryDequeue(out string? result, out int priority);
        if (result != null)
        {
            _processedUrls.Add(result); // mark as processed
        }

        return result ?? string.Empty;
    }






    /// <summary>
    /// Gets the current size of the frontier.
    /// </summary>
    /// <returns>The number of URLs currently in the frontier.</returns>
    public int GetQueueSize()
    {
        // The Count property of ConcurrentQueue returns the number of elements currently contained in the queue.
        // This method is thread-safe and does not block other threads.
        return _queue.Count;
    }






    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }






    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~FrontierToo()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }






    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}