using Microsoft.UI.Xaml;

namespace DemoUnoApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new DemoUnoAppBootstrapper().Run();

        var window = new Window { Title = "DemoUnoApp", Content = new MainPage() };
        window.Activate();
    }
}
