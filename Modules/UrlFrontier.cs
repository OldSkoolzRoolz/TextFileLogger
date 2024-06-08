using System.Diagnostics;
using System.Text;

namespace UrlFrontier;

public class UrlFrontierz
{
    ///<summary>
    /// TODO: implement weighting mechanisms
    ///  Prioritizing Weight Criteria
    ///
    ///  1. Urls containing the search term carry the highest weight and are the highest priority       --- Context Targeting
    ///  2. The most unique urls are weighted higher than others                                        --- Diversity
    ///  3. Urls with the word 'video' in the url are weighted higher than others.                      --- Targeting Type
    ///  4. Urls are grouped by host and path and the smaller groups are weighted higher than others.   --- Diversity
    ///  5. Hosts with more recent accesses are weighted lower than others.                             --- Politeness <summary>
    /// </summary>
    private readonly object _lockObject = new object();

    // A mandatory time delay between any requests
    private TimeSpan _delayBetweenRequests;
    public readonly ScraperOptions Options = new();

    public TimeSpan DelayBetweenAllRequests
    {
        get => _delayBetweenRequests;
        set { _delayBetweenRequests = value; }
    }

    // A minimum time span between calls to the same host.
    private TimeSpan? _hardHostDelayTimeSpan;

    public TimeSpan? HardHostDelayTimeSpan
    {
        get { return _hardHostDelayTimeSpan; }
        set { _hardHostDelayTimeSpan = value; }
    }

    // A hard threshold for hosts configurable only at startup. If set a host will not be accessed more than this many times.
    private int _hardHostRequestThreshold;

    public int HardHostRequestThreshold
    {
        get => _hardHostRequestThreshold;
        set => _hardHostRequestThreshold = value;
    }


    // a dictionary of the last time a url was accessed keyed on the url
    public Dictionary<Uri, DateTime> LastAccessTimes { get; set; }

    public Dictionary<string, int> HostAccessCounts { get; set; }

    // a set of urls that have been checked to prevent duplicate processing.
    private readonly HashSet<Uri> _urlCheckSet;

    // Master sorted set containing the queue of weighted urls.
    // The SortedSet is constantly being adjusted as the UrlFrontier grows. //TODO: Implement service to monitor and adjust list.
    private List<PriorityUrl> _urlSet;
    public List<PriorityUrl> UrlSet => _urlSet;

    // User defined search term for context targeted results. Carries the highest priority weight.
    public string SearchTerm { get; set; }
    public int Count => _urlSet.Count;
    public bool IsQueueOpen { get; set; }
    public long QueueCapacity { get; set; }






    public UrlFrontierz()
    {
        _urlSet = new List<PriorityUrl>();
        _urlCheckSet = new HashSet<Uri>();
        LastAccessTimes = new Dictionary<Uri, DateTime>();
        // this._delayBetweenRequests = delayBetweenRequests;
        // this._hardHostLimit = hardHostLimit;
        //  this._hardHostRequestThreshold = hardHostRequestThreshold;
        this.HostAccessCounts = new();
        IsQueueOpen = true;
        SearchTerm = string.Empty;
        QueueCapacity = 1000;
    }






    /// <summary>
    /// Add a URL to the frontier. The url will be assigned a priority weight that is calculated
    /// based on a set of rules layed out above. The Frontier will be sorted by this priority and will
    /// be monitored and adjusted as the Frontier builds. Some rules are based on counts of certain types or characteristics
    /// contained in the url, so as the frontier grows it must be adjusted to adhere to these rules..
    /// </summary>
    /// <param name="url">The URL to be added to the frontier.</param>
    /// <exception cref="ArgumentNullException">Thrown if url is null.</exception>
    /// <exception cref="ArgumentException">Thrown if url is null or whitespace.</exception>
    public void AddUrl(Uri url)
    {
        if (url == null)
        {
            throw new ArgumentNullException(nameof(url), "The url cannot be null or whitespace.");
        }

        if (!IsValidUrl(url)) return;

        // Check if the host of the url is excluded from the frontier
        if (Options?.HostExclusions?.Any(h => url.Host.Contains(h, StringComparison.OrdinalIgnoreCase)) ?? false) return;

        // Check if the url fragment contains any of the excluded strings
        if (Options?.UrlFragmentFilters?.Any(f => url.ToString().Contains(f, StringComparison.OrdinalIgnoreCase)) ?? false) return;

        // If the queue is at capacity, return without adding the url
        if (Count >= QueueCapacity)
        {
            IsQueueOpen = false;
            // Create a backup of current queue for troubleshooting purposes.
            File.WriteAllLines("UrlFrontierQueue.bak", _urlSet.Select(u => u.Url.ToString()).ToArray(),
                Encoding.Default);
            return;
        }

        // Calculate the priority of the url
        var priority = CalculatePriority(url);

        // Add the url to the frontier if it is not already there
        lock (_lockObject)
        {
            if (_urlCheckSet.Contains(url))
            {
                return;
            }

            var priorityUrl = new PriorityUrl(url, priority);

                _urlSet.Add(priorityUrl);

                _urlCheckSet.Add(url);
                LastAccessTimes[url] = DateTime.UtcNow;
        }

    }







   





    /// <summary>
    /// Calculates a priority weight for a given url based on a set of rules. The rules are as follows:
    /// 1. Urls containing the search term carry the highest weight and are the highest priority       --- Context Targeting
    /// 2. The most unique urls are weighted higher than others                                        --- Diversity
    /// 3. Urls with the word 'video' in the url are weighted higher than others.                      --- Targeting Type
    /// 4. Urls are grouped by host and path and the smaller groups are weighted higher than others.   --- Diversity
    /// 5. Hosts with more recent accesses are weighted lower than others.                             --- Politeness
    /// 6. Avoid hitting the hard host limit for any given host
    /// </summary>
    /// <param name="url"></param>
    /// <returns>A priority weight based on the characteristics of the url</returns>
    /// <exception cref="UriFormatException">a priority of 0 will be returned on exception.</exception>
    /// <remarks>
    /// The rules are as follows:
    /// 1. If the url contains the search term, assign a priority of 30. This is the highest possible
    ///    priority and will be the first url to be processed.
    /// 2. If the url is not already in the set of urls, assign a priority of 5. This is to encourage
    ///    diversity in the urls that are processed.
    /// 3. If the url contains the word 'video', assign a priority of 10.
    /// 4. Group urls by host and path and assign a priority that is inversely proportional to the size of the
    ///    group. This is to encourage diversity in the urls that are processed.
    /// 5. If the host of the url has been accessed recently, assign a lower priority. This is to prevent a
    ///    host from being accessed too frequently.
    /// 6. Avoid hitting the hard host limit for any given host. If the number of accesses to a host is
    ///    greater than the hard host limit, assign a lower priority.
    /// </remarks>
    public int CalculatePriority(Uri url)
    {
        int priority = 0;

        try
        {
            // 1. Urls containing the search term carry the highest weight and are the highest priority       --- Context Targeting
            var prioritySearchTerm = url.GetLeftPart(UriPartial.Query)
                .Contains(SearchTerm, StringComparison.CurrentCultureIgnoreCase)
                ? 30
                : 0;
            priority += prioritySearchTerm;

            var urlHash = url.GetHashCode();
            // 2. The most unique urls are weighted higher than others                                        --- Diversity
            if (!_urlSet.Any(u => u.Url.GetHashCode() == urlHash))
                priority += 5;


            // 3. Urls with the word 'video' in the url are weighted higher than others.                      --- Targeting Type
            var priorityVideo = url.GetLeftPart(UriPartial.Query).Contains("video") ? 10 : 0;
            priority += priorityVideo;









            // 4. Urls are grouped by host and path and the smaller groups are weighted higher than others.   --- Diversity
            var groupBy = url.GetLeftPart(UriPartial.Path);

            // 5. Hosts with more recent accesses are weighted lower than others.                             --- Politeness
            if (LastAccessTimes.TryGetValue(url, out var hostAccessTime))
                priority -= (int)Math.Floor((DateTime.UtcNow - hostAccessTime).TotalSeconds / 30.0);
            else
                LastAccessTimes.Add(url, DateTime.UtcNow);

            // 6. Avoid hitting the hard host count limit for any given host
            if (HostAccessCounts.TryGetValue(url.Host, out var hostAccessCount))
            {
                if (hostAccessCount >= HardHostRequestThreshold)
                    priority -= 10;
                HostAccessCounts[url.Host] = hostAccessCount + 10;
            }
            else
                HostAccessCounts.Add(url.Host, 1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An exception occurred while calculating priority for url '{url}'.");
            Console.Error.WriteLine(ex);
        }

        return priority;
    }







    public void AdjustFrontierPeriodically(TimeSpan adjustmentInterval)
    {
        while (true)
        {
            Thread.Sleep(adjustmentInterval);

            lock (_lockObject)
            {
                try
                {
                    foreach (var priorityUrl in _urlSet)
                    {
                        var url = priorityUrl.Url;
                        var priority = priorityUrl.Priority;

                        // 5. Hosts with more recent accesses are weighted lower than others.                             --- Politeness
                        var host = url.Host;
                        if (LastAccessTimes.TryGetValue(url, out DateTime lastAccess))
                        {
                            var timeSinceLastAccess = DateTime.UtcNow - lastAccess;
                            if (timeSinceLastAccess < HardHostDelayTimeSpan)
                            {
                                priority -= (int)timeSinceLastAccess.TotalMinutes;
                            }
                        }

                        // 6. Avoid hitting the hard host limit for any given host
                        var hostAccessCount = LastAccessTimes.Count(kvp => kvp.Key.Host == host);
                        if (hostAccessCount >= HardHostRequestThreshold)
                        {
                            priority -= 10;
                        }

                        // Update the priority if it's changed
                        if (priority != priorityUrl.Priority)
                        {
                            _urlSet.Remove(priorityUrl);
                            priorityUrl.Priority = priority;
                            _urlSet.Add(priorityUrl);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Exception in AdjustFrontierPeriodically: {ex.Message}");
                    Console.Error.WriteLine(ex.StackTrace);
                }
            }
        }
    }






    // Get the next URL from the frontier
    public Uri? GetNextUrl()
    {
        _urlSet.Sort(new UrlPriorityComparer());

      

        lock (_lockObject)
        {
            if (_urlSet.Count == 0)
            {
                return null;
            }


            PriorityUrl? nextUrl = default;
            try
            {
                if (!_urlSet.TryTake(out var result))
                {
                    Console.WriteLine($"Attempted to take item from empty frontier {_urlSet.Count}");
                    return null;
                }

                nextUrl = result;

                var currentTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception in GetNextUrl: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }

            return nextUrl?.Url ?? null;
        }
    }






    /// <summary>
    /// Updates the access counts for a given host and potentially removes urls from the frontier
    /// if the hard host request threshold has been exceeded.
    /// </summary>
    /// <param name="nextUrl">The url that was just retrieved from the frontier.</param>
    /// <param name="hardHostRequestThreshold">The maximum number of times that a host can be accessed before urls from that host are removed from the frontier.</param>
    private void UpdateAccessCounts(PriorityUrl nextUrl, int hardHostRequestThreshold)
    {
        // Lock the object to prevent concurrent updates to the access counts and the frontier
        lock (_lockObject)
        {
            // Get the current access count for the host of the just-retrieved url
            var hostAccessCount = HostAccessCounts.TryGetValue(nextUrl.Url.Host, out var count) ? count : 0;

            // If the access count for the host has exceeded the hard host request threshold
            if (hostAccessCount >= hardHostRequestThreshold)
            {
              
              
              
              // Find all urls in the frontier that have the same host as the just-retrieved url
              // and remove them from the frontier
              var urlsToRemove = _urlSet.Where(u => u.Url.Host == nextUrl.Url.Host).ToList();
              foreach (var url in urlsToRemove)
              {
                  // Remove the url from the frontier
                  _urlSet.Remove(url);
                  
                  // Remove the url from the set of urls that should be checked (in case it was previously checked)
                  _urlCheckSet.Remove(url.Url);
                  
                  // Remove the url from the dictionary of last access times
                  LastAccessTimes.Remove(url.Url);
              }
            }
        }
    }






    private void UpdateAccessTime(PriorityUrl nextUrl, DateTime currentTime)
    {
        lock (_lockObject)
        {
            if (LastAccessTimes.TryGetValue(nextUrl.Url, out var lastAccessTime))
            {
                LastAccessTimes[nextUrl.Url] = currentTime;
            }
            else
            {
                LastAccessTimes.Add(nextUrl.Url, currentTime);
            }
        }
    }






    // Check if the frontier is empty
    public bool IsEmpty()
    {
        lock (_lockObject)
        {
            return _urlSet.Count == 0;
        }
    }






    public static bool IsValidUrl(Uri uri)
    {
        try
        {
            if (!uri.IsWellFormedOriginalString()) return false;


            var left = uri.GetLeftPart(UriPartial.Scheme);
            if (!uri.OriginalString.StartsWith("http")) return false;



            return true;
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
} // End of Frontier class














public class PriorityUrl : IEquatable<PriorityUrl>
{
    public PriorityUrl(Uri url, int priority)
    {
        Url = url;
        Priority = priority;
    }






    public Uri Url { get; }
    public int Priority { get; set; }






    public bool Equals(PriorityUrl? other)
    {
        return other != null && Url.Equals(other.Url);
    }
}

public class UrlPriorityComparer : IComparer<PriorityUrl>
{
    public int Compare(PriorityUrl? x, PriorityUrl? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1;
        }

        if (y is null)
        {
            return 1;
        }

        return y.Priority.CompareTo(x.Priority);
    }
}

public static class SortedSetExtension
{
    public static bool TryTake(this List<PriorityUrl> set, out PriorityUrl? result)
    {
        if (set.Count > 0)
        {
            result = set.First();
            set.Remove(result);
            return true;
        }
        else
        {
            result = default;
            return false;
        }
    }
}


