using DemoWinUI3App.Local.ViewModels;
using DemoWinUI3App.UI.Views;
using Jamesnet.Foundation;

namespace DemoWinUI3App;

internal class DemoWinUI3AppBootstrapper : AppBootstrapper
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
