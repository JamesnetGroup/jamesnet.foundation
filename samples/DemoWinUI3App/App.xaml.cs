using Microsoft.UI.Xaml;

namespace DemoWinUI3App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        new DemoWinUI3AppBootstrapper().Run();

        var window = new Window { Title = "DemoWinUI3App", Content = new MainPage() };
        window.Activate();
    }
}
