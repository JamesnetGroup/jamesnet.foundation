using System.Configuration;
using System.Data;
using System.Windows;

namespace DemoOpenSilverApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DemoWpfAppBootstrapper app = new();
            app.Run();
        }
    }

}
