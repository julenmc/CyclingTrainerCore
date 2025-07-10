public static class Program
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public static void Main()
    {
        try
        {
            Logger.Info("Hello info");
            Logger.Debug("Hello debug");
            Logger.Warn("Hello warn");
            Logger.Error("Hello error");
            System.Console.ReadKey();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Goodbye cruel world");
        }
    }
}