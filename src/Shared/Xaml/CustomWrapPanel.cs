using System;
#if WINUI3 || UNO || UWP
using Windows.Foundation;
#endif

namespace Jamesnet.Foundation;

/// <summary>
/// 가로로 배치하다 폭이 모자라면 다음 줄로 넘기는 패널. 항목 사이 간격(HorizontalSpacing/VerticalSpacing)을 지원한다.
/// </summary>
public class CustomWrapPanel : Panel
{
    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(CustomWrapPanel), new PropertyMetadata(0.0, OnSpacingChanged));

    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(CustomWrapPanel), new PropertyMetadata(0.0, OnSpacingChanged));

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    private static void OnSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomWrapPanel)d).InvalidateMeasure();

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (UIElement child in Children)
        {
            child.Measure(availableSize);
        }

        return Layout(availableSize.Width, arrange: false);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Layout(finalSize.Width, arrange: true);
        return finalSize;
    }

    /// <summary>측정과 배치가 같은 줄바꿈 규칙을 쓰도록 한 곳에서 계산한다.</summary>
    private Size Layout(double maxWidth, bool arrange)
    {
        double x = 0, y = 0, lineHeight = 0, width = 0;

        foreach (UIElement child in Children)
        {
            Size size = child.DesiredSize;

            if (x > 0 && x + size.Width > maxWidth)
            {
                y += lineHeight + VerticalSpacing;
                x = 0;
                lineHeight = 0;
            }

            if (arrange)
            {
                child.Arrange(new Rect(x, y, size.Width, size.Height));
            }

            width = Math.Max(width, x + size.Width);
            x += size.Width + HorizontalSpacing;
            lineHeight = Math.Max(lineHeight, size.Height);
        }

        return new Size(width, y + lineHeight);
    }
}
