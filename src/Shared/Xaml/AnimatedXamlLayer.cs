using System;

namespace Jamesnet.Foundation;

/// <summary>
/// 뷰가 바뀔 때 페이드 아웃/인으로 전환하는 <see cref="XamlLayer"/>.
/// 전환 중에 새 요청이 여러 번 들어오면 마지막 요청만 기억했다가, 현재 전환이 끝나면 그 뷰로 이어서 전환한다.
/// </summary>
public class AnimatedXamlLayer : XamlLayer
{
    public static readonly DependencyProperty FadeOutDurationProperty =
        DependencyProperty.Register(nameof(FadeOutDuration), typeof(TimeSpan), typeof(AnimatedXamlLayer), new PropertyMetadata(TimeSpan.FromMilliseconds(600)));

    public static readonly DependencyProperty FadeInDurationProperty =
        DependencyProperty.Register(nameof(FadeInDuration), typeof(TimeSpan), typeof(AnimatedXamlLayer), new PropertyMetadata(TimeSpan.FromMilliseconds(400)));

    public static readonly DependencyProperty FadeInDelayProperty =
        DependencyProperty.Register(nameof(FadeInDelay), typeof(TimeSpan), typeof(AnimatedXamlLayer), new PropertyMetadata(TimeSpan.Zero));

    public TimeSpan FadeOutDuration
    {
        get => (TimeSpan)GetValue(FadeOutDurationProperty);
        set => SetValue(FadeOutDurationProperty, value);
    }

    public TimeSpan FadeInDuration
    {
        get => (TimeSpan)GetValue(FadeInDurationProperty);
        set => SetValue(FadeInDurationProperty, value);
    }

    public TimeSpan FadeInDelay
    {
        get => (TimeSpan)GetValue(FadeInDelayProperty);
        set => SetValue(FadeInDelayProperty, value);
    }

    private ContentControl _current = CreatePresenter(1);
    private ContentControl _next = CreatePresenter(0);
    private bool _isAnimating;
    private bool _hasPending;
    private UIElement _pending;

    public AnimatedXamlLayer()
    {
        var grid = new Grid();
        grid.Children.Add(_current);
        grid.Children.Add(_next);
        Content = grid;

        HorizontalContentAlignment = HorizontalAlignment.Stretch;
        VerticalContentAlignment = VerticalAlignment.Stretch;
    }

    public override object UIContent
    {
        get => _current.Content;
        set
        {
            var view = value as UIElement;
            if (_isAnimating)
            {
                _pending = view;
                _hasPending = true;
            }
            else if (!ReferenceEquals(_current.Content, view))
            {
                Transition(view);
            }
        }
    }

    private void Transition(UIElement view)
    {
        _isAnimating = true;
        _next.Content = view;

        var easing = new CubicEase { EasingMode = EasingMode.EaseInOut };
        var storyboard = new Storyboard();
        storyboard.Children.Add(Fade(_current, 1, 0, FadeOutDuration, TimeSpan.Zero, easing));
        storyboard.Children.Add(Fade(_next, 0, 1, FadeInDuration, FadeInDelay, easing));
        storyboard.Completed += (_, _) =>
        {
            (_current, _next) = (_next, _current);
            _next.Content = null;
            _next.Opacity = 0;
            _current.Opacity = 1;
            _isAnimating = false;

            if (_hasPending)
            {
                _hasPending = false;
                var pending = _pending;
                _pending = null;
                UIContent = pending;
            }
        };
        storyboard.Begin();
    }

    private static DoubleAnimation Fade(UIElement target, double from, double to, TimeSpan duration, TimeSpan delay, EasingFunctionBase easing)
    {
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = new Duration(duration),
            BeginTime = delay,
            EasingFunction = easing,
        };
        Storyboard.SetTarget(animation, target);
#if WINUI3 || UNO || UWP
        Storyboard.SetTargetProperty(animation, "Opacity");
#else
        Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
#endif
        return animation;
    }

    private static ContentControl CreatePresenter(double opacity) => new()
    {
        Opacity = opacity,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Stretch,
    };
}
