using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;


namespace KC.Apps.Logging;



/// <summary>
///     A logger that writes messages to a text file.
/// </summary>
internal class TextFileLogger : ILogger
{
    [ThreadStatic] private static StreamWriter? t_streamWriter;
    private readonly string _name;





    internal TextFileLogger(
        string name,
        TextFileFormatter formatter,
        IExternalScopeProvider? scopeProvider,
        TextFileLoggerConfiguration config)
        {
            if (formatter is null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            _name = name;
            this.Formatter = formatter;
            this.Config = config;
            this.ScopeProvider = scopeProvider;
        }





    internal TextFileFormatter Formatter { get; set; }
    internal IExternalScopeProvider? ScopeProvider { get; set; }
    internal TextFileLoggerConfiguration Config { get; set; }





    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return default!;
        }





    public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }





    /// <summary>
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="state"></param>
    /// <param name="exception"></param>
    /// <param name="formatter"></param>
    /// <typeparam name="TState"></typeparam>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var logName = GetLogFileName();
            ArgumentNullException.ThrowIfNull(formatter);
            var logEntry = new LogEntry<TState>(logLevel, _name, eventId, state, exception, formatter);
            using (var streamWriter = new StreamWriter(GetLogFileName(), true))
            {
                this.Formatter.Write(in logEntry, this.ScopeProvider, streamWriter);
            }
        }





    private string GetLogFileName()
        {
            var name = "";
            if (this.Config.UseSingleLogFile)
            {
                name = "FileLogger-UnifiedLog.log";
            }
            else
            {
                //create separate Log file for each category 
                name = $"FileLogger-{_name}.log";
            }

            return name;

// TODO: Implement log rotator.
// Log name to incorporate date/time
        }
}