


using System.Runtime.ExceptionServices;

namespace UrlFrontier;

public class Program
{
    public static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.FirstChanceException += FirstChangeException;
        try
        {
            await AppMenu.StartMenu();
        }
        catch (Exception a)
        {
            Console.WriteLine(a);
        }
    }






    private static void FirstChangeException(object? sender, FirstChanceExceptionEventArgs e)
    {
        Console.WriteLine(e.Exception.Message);
    }
}
