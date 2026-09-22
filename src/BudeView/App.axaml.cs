using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BudeView.Core;

namespace BudeView;

public sealed partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var options = StartupOptions.Parse(Environment.GetCommandLineArgs().Skip(1));
            desktop.MainWindow = new MainWindow(options);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
