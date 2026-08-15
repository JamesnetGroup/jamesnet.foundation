#if WINUI3 || UNO
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else // WPF, OPENSILVER
using System.Windows;
using System.Windows.Controls;
#endif

namespace Jamesnet.Foundation;

/// <summary>
/// 플랫폼 중립 레이어 컨트롤. WPF, OpenSilver, WinUI3, Uno에서 동일한 이름과
/// 동일한 동작으로 사용한다. 플랫폼마다 별도의 어셈블리로 컴파일되지만
/// 소스는 이 파일 한 벌뿐이다.
/// </summary>
public class XamlLayer :
#if OPENSILVER
    UserControl
#else
    ContentControl
#endif
    , ILayer
{
    public static readonly DependencyProperty LayerNameProperty =
        DependencyProperty.Register(
            nameof(LayerName),
            typeof(string),
            typeof(XamlLayer),
            new PropertyMetadata(null, OnLayerNameChanged));

    public bool IsRegistered { get; set; }

    public string LayerName
    {
        get => (string)GetValue(LayerNameProperty);
        set => SetValue(LayerNameProperty, value);
    }

    public object UIContent
    {
        get => Content;
#if OPENSILVER || WINUI3 || UWP
        set => Content = (UIElement)value;
#else
        set => Content = value;
#endif
    }

    public XamlLayer()
    {
        DefaultStyleKey = typeof(XamlLayer);

        if (IsInDesignMode(this))
        {
            return;
        }

        LayerManager.InitializeLayer(this);
    }

    private static void OnLayerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not XamlLayer layer || IsInDesignMode(layer))
        {
            return;
        }

        layer.IsRegistered = false;
        LayerManager.RegisterToLayerManager(layer);
    }

    private static bool IsInDesignMode(DependencyObject d)
    {
#if WPF
        return System.ComponentModel.DesignerProperties.GetIsInDesignMode(d);
#elif WINUI3 || UNO || UWP
        return global::Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
        return false;
#endif
    }
}
