using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using DemoWpfApp.Local.Models;
using Jamesnet.Foundation;

namespace DemoWpfApp.Local.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IContainer _container;
        private readonly ILayerManager _layerManager;
        private MenuDataItem _currentMenu;
        private bool _isProcessing = false;
        private MenuDataItem _lastMenu = null;

        public List<MenuDataItem> Menus { get; }
        public MenuDataItem CurrentMenu
        {
            get => _currentMenu;
            set => SetProperty(ref _currentMenu, value, OnMenuChanged);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
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

        private async void OnMenuChanged()
        {
            _lastMenu = CurrentMenu;

            if (_isProcessing)
                return;

            IsProcessing = true; // 처리 중 UI 비활성화

            try
            {
                await Task.Delay(50);
                if (_lastMenu != CurrentMenu)
                    return;

                IView view = _container.Resolve<IView>(CurrentMenu.ContentName);
                _layerManager.Show("Content", view);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnMenuChanged: {ex.Message}");
            }
            finally
            {
                IsProcessing = false; // 처리 완료 후 UI 활성화

                if (_lastMenu != CurrentMenu)
                {
                    OnMenuChanged();
                }
            }
        }
    }
}