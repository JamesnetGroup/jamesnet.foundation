using Jamesnet.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Jamesnet.Uwp;

public class UwpLayer : ContentControl, ILayer
{
    public static readonly DependencyProperty LayerNameProperty =
        DependencyProperty.Register(nameof(LayerName), typeof(string), typeof(UwpLayer), new PropertyMetadata(null, OnLayerNameChanged));

    public bool IsRegistered { get; set; }

    public string LayerName
    {
        get => (string)GetValue(LayerNameProperty);
        set => SetValue(LayerNameProperty, value);
    }

    public UwpLayer()
    {
        DefaultStyleKey = typeof(UwpLayer);
        LayerManager.InitializeLayer(this);
    }

    private static void OnLayerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UwpLayer layer)
        {
            layer.IsRegistered = false;
            LayerManager.RegisterToLayerManager(layer);
        }
    }
}
