namespace Jamesnet.Foundation;

public abstract class AppBootstrapper
{
    protected readonly IContainer Container;
    protected readonly ILayerManager Layer;
    protected readonly IViewModelMapper ViewModelMapper;

    protected AppBootstrapper()
    {
        Container = new Container();
        Layer = new LayerManager();
        ViewModelMapper = new ViewModelMapper();

        ContainerProvider.SetContainer(Container);
        ConfigureContainer();
    }

    public void Run()
    {
        RegisterViewModels(ViewModelMapper);
        RegisterDependencies(Container);
        SettingsLayer(Layer, Container);
    }

    protected virtual void ConfigureContainer()
    {
        Container.RegisterInstance<IContainer>(Container);
        Container.RegisterInstance<ILayerManager>(Layer);
        Container.RegisterInstance<IViewModelMapper>(ViewModelMapper);
        Container.RegisterSingleton<IViewModelInitializer, DefaultViewModelInitializer>();
    }

    protected abstract void RegisterViewModels(IViewModelMapper viewModelMapper);
    protected abstract void RegisterDependencies(IContainer container);
    protected abstract void SettingsLayer(ILayerManager layer, IContainer container);
}