using Jamesnet.Foundation;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Jamesnet.Platform.Wpf
{
    public class AnimatedWPFLayer : UserControl, ILayer
    {
        public static readonly DependencyProperty LayerNameProperty =
            DependencyProperty.Register(nameof(LayerName), typeof(string), typeof(AnimatedWPFLayer),
                new PropertyMetadata(null, OnLayerNameChanged));

        public static readonly DependencyProperty FadeOutDurationProperty =
            DependencyProperty.Register(nameof(FadeOutDuration), typeof(TimeSpan),
                typeof(AnimatedWPFLayer),
                new PropertyMetadata(TimeSpan.FromMilliseconds(600)));

        public static readonly DependencyProperty FadeInDurationProperty =
            DependencyProperty.Register(nameof(FadeInDuration), typeof(TimeSpan),
                typeof(AnimatedWPFLayer),
                new PropertyMetadata(TimeSpan.FromMilliseconds(400)));

        public static readonly DependencyProperty FadeInDelayProperty =
            DependencyProperty.Register(nameof(FadeInDelay), typeof(TimeSpan),
                typeof(AnimatedWPFLayer),
                new PropertyMetadata(TimeSpan.FromMilliseconds(0)));

        private Grid _containerGrid;
        private ContentControl _currentContent;
        private ContentControl _nextContent;
        private bool _isAnimating = false;

        public bool IsRegistered { get; set; }

        public string LayerName
        {
            get => (string)GetValue(LayerNameProperty);
            set => SetValue(LayerNameProperty, value);
        }

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

        public object UIContent
        {
            get => _currentContent?.Content;
            set
            {
                if (_currentContent == null || _isAnimating)
                    return;

                if (_currentContent.Content == null || _currentContent.Content == value)
                {
                    _currentContent.Content = value;
                    return;
                }

                AnimateContentChange(value as UIElement);
            }
        }

        public AnimatedWPFLayer()
        {
            DefaultStyleKey = typeof(AnimatedWPFLayer);

            _containerGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _currentContent = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };

            _nextContent = new ContentControl
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Opacity = 0
            };

            _containerGrid.Children.Add(_currentContent);
            _containerGrid.Children.Add(_nextContent);

            base.Content = _containerGrid;

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            LayerManager.InitializeLayer(this);
        }

        private static void OnLayerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedWPFLayer layer)
            {
                layer.IsRegistered = false;
                LayerManager.RegisterToLayerManager(layer);
            }
        }

        private void AnimateContentChange(UIElement newContent)
        {
            _isAnimating = true;

            _nextContent.Content = newContent;

            Storyboard storyboard = new Storyboard();

            CubicEase easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            DoubleAnimation fadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = new Duration(FadeOutDuration),
                EasingFunction = easing
            };
            Storyboard.SetTarget(fadeOutAnimation, _currentContent);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOutAnimation);

            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                BeginTime = FadeInDelay, 
                Duration = new Duration(FadeInDuration),
                EasingFunction = easing
            };
            Storyboard.SetTarget(fadeInAnimation, _nextContent);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeInAnimation);

            TimeSpan totalDuration = TimeSpan.FromTicks(
                Math.Max(
                    FadeInDelay.Ticks + FadeInDuration.Ticks,
                    FadeOutDuration.Ticks
                )
            );

            storyboard.Completed += (s, e) =>
            {
                ContentControl temp = _currentContent;
                _currentContent = _nextContent;
                _nextContent = temp;

                _nextContent.Content = null;
                _nextContent.Opacity = 0;

                _isAnimating = false;
            };

            storyboard.Begin();
        }
    }
}