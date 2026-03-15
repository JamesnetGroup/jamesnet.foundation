using System.Windows;
using System.Windows.Controls.Primitives;

namespace Jamesnet.Platform.Wpf.Responsive;

public static class ResponsiveColumns
{
    public static readonly DependencyProperty BaseWidthProperty =
        DependencyProperty.RegisterAttached("BaseWidth", typeof(double), typeof(ResponsiveColumns),
            new PropertyMetadata(300.0, OnPropertyChanged));
    public static double GetBaseWidth(DependencyObject obj) => (double)obj.GetValue(BaseWidthProperty);
    public static void SetBaseWidth(DependencyObject obj, double value) => obj.SetValue(BaseWidthProperty, value);

    public static readonly DependencyProperty MinColumnsProperty =
        DependencyProperty.RegisterAttached("MinColumns", typeof(int), typeof(ResponsiveColumns),
            new PropertyMetadata(1, OnPropertyChanged));
    public static int GetMinColumns(DependencyObject obj) => (int)obj.GetValue(MinColumnsProperty);
    public static void SetMinColumns(DependencyObject obj, int value) => obj.SetValue(MinColumnsProperty, value);

    public static readonly DependencyProperty MaxColumnsProperty =
        DependencyProperty.RegisterAttached("MaxColumns", typeof(int), typeof(ResponsiveColumns),
            new PropertyMetadata(0, OnPropertyChanged));
    public static int GetMaxColumns(DependencyObject obj) => (int)obj.GetValue(MaxColumnsProperty);
    public static void SetMaxColumns(DependencyObject obj, int value) => obj.SetValue(MaxColumnsProperty, value);

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UniformGrid grid)
            ResponsiveHelper.Register(grid, UpdateColumns);
    }

    private static void UpdateColumns(FrameworkElement element, double windowWidth)
    {
        var grid = (UniformGrid)element;
        double baseWidth = GetBaseWidth(grid);
        int minColumns = GetMinColumns(grid);
        int maxColumns = GetMaxColumns(grid);

        int columns = Math.Max(minColumns, (int)Math.Floor(windowWidth / baseWidth));
        if (maxColumns > 0)
            columns = Math.Min(columns, maxColumns);

        grid.Columns = columns;
    }
}
