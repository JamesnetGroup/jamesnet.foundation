namespace Jamesnet.Foundation;

/// <summary>
/// 플랫폼 중립 레이어 컨트롤. WPF, OpenSilver, WinUI3, Uno, UWP에서 동일한 이름과
/// 동일한 동작으로 사용한다. 플랫폼마다 별도의 어셈블리로 컴파일되지만
/// 소스는 이 파일 한 벌뿐이다.
/// </summary>
/// <remarks>
/// DefaultStyleKey를 바꾸지 않는다. 바꾸면 해당 키의 테마 스타일(Generic.xaml)이 없는 플랫폼에서
/// 템플릿이 비어 아무것도 그려지지 않는다. 기본 ContentControl/UserControl 템플릿을 그대로 쓴다.
/// </remarks>
public class XamlLayer :
#if OPENSILVER
    UserControl
#else
    ContentControl
#endif
    , ILayer
{
    public static readonly DependencyProperty LayerNameProperty =
        DependencyProperty.Register(nameof(LayerName), typeof(string), typeof(XamlLayer), new PropertyMetadata(null, OnLayerNameChanged));

    public bool IsRegistered { get; set; }

    public string LayerName
    {
        get => (string)GetValue(LayerNameProperty);
        set => SetValue(LayerNameProperty, value);
    }

    /// <summary>LayerManager가 보여줄 뷰. 파생 레이어는 전환 방식을 바꾸려면 재정의한다.</summary>
    public virtual object UIContent
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
        if (!IsInDesignMode(this))
        {
            LayerManager.InitializeLayer(this);
        }
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
