
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Console;


namespace KC.Apps.StyledLogger
{

    public class StyledLoggerOptions : ConsoleFormatterOptions
    {
        public StyledLoggerOptions() { }


        public string? CustomPrefix { get; set; }

        public string? CustomSuffix { get; set; }
        
        public LoggerColorBehavior ColorBehavior { get; set; }
        
        public bool SingleLine { get; set; }
        
        internal void Configure(IConfiguration configuration) => configuration.Bind(this);
    }
}