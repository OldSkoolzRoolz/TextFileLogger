using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


namespace KC.Apps.StyledLogger;

public static class Extensions
{
    public static ILoggingBuilder AddStyledFormatter(
        this ILoggingBuilder builder,
        Action<StyledLoggerOptions> configure) =>
        builder.AddConsole(options => options.FormatterName = "StyledFormatter")
            .AddConsoleFormatter<StyledLoggerFormatter, StyledLoggerOptions>(configure);
}
