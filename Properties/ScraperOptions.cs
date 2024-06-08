
public class ScraperOptions
{
    public int MaxDepth { get; set; } = 5;

    public string? UserAgent { get; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";


    public int Timeout { get; } = 180000;
    public int MaxPages { get; } = 100;
    public int MaxConcurrentPages { get; } = 4;

    public string OutputPath
    {
        get
        {
            var path2 = AppContext.BaseDirectory;
            var dirs = Path.Combine(path2, "output");

            return dirs;
        }
    }

    public string ConnectionString { get; } =
        "server=127.0.0.1;user=plato;database=spyderlib;port=3306;password=password;sslmode=none;pooling=true;maxpoolsize=50;connection timeout=30;connection lifetime=60;default command timeout=30;connectionidletimeout=20;AllowPublicKeyRetrieval=True;";

    public int MaxPagesPerHost { get; } = 50;
    public string CachePath { get; internal set; } = "./cache";

    public bool LimitResources { get; set; } = false;
    public bool FollowExternalLinks { get; set; } = false;


    public static ScraperOptions DefaultOptions { get; } = new();

    public string[] HostExclusions { get; set; }

    public string[] UrlFragmentFilters { get; set; }

    public ScraperOptions()
    {
        HostExclusions = new[]
        {
            "twitter.com", "microsoft.com", "google.com", "bing.com", "pineapplesupport.org", "instagram.com",
            "facebook.com"
        };

        UrlFragmentFilters = new[] { "mailto", "ftp:" };




    }
}
