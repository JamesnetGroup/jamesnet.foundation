using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoOpenSilverApp.Local.ViewModels;
using DemoOpenSilverApp.UI.Views;
using Jamesnet.Foundation;

namespace DemoOpenSilverApp
{
    internal class DemoOpenSilverAppBootstrapper : AppBootstrapper
    {
        protected override void RegisterDependencies(IContainer container)
        {
            container.RegisterSingleton<IView, ATypeContent>(nameof(ATypeContent));
            container.RegisterSingleton<IView, BTypeContent>(nameof(BTypeContent));
            container.RegisterSingleton<IView, MainContent>(nameof(MainContent));
        }

        protected override void RegisterViewModels(IViewModelMapper viewModelMapper)
        {
            viewModelMapper.Register<MainContent, MainViewModel>();
        }

        protected override void SettingsLayer(ILayerManager layer, IContainer container)
        {
            layer.Mapping("Main", container.Resolve<IView>(nameof(MainContent)));
        }
    }
}
