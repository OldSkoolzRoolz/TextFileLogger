using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


namespace KC.Apps.StyledLogger;

internal class Program
{
  public static void Main(string[] args)
    {
      HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

      builder.Logging.AddConsole()
        .AddConsoleFormatter<StyledLoggerFormatter, StyledLoggerOptions>();
      
        using IHost host = builder.Build();


        ILoggerFactory loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
      ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

      using (logger.BeginScope("Logging Scope"))
      {
        logger.LogTrace("Trace Test Message");
        logger.LogCritical("Critical Error Test message.");
        logger.LogError("Error Test Message");
        logger.LogDebug("Debug Test message");
        logger.LogWarning("Warning Test Message");
        logger.LogInformation("Hello World!");
      }
    }
}