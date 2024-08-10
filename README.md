# Styled Console Logger

This package is a quick drop in for any project to provide ASCII text log file for any program.

### Key Features:

    - Easy extension method to add to project
    - Configure log entry format to distinguish entries
    - Can create one log file per application or one for each class/category.
    - Colorized entries
    - Supports structured logging


---

### Implementation/Configuration
```  
builder.Logging.AddTextFileLogger(
                    configuration =>
                    {
                        // just a little flare to make the entries stand out
                        // can be text chars as below or string variable
                        configuration.EntryPrefix = "~<[";
                        configuration.EntrySuffix = "]>~";

                        // Timespan options
                        configuration.TimestampFormat = "HH:mm:ss";
                        configuration.UseUtcTime = false;

                        // Scope info
                        configuration.IncludeScopes = false;

                        // Send all log entries to one file
                        // or create individual logs for each category
                        configuration.UseSingleLogFile = true;
                        // TBD
                        configuration.LogRotationPolicy = LogRotationPolicy.Hourly;
                    });  
```


---
##  Usage

```

 var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();
                var logger2 = loggerFactory.CreateLogger("AnotherCategory");
                var logger3 = loggerFactory.CreateLogger("SeperateLogFile");
                logger2.LogInformation("This is for logger2 testing. ssame LogFilename");
                logger3.LogInformation("Seperate log file testing");
                logger.LogTrace("Trace Test Message");
                logger.LogCritical("Critical Error Test message.");
                logger.LogError("Error Test Message");
                logger.LogDebug("Debug Test message");
                logger.LogWarning("Warning Test Message");
                logger.LogInformation("Hello World!");

                // Logger message attr - defined as an extension method
                logger.ApplicationInfo("Basic Compiled messages");

                //Logger message - passing in ILogger as param
                LoggerMessages.ApplicationWarning(logger, "There was an error in the module");

```

