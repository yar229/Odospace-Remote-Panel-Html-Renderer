namespace YaR.Odospace.RemotePanel.HtmlRendererClient;

public static class Logger
{
    public static void Info(string msg)
    {
        Console.WriteLine($"{DateTime.Now}\t{msg}");
    }

    public static void Error(string msg, Exception exception)
    {
        Console.WriteLine($"{DateTime.Now}\t{msg}\t{exception.Message}");
    }
}