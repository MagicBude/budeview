using Avalonia;

namespace BudeView;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Any(static arg => string.Equals(arg, "--self-test", StringComparison.OrdinalIgnoreCase)))
        {
            return SelfTests.Run();
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
