


using System.Diagnostics;

namespace UrlFrontier;


public static class Log
{

    public static void Error(Exception ex, string message)
    {
        
        Trace.TraceError(ex.ToString());
        Trace.TraceError(message);
        Console.WriteLine(message);
        Console.WriteLine(ex.Message);
    }


    public static void Information(string message)
    {

        Trace.TraceInformation(message);
        Console.WriteLine(message);

    }


}