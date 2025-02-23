using System.Windows.Browser;
using System;
using System.Windows;

namespace Jamesnet.Platform.OpenSilver.Scripts;

public class DeviceInfo
{
    private const double _mobileWidth = 600;
    private static bool _useDocsDockMode;
    public static bool IsMobile { get; private set; }
    public static event Action<bool> OnDockModeChanged = null;

    public static bool UseDockMode
    {
        get => _useDocsDockMode;
        set
        {
            _useDocsDockMode = value;
            OnDockModeChanged?.Invoke(value);
        }
    }

    public static Visibility AllowDockMode => !IsMobile ? Visibility.Visible : Visibility.Collapsed;

    static DeviceInfo()
    {
        var width = WindowWidth;
        var height = WindowHeight;

        // 가로나 세로 중 작은 쪽이 600px 이하면 모바일로 간주
        var smallerDimension = Math.Min(width, height);
        IsMobile = smallerDimension <= _mobileWidth;
        UseDockMode = IsMobile;
    }

    public static double WindowWidth => Convert.ToDouble(HtmlPage.Window.Eval("window.innerWidth"));
    public static double WindowHeight => Convert.ToDouble(HtmlPage.Window.Eval("window.innerHeight"));
}