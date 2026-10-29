using System.Windows;

namespace DemoOpenSilverApp
{
    public sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();

            // Enter construction logic here...
            var app = new DemoOpenSilverAppBootstrapper();
            app.Run();

            var mainPage = new MainPage();
            Window.Current.Content = mainPage;
        }
    }
}
