using DemoUnoApp.Local.Models;
using Jamesnet.Foundation;

namespace DemoUnoApp.Local.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IContainer _container;
    private readonly ILayerManager _layerManager;
    private MenuDataItem _currentMenu;

    public List<MenuDataItem> Menus { get; }

    public MenuDataItem CurrentMenu
    {
        get => _currentMenu;
        set => SetProperty(ref _currentMenu, value, OnMenuChanged);
    }

    public MainViewModel(IContainer container, ILayerManager layerManager)
    {
        _container = container;
        _layerManager = layerManager;

        Menus = new List<MenuDataItem>
        {
            new MenuDataItem { MenuName = "Type A", ContentName = "ATypeContent" },
            new MenuDataItem { MenuName = "Type B", ContentName = "BTypeContent" },
        };
    }

    // AnimatedXamlLayer가 연속 요청을 마지막 것만 남기고 처리하므로 여기서는 바로 보여주기만 한다.
    private void OnMenuChanged()
    {
        if (CurrentMenu != null)
        {
            _layerManager.Show("Content", _container.Resolve<IView>(CurrentMenu.ContentName));
        }
    }
}
