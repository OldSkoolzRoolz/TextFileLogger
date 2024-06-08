


namespace UrlFrontier.Properties;

public class FrontierOptions
{
    public bool FollowExternalLinks { get; set; } = true;
    public bool EnforceRobotFile { get; set; } = false;
    public Double HostFetchInterval { get; set; } = TimeSpan.FromSeconds(15).TotalMilliseconds;
    public int MaxThreads { get; set; } = 10;
    public int MaxHostAccess { get; set; } = 100;
    public int QueueMaxCapacity { get; set; } = 10000;


    public static FrontierOptions DefaultOptions
    {
        get
        {
            return new FrontierOptions
            {
                FollowExternalLinks = true,
                EnforceRobotFile = false,
                HostFetchInterval = TimeSpan.FromSeconds(15).TotalMilliseconds,
                MaxThreads = 10,
                MaxHostAccess = 100,
                QueueMaxCapacity = 10000,
            };
        }
    }
}