using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Jamesnet.Foundation.Responsive;
using Xunit;

namespace Jamesnet.Foundation.Tests;

/// <summary>
/// 실제 WPF 창을 화면 밖에 띄워 XAML 쪽 동작을 확인한다.
/// 각 테스트는 전용 STA 스레드에서 돈다.
/// </summary>
public class WpfTests
{
    [Fact]
    public void XamlLayer_renders_its_content() => Sta(() =>
    {
        var layer = new XamlLayer { Content = new TextBlock { Text = "hello" } };
        using var w = new TestWindow(layer, 1000);

        Assert.NotNull(Find<TextBlock>(w.Window));
    });

    [Fact]
    public void Responsive_properties_follow_window_width() => Sta(() =>
    {
        var border = new Border { Tag = "user-tag" };
        ResponsiveWidth.SetDesktop(border, 200);
        ResponsiveWidth.SetMobile(border, 50);
        ResponsiveMargin.SetDesktop(border, new Thickness(10));
        ResponsiveMargin.SetMobile(border, new Thickness(1));

        using var w = new TestWindow(border, 1000);
        Assert.Equal(200, border.Width);
        Assert.Equal(10, border.Margin.Left);

        w.Resize(500);
        Assert.Equal(50, border.Width);
        Assert.Equal(1, border.Margin.Left);
        Assert.Equal("user-tag", border.Tag);
    });

    [Fact]
    public void Responsive_element_keeps_tracking_after_reattach() => Sta(() =>
    {
        var border = new Border();
        ResponsiveWidth.SetDesktop(border, 200);
        ResponsiveWidth.SetMobile(border, 50);
        var layer = new XamlLayer { Content = border };
        using var w = new TestWindow(layer, 500);

        layer.Content = null;
        Pump();
        w.Resize(1000);
        layer.Content = border;
        Pump();
        Assert.Equal(200, border.Width);

        w.Resize(600);
        Assert.Equal(50, border.Width);
    });

    [Fact]
    public void Unset_responsive_value_falls_back_to_inherited() => Sta(() =>
    {
        var tb = new TextBlock();
        double inherited = tb.FontSize;
        ResponsiveFontSize.SetDesktop(tb, 30);

        using var w = new TestWindow(tb, 500);
        Assert.Equal(inherited, tb.FontSize);

        w.Resize(1000);
        Assert.Equal(30, tb.FontSize);
    });

    [Fact]
    public void AnimatedXamlLayer_ends_on_last_requested_view() => Sta(() =>
    {
        var anim = new AnimatedXamlLayer
        {
            FadeInDuration = TimeSpan.FromMilliseconds(50),
            FadeOutDuration = TimeSpan.FromMilliseconds(50),
        };
        using var w = new TestWindow(anim, 1000);
        var v1 = new TextBlock();
        var v2 = new TextBlock();
        var v3 = new TextBlock();

        anim.UIContent = v1;
        anim.UIContent = v2;
        anim.UIContent = v3;
        Pump(600);

        Assert.Same(v3, anim.UIContent);
    });

    [Fact]
    public void XamlLayer_registers_to_LayerManager_on_load() => Sta(() =>
    {
        var container = new Container();
        container.RegisterSingleton<ILayerManager, LayerManager>();
        ContainerProvider.SetContainer(container);

        var layer = new XamlLayer { LayerName = "TestLayer" };
        using var w = new TestWindow(layer, 1000);

        Assert.Same(layer, container.Resolve<ILayerManager>().GetLayer("TestLayer"));
    });

    private sealed class TestWindow : IDisposable
    {
        public Window Window { get; }

        public TestWindow(object content, double width)
        {
            Window = new Window
            {
                Width = width,
                Height = 300,
                Content = content,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -5000,
            };
            Window.Show();
            Pump();
        }

        public void Resize(double width)
        {
            Window.Width = width;
            Pump();
        }

        public void Dispose() => Window.Close();
    }

    private static void Sta(Action test)
    {
        ExceptionDispatchInfo error = null;
        var thread = new Thread(() =>
        {
            try { test(); }
            catch (Exception e) { error = ExceptionDispatchInfo.Capture(e); }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        error?.Throw();
    }

    private static void Pump(int ms = 50)
    {
        var end = DateTime.Now.AddMilliseconds(ms);
        do
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => frame.Continue = false);
            Dispatcher.PushFrame(frame);
        }
        while (DateTime.Now < end);
    }

    private static T Find<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match || (match = Find<T>(child)) != null)
            {
                return match;
            }
        }
        return null;
    }
}
