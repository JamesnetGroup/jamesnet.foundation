using System.Collections;
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
/// ItemsSource의 각 항목을 자기 자신의 컨테이너로 재귀 렌더링하는
/// 플랫폼 중립 컨트롤. 소스는 이 파일 한 벌이고 각 flavor가 컴파일한다.
///
/// WinUI3와 UWP는 9.x부터 ListBoxItem을 상속해 왔으므로 그대로 유지하고,
/// 나머지 플랫폼은 ContentControl을 상속한다. OnApplyTemplate의 접근성은
/// WPF/OpenSilver 계열(public)과 WinUI 계열(protected)이 다르다.
/// </summary>
public class RecursiveControl :
#if WINUI3 || UWP
    ListBoxItem
#else
    ContentControl
#endif
{
    public RecursiveControl()
    {
        DefaultStyleKey = typeof(RecursiveControl);
    }

    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(RecursiveControl), new PropertyMetadata(null, OnItemsSourceChanged));

    public string ItemsBindingPath
    {
        get => (string)GetValue(ItemsBindingPathProperty);
        set => SetValue(ItemsBindingPathProperty, value);
    }

    public static readonly DependencyProperty ItemsBindingPathProperty =
        DependencyProperty.Register(nameof(ItemsBindingPath), typeof(string), typeof(RecursiveControl), new PropertyMetadata(null));

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RecursiveControl)d).GenerateItems();
    }

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(RecursiveControl), new PropertyMetadata(true, OnIsExpandedChanged));

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RecursiveControl)d).UpdateChildrenVisibility();
    }

    private Panel _itemsPanel;

#if WPF || OPENSILVER
    public override void OnApplyTemplate()
#else
    protected override void OnApplyTemplate()
#endif
    {
        base.OnApplyTemplate();
        _itemsPanel = GetTemplateChild("PART_ItemsPanel") as Panel;
        UpdateChildrenVisibility();
        SetItemsSourceFromDataContext();
    }

    private void SetItemsSourceFromDataContext()
    {
        if (DataContext == null || string.IsNullOrEmpty(ItemsBindingPath)) return;

        var dataContextType = DataContext.GetType();
        var property = dataContextType.GetProperty(ItemsBindingPath);

        if (property != null && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
        {
            var value = property.GetValue(DataContext);
            if (value is IEnumerable enumerable)
            {
                ItemsSource = enumerable;
            }
        }
    }

    private void GenerateItems()
    {
        if (_itemsPanel == null || ItemsSource == null) return;

        _itemsPanel.Children.Clear();

        foreach (var item in ItemsSource as IEnumerable)
        {
            var container = GetContainerForItemOverride();

            if (container is FrameworkElement fe)
            {
                fe.DataContext = item;
                _itemsPanel.Children.Add(fe);
            }
        }
    }

    protected virtual DependencyObject GetContainerForItemOverride()
    {
        return new RecursiveControl();
    }

    private void UpdateChildrenVisibility()
    {
        if (_itemsPanel != null)
        {
            _itemsPanel.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
