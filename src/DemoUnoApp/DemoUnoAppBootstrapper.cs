using DemoUnoApp.Local.ViewModels;
using DemoUnoApp.UI.Views;
using Jamesnet.Foundation;

namespace DemoUnoApp;

internal class DemoUnoAppBootstrapper : AppBootstrapper
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
