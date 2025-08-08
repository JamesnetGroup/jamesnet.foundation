using Jamesnet.Foundation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Jamesnet.Platform.OpenSilver
{
    public class AnimatedOpenSilverLayer : UserControl, ILayer
    {
        public static readonly DependencyProperty LayerNameProperty =
            DependencyProperty.Register(nameof(LayerName), typeof(string), typeof(AnimatedOpenSilverLayer),
                new PropertyMetadata(null, OnLayerNameChanged));

        public static readonly DependencyProperty FadeOutDurationProperty =
            DependencyProperty.Register(nameof(FadeOutDuration), typeof(TimeSpan),
                typeof(AnimatedOpenSilverLayer),
                new PropertyMetadata(TimeSpan.FromMilliseconds(600)));

        public static readonly DependencyProperty FadeInDurationProperty =
            DependencyProperty.Register(nameof(FadeInDuration), typeof(TimeSpan),
                typeof(AnimatedOpenSilverLayer),
                new PropertyMetadata(TimeSpan.FromMilliseconds(400)));

        public static readonly DependencyProperty FadeInDelayProperty =
            DependencyProperty.Register(nameof(FadeInDelay), typeof(TimeSpan),
                typeof(AnimatedOpenSilverLayer),
                new PropertyMetadata(TimeSpan.FromMilliseconds(0)));

        private Grid _containerGrid;
        private ContentControl _currentContent;
        private ContentControl _nextContent;
        private bool _isAnimating = false;
        private Storyboard _currentStoryboard;
        private UIElement _pendingContent = null;
        private readonly Queue<UIElement> _requestHistory = new Queue<UIElement>();

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
                if (_currentContent == null)
                    return;

                if (_currentContent.Content == value)
                {
                    _pendingContent = null;
                    return;
                }

                UIElement newContent = value as UIElement;
                if (newContent == null)
                    return;

                _requestHistory.Enqueue(newContent);
                _pendingContent = newContent;

                if (_isAnimating)
                {
                    _currentStoryboard?.Stop();
                    _currentContent.Opacity = 1;
                    _nextContent.Content = null;
                    _nextContent.Opacity = 0;
                    _isAnimating = false;
                }

                AnimateContentChange(_pendingContent);
            }
        }

        public AnimatedOpenSilverLayer()
        {
            DefaultStyleKey = typeof(AnimatedOpenSilverLayer);

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
                VerticalContentAlignment = VerticalAlignment.Stretch,
                Opacity = 1
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
            if (d is AnimatedOpenSilverLayer layer)
            {
                layer.IsRegistered = false;
                LayerManager.RegisterToLayerManager(layer);
            }
        }

        private void AnimateContentChange(UIElement newContent)
        {
            if (newContent == null)
            {
                _isAnimating = false;
                _pendingContent = null;
                VerifyFinalContent();
                return;
            }

            _isAnimating = true;
            _nextContent.Content = newContent;

            _currentStoryboard = new Storyboard();
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
            _currentStoryboard.Children.Add(fadeOutAnimation);

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
            _currentStoryboard.Children.Add(fadeInAnimation);

            _currentStoryboard.Completed += (s, e) =>
            {
                ContentControl temp = _currentContent;
                _currentContent = _nextContent;
                _nextContent = temp;

                _nextContent.Content = null;
                _nextContent.Opacity = 0;
                _currentContent.Opacity = 1;

                _isAnimating = false;
                _currentStoryboard = null;

                if (_pendingContent != null && _pendingContent != newContent)
                {
                    var nextContent = _pendingContent;
                    _pendingContent = null;
                    AnimateContentChange(nextContent);
                }
                else
                {
                    _pendingContent = null;
                    VerifyFinalContent();
                }
            };

            _currentStoryboard.Begin();
        }

        private void VerifyFinalContent()
        {
            if (_requestHistory.Count == 0)
                return;

            UIElement lastRequestedContent = null;
            while (_requestHistory.Count > 0)
            {
                lastRequestedContent = _requestHistory.Dequeue();
            }

            if (_currentContent.Content != lastRequestedContent)
            {
                _pendingContent = lastRequestedContent;
                AnimateContentChange(_pendingContent);
            }
        }
    }
}