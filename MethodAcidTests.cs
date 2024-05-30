
using Microsoft.VisualStudio.TestTools.UnitTesting;
using console;


namespace console;

[TestClass]
public class UrlFrontierTests
{

    [TestMethod]
    public void AddUrl_NullUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var frontier = new UrlFrontier();

        // Act & Assert
        _ = Assert.ThrowsException<ArgumentNullException>(() => frontier.AddUrl(null!));
    }

    [TestMethod]
    public void AddUrl_EmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var frontier = new UrlFrontier();

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => frontier.AddUrl(new Uri(string.Empty)));
    }

    [TestMethod]
    public void AddUrl_InvalidUrl_Returns()
    {
        // Arrange
        var frontier = new UrlFrontier();
        var url = "not a url";
        var url1 = "javascript:void(0);";

        // Act
        frontier.AddUrl(new Uri(url));
        frontier.AddUrl(new Uri(url1));

        // Assert
        Assert.AreEqual(0, frontier.Count);
    }

    [TestMethod]
    public void AddUrl_ExcludedHost_Returns()
    {
        // Arrange
        var frontier = new UrlFrontier();
        frontier.Options.HostExclusions.Append("example.com");
        var url = "http://pineapplesupport.org/page";

        // Act
        frontier.AddUrl(new Uri(url));

        // Assert
        Assert.AreEqual(0, frontier.Count);
    }

    [TestMethod]
    public void AddUrl_FilteredFragment_Returns()
    {
        // Arrange
        var frontier = new UrlFrontier();
        frontier.Options.UrlFragmentFilters.Append("filter");
        var url = "http://example.com/page#filter";

        // Act
        frontier.AddUrl(new Uri(url));

        // Assert
        Assert.AreEqual(0, frontier.Count);
    }

    [TestMethod]
    public void AddUrl_ValidUrl_AddsToFrontier()
    {
        // Arrange
        var frontier = new UrlFrontier();
        var url = "http://example.com/page";

        // Act
        frontier.AddUrl(new Uri(url));

        // Assert
        Assert.AreEqual(1, frontier.Count);
        Assert.AreEqual(url, frontier.UrlSet[0].Url.ToString());
    }

    [TestMethod]
    public void AddUrl_DuplicateUrl_Returns()
    {
        // Arrange
        var frontier = new UrlFrontier();
        var url = "http://example.com/page";
        frontier.AddUrl(new Uri(url));

        // Act
        frontier.AddUrl(new Uri(url));

        // Assert
        Assert.AreEqual(1, frontier.Count);
    }

    [TestMethod]
    public void AddUrl_QueueFull_Returns()
    {
        // Arrange
        var frontier = new UrlFrontier();
        frontier.QueueCapacity = 1;
        frontier.AddUrl(new Uri("http://example.com/page1"));

        // Act
        frontier.AddUrl(new ("http://example.com/page2"));

        // Assert
        Assert.AreEqual(1, frontier.Count);
        Assert.IsFalse(frontier.IsQueueOpen);
    }














}



