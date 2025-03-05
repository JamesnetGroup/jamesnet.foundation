using Jamesnet.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Jamesnet.Platform.WinUI3
{
    public class WinUI3Layer : ContentControl, ILayer
    {
        public static readonly DependencyProperty LayerNameProperty =
            DependencyProperty.Register(nameof(LayerName), typeof(string), typeof(WinUI3Layer), new PropertyMetadata(null, OnLayerNameChanged));

        public bool IsRegistered { get; set; }

        public string LayerName
        {
            get => (string)GetValue(LayerNameProperty);
            set => SetValue(LayerNameProperty, value);
        }

        public object UIContent
        {
            get => (object)Content;
            set => Content = (UIElement)value;
        }

        public WinUI3Layer()
        {
            DefaultStyleKey = typeof(WinUI3Layer);
            LayerManager.InitializeLayer(this);
        }

        private static void OnLayerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WinUI3Layer layer)
            {
                layer.IsRegistered = false;
                LayerManager.RegisterToLayerManager(layer);
            }
        }
    }
}

