using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;


namespace KC.Apps.StyledLogger;



public sealed class StyledLoggerFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private const string LoglevelPadding = ": ";
    private StyledLoggerOptions _formatterOptions;





    public StyledLoggerFormatter(IOptionsMonitor<StyledLoggerOptions> options)
        // Case insensitive
        : base(nameof(StyledLoggerFormatter)) =>
        (_optionsReloadToken, _formatterOptions) =
        (options.OnChange(ReloadLoggerOptions), options.CurrentValue);





    private void ReloadLoggerOptions(StyledLoggerOptions options) =>
        _formatterOptions = options;





    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            LogLevel logLevel = logEntry.LogLevel;
            ConsoleColors logLevelColors = GetLogLevelConsoleColors(logLevel);
            string logLevelString = GetLogLevelString(logLevel);
            WritePrefix(textWriter);
            textWriter.Write("  ");
            textWriter.WriteWithColor(logLevelString, logLevelColors.Background, logLevelColors.Foreground);
            textWriter.Write(LoglevelPadding);
            textWriter.WriteWithColor(message, logLevelColors.Background, logLevelColors.Foreground);
            WriteSuffix(textWriter);
        }





    private void WritePrefix(TextWriter textWriter)
        {
            DateTime now = _formatterOptions.UseUtcTimestamp
                ? DateTime.UtcNow
                : DateTime.Now;

            textWriter.Write(
                $"""{_formatterOptions.CustomPrefix} {now.ToString(_formatterOptions.TimestampFormat)} """);
        }





    private void WriteSuffix(TextWriter textWriter) =>
        textWriter.WriteLine($" {_formatterOptions.CustomSuffix}");





    public void Dispose() => _optionsReloadToken?.Dispose();





    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
        {
            // We shouldn't be outputting color codes for Android/Apple mobile platforms,
            // they have no shell (adb shell is not meant for running apps) and all the output gets redirected to some log file.
            bool disableColors = (_formatterOptions.ColorBehavior == LoggerColorBehavior.Disabled) ||
                                 (_formatterOptions.ColorBehavior == LoggerColorBehavior.Default);

            //&& (!EmitAnsiColorCodes || IsAndroidOrAppleMobile));
            if (disableColors)
            {
                return new ConsoleColors(null, null);
            }

            // We must explicitly set the background color if we are setting the foreground color,
            // since just setting one can look bad on the users console.
            return logLevel switch
            {
                LogLevel.Trace => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black)
                , LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black)
                , LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black)
                , LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black)
                , LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed)
                , LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed)
                , _ => new ConsoleColors(null, null)
            };
        }





    private readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
            {
                Foreground = foreground;
                Background = background;
            }





        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }
    }





    private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
        {
            if (_formatterOptions.IncludeScopes && scopeProvider != null)
            {
                bool paddingNeeded = !singleLine;
                scopeProvider.ForEachScope(
                    (scope, state) =>
                    {
                        if (paddingNeeded)
                        {
                            paddingNeeded = false;
                            state.Write("  ");
                            state.Write("=> ");
                        }
                        else
                        {
                            state.Write(" => ");
                        }

                        state.Write(scope);
                    }, textWriter);

                if (!paddingNeeded && !singleLine)
                {
                    textWriter.Write(Environment.NewLine);
                }
            }
        }





    private DateTimeOffset GetCurrentDateTime()
        {
            return _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
        }





    private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "trce", LogLevel.Debug => "dbug", LogLevel.Information => "info"
                , LogLevel.Warning => "warn", LogLevel.Error => "fail", LogLevel.Critical => "crit"
                , _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }
}