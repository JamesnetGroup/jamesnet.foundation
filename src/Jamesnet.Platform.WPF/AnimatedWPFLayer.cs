using Jamesnet.Foundation;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

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
        private Storyboard _currentStoryboard;
        private UIElement _pendingContent = null;
        private readonly Queue<UIElement> _requestHistory = new Queue<UIElement>(); // 변경된 콘텐츠 순서 기억

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
                Dispatcher.BeginInvoke(new Action(() =>
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

                    // 변경된 콘텐츠를 순서대로 기억
                    _requestHistory.Enqueue(newContent);
                    _pendingContent = newContent; // 최신 콘텐츠만 애니메이션 처리

                    // 애니메이션 중이면 중단
                    if (_isAnimating)
                    {
                        _currentStoryboard?.Stop();
                        _currentContent.Opacity = 1;
                        _nextContent.Content = null;
                        _nextContent.Opacity = 0;
                        _isAnimating = false;
                    }

                    AnimateContentChange(_pendingContent);
                }), DispatcherPriority.Input);
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
            if (d is AnimatedWPFLayer layer)
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
                VerifyFinalContent(); // 마지막 체크
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

                // _pendingContent가 있으면 계속 처리
                if (_pendingContent != null && _pendingContent != newContent)
                {
                    var nextContent = _pendingContent;
                    _pendingContent = null;
                    AnimateContentChange(nextContent);
                }
                else
                {
                    _pendingContent = null;
                    VerifyFinalContent(); // 마지막 체크
                }
            };

            _currentStoryboard.Begin();
        }

        private void VerifyFinalContent()
        {
            if (_requestHistory.Count == 0)
                return;

            // 큐에서 마지막 요청 콘텐츠 가져오기
            UIElement lastRequestedContent = null;
            while (_requestHistory.Count > 0)
            {
                lastRequestedContent = _requestHistory.Dequeue();
            }

            // 최종 표시된 콘텐츠와 비교
            if (_currentContent.Content != lastRequestedContent)
            {
                Console.WriteLine($"Mismatch detected! Last requested: {lastRequestedContent}, Displayed: {_currentContent.Content}");
                // 불일치 시 마지막 콘텐츠로 강제 업데이트
                _pendingContent = lastRequestedContent;
                AnimateContentChange(_pendingContent);
            }
            else
            {
                Console.WriteLine("Final content matches the last request.");
            }
        }
    }
}