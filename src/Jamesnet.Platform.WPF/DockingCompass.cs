using Jamesnet.Foundation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Jamesnet.Platform.Wpf;

public class DockingCompass : Grid
{
    private const double ButtonSize = 32;
    private const double GapSize = 4;
    private Border _centerBtn, _leftBtn, _topBtn, _rightBtn, _bottomBtn;
    public CompassTarget CurrentHover { get; private set; } = CompassTarget.None;

    public DockingCompass()
    {
        Width = Height = (ButtonSize * 3) + (GapSize * 2);
        for (int i = 0; i < 5; i++) { ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(i % 2 == 0 ? ButtonSize : GapSize) }); RowDefinitions.Add(new RowDefinition { Height = new GridLength(i % 2 == 0 ? ButtonSize : GapSize) }); }
        _topBtn = CreateCompassButton(Geometry.Parse("M 4,12 L 12,4 L 20,12"), 2, 0);
        _leftBtn = CreateCompassButton(Geometry.Parse("M 12,4 L 4,12 L 12,20"), 0, 2);
        _centerBtn = CreateCompassButton(Geometry.Parse("M 6,6 L 18,6 L 18,18 L 6,18 Z"), 2, 2);
        _rightBtn = CreateCompassButton(Geometry.Parse("M 4,4 L 12,12 L 4,20"), 4, 2);
        _bottomBtn = CreateCompassButton(Geometry.Parse("M 4,4 L 12,12 L 20,4"), 2, 4);
        Children.Add(_topBtn); Children.Add(_leftBtn); Children.Add(_centerBtn); Children.Add(_rightBtn); Children.Add(_bottomBtn);
    }

    private Border CreateCompassButton(Geometry data, int c, int r)
    {
        var b = new Border { Background = new SolidColorBrush(Color.FromArgb(230, 245, 245, 245)), BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(2), Child = new Path { Data = data, Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 204)), StrokeThickness = 2, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, IsHitTestVisible = false } };
        Grid.SetColumn(b, c); Grid.SetRow(b, r); return b;
    }

    public bool UpdateHover(Point p)
    {
        ResetStyle(_centerBtn); ResetStyle(_leftBtn); ResetStyle(_topBtn); ResetStyle(_rightBtn); ResetStyle(_bottomBtn);
        CurrentHover = CompassTarget.None;
        if (IsOver(_centerBtn, p)) { CurrentHover = CompassTarget.Center; Highlight(_centerBtn); }
        else if (IsOver(_leftBtn, p)) { CurrentHover = CompassTarget.Left; Highlight(_leftBtn); }
        else if (IsOver(_topBtn, p)) { CurrentHover = CompassTarget.Top; Highlight(_topBtn); }
        else if (IsOver(_rightBtn, p)) { CurrentHover = CompassTarget.Right; Highlight(_rightBtn); }
        else if (IsOver(_bottomBtn, p)) { CurrentHover = CompassTarget.Bottom; Highlight(_bottomBtn); }
        return CurrentHover != CompassTarget.None;
    }

    private bool IsOver(FrameworkElement e, Point p) { var t = e.TransformToAncestor(this); var rp = t.Transform(new Point(0, 0)); return p.X >= rp.X && p.X <= rp.X + e.ActualWidth && p.Y >= rp.Y && p.Y <= rp.Y + e.ActualHeight; }
    private void Highlight(Border b) { b.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204)); if (b.Child is Path p) p.Stroke = Brushes.White; }
    private void ResetStyle(Border b) { b.Background = new SolidColorBrush(Color.FromArgb(230, 245, 245, 245)); if (b.Child is Path p) p.Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 204)); }
}