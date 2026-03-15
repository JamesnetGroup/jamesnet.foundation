using System.Windows;
using Jamesnet.Foundation;

namespace Jamesnet.Platform.Wpf.Responsive;

internal class WpfResponsiveHost : IResponsiveHost
{
    private readonly Dictionary<object, SizeChangedEventHandler> _handlers = new();

    public double GetWidth(object element)
    {
        if (element is FrameworkElement fe)
        {
            Window window = Window.GetWindow(fe);
            return window?.ActualWidth ?? 0;
        }
        return 0;
    }

    public void Subscribe(object element, Action callback)
    {
        if (element is not FrameworkElement fe) return;

        Window window = Window.GetWindow(fe);
        if (window == null) return;

        SizeChangedEventHandler handler = (s, e) => callback();
        _handlers[element] = handler;
        window.SizeChanged += handler;
    }

    public void Unsubscribe(object element)
    {
        if (element is not FrameworkElement fe) return;
        if (!_handlers.Remove(element, out var handler)) return;

        Window window = Window.GetWindow(fe);
        if (window != null)
            window.SizeChanged -= handler;
    }
}

internal static class ResponsiveHelper
{
    private static readonly WpfResponsiveHost _host = new();
    private static readonly ResponsiveManager _manager = new(_host);

    private static readonly DependencyProperty IsRegisteredProperty =
        DependencyProperty.RegisterAttached("IsRegistered", typeof(bool), typeof(ResponsiveHelper),
            new PropertyMetadata(false));

    internal static void Register(FrameworkElement element, Action<FrameworkElement, double> updateAction)
    {
        if ((bool)element.GetValue(IsRegisteredProperty))
        {
            double width = _host.GetWidth(element);
            if (width > 0)
                updateAction(element, width);
            return;
        }

        element.SetValue(IsRegisteredProperty, true);

        element.Loaded += OnLoaded;
        element.Unloaded += OnUnloaded;

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var adapter = new ResponsiveAdapter(element, updateAction);
            element.Tag = adapter;

            _manager.Register(element, adapter);
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _manager.Unregister(element);
            element.SetValue(IsRegisteredProperty, false);

            element.Loaded -= OnLoaded;
            element.Unloaded -= OnUnloaded;
        }
    }

    private class ResponsiveAdapter : IResponsiveElement
    {
        private readonly FrameworkElement _element;
        private readonly Action<FrameworkElement, double> _updateAction;

        public double Breakpoint => 0;

        public ResponsiveAdapter(FrameworkElement element, Action<FrameworkElement, double> updateAction)
        {
            _element = element;
            _updateAction = updateAction;
        }

        public void Update(double width)
        {
            _updateAction(_element, width);
        }
    }
}
