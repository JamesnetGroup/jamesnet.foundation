using Jamesnet.Foundation;

namespace Jamesnet.Platform.Uno;

public class UnoLayer : ContentControl, ILayer
{
    public static readonly DependencyProperty LayerNameProperty =
        DependencyProperty.Register(nameof(LayerName), typeof(string), typeof(UnoLayer), new PropertyMetadata(null, OnLayerNameChanged));

    public bool IsRegistered { get; set; }

    public string LayerName
    {
        get => (string)GetValue(LayerNameProperty);
        set => SetValue(LayerNameProperty, value);
    }

    public object UIContent
    {
        get => (object)Content;
        set => Content = value;
    }

    public UnoLayer()
    {
        DefaultStyleKey = typeof(UnoLayer);
        LayerManager.InitializeLayer(this);
    }

    private static void OnLayerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UnoLayer layer)
        {
            layer.IsRegistered = false;
            LayerManager.RegisterToLayerManager(layer);
        }
    }
}

