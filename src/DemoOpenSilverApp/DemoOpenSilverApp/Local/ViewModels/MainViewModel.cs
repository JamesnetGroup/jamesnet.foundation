using System;
using System.Collections.Generic;
using DemoOpenSilverApp.Local.Models;
using Jamesnet.Foundation;

namespace DemoOpenSilverApp.Local.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IContainer _container;
    private readonly ILayerManager _layerManager;
    private MenuDataItem _currentMenu;

    public List<MenuDataItem> Menus { get; }
    public MenuDataItem CurrentMenu
    {
        get => _currentMenu;
        set=> SetProperty(ref  _currentMenu, value, OnMenuChanged);    
    }

    public MainViewModel(
        IContainer container,
        ILayerManager layerManager)
    {
        _container = container;
        _layerManager = layerManager;

        Menus = new List<MenuDataItem> 
        { 
            new MenuDataItem { MenuName = "Type A", ContentName = "ATypeContent" },
            new MenuDataItem { MenuName = "Type B", ContentName = "BTypeContent" },
        };
    }

    private void OnMenuChanged()
    {
        try
        {
            IView view = _container.Resolve<IView>(CurrentMenu.ContentName);
            _layerManager.Show("Content", view);
        }
        catch (Exception ex)
        {

        }
    }
}