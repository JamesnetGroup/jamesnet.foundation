using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Jamesnet.Platform.OpenSilver.Responsive
{
    public static class ResponsiveColumns
    {
        public static readonly DependencyProperty BaseWidthProperty =
            DependencyProperty.RegisterAttached("BaseWidth", typeof(double), typeof(ResponsiveColumns),
                new PropertyMetadata(300.0, OnColumnsChanged));

        public static double GetBaseWidth(DependencyObject obj) => (double)obj.GetValue(BaseWidthProperty);
        public static void SetBaseWidth(DependencyObject obj, double value) => obj.SetValue(BaseWidthProperty, value);

        public static readonly DependencyProperty MinColumnsProperty =
            DependencyProperty.RegisterAttached("MinColumns", typeof(int), typeof(ResponsiveColumns),
                new PropertyMetadata(1, OnColumnsChanged));

        public static int GetMinColumns(DependencyObject obj) => (int)obj.GetValue(MinColumnsProperty);
        public static void SetMinColumns(DependencyObject obj, int value) => obj.SetValue(MinColumnsProperty, value);

        public static readonly DependencyProperty MaxColumnsProperty =
            DependencyProperty.RegisterAttached("MaxColumns", typeof(int), typeof(ResponsiveColumns),
                new PropertyMetadata(0, OnColumnsChanged));

        public static int GetMaxColumns(DependencyObject obj) => (int)obj.GetValue(MaxColumnsProperty);
        public static void SetMaxColumns(DependencyObject obj, int value) => obj.SetValue(MaxColumnsProperty, value);

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UniformGrid grid)
            {
                // 이벤트 핸들러 등록 (중복 방지)
                if (!IsHandlerRegistered(grid))
                {
                    grid.SizeChanged += (s, args) => UpdateColumns(grid);
                    grid.Loaded += (s, args) => UpdateColumns(grid);
                    RegisterHandler(grid);
                }
                UpdateColumns(grid);
            }
        }

        private static void UpdateColumns(UniformGrid grid)
        {
            double windowWidth = grid.ActualWidth;
            double baseWidth = GetBaseWidth(grid);
            int minColumns = GetMinColumns(grid);
            int maxColumns = GetMaxColumns(grid);

            int calculatedColumns = (int)Math.Floor(windowWidth / baseWidth);

            int columns = Math.Max(minColumns, calculatedColumns);
            if (maxColumns > 0)
                columns = Math.Min(columns, maxColumns);

            grid.Columns = columns;
        }

        private static readonly DependencyProperty IsHandlerRegisteredProperty =
            DependencyProperty.RegisterAttached("IsHandlerRegistered", typeof(bool), typeof(ResponsiveColumns),
                new PropertyMetadata(false));

        private static bool IsHandlerRegistered(DependencyObject obj) => (bool)obj.GetValue(IsHandlerRegisteredProperty);
        private static void RegisterHandler(DependencyObject obj) => obj.SetValue(IsHandlerRegisteredProperty, true);
    }
}