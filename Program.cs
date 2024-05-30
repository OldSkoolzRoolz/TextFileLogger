



namespace console;

public class Program
{
   
   
   
    public static async Task Main(string[] args)
    {
        try
        {

              await  AppMenu.StartMenu();


        }
        catch (Exception a)
        {
            Console.WriteLine(a);
        }
    }
}
