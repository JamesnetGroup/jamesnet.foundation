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

            // 컨테이너 초기화 및 크기 설정
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

            // 사용자 컨트롤의 Content에 내부 컨테이너 설정
            base.Content = _containerGrid;

            // 사용자 컨트롤 자체의 크기 설정
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            // LayerManager 등록
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

            // 새 콘텐츠 설정
            _nextContent.Content = newContent;

            // 페이드 애니메이션 생성
            Storyboard storyboard = new Storyboard();

            // 애니메이션에 이징 함수 추가
            CubicEase easing = new CubicEase { EasingMode = EasingMode.EaseInOut };

            // 기존 콘텐츠 페이드 아웃
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

            // 새 콘텐츠 페이드 인 - 지정된 지연 시간 후 시작
            DoubleAnimation fadeInAnimation = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                BeginTime = FadeInDelay, // 지정된 지연 시간 후 시작
                Duration = new Duration(FadeInDuration),
                EasingFunction = easing
            };
            Storyboard.SetTarget(fadeInAnimation, _nextContent);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeInAnimation);

            // 전체 애니메이션 시간 계산 (페이드인 지연 + 페이드인 지속시간과 페이드아웃 지속시간 중 큰 값)
            TimeSpan totalDuration = TimeSpan.FromTicks(
                Math.Max(
                    FadeInDelay.Ticks + FadeInDuration.Ticks,
                    FadeOutDuration.Ticks
                )
            );

            // 애니메이션 완료 후 정리 작업
            storyboard.Completed += (s, e) =>
            {
                // 콘텐츠 컨트롤 교체
                ContentControl temp = _currentContent;
                _currentContent = _nextContent;
                _nextContent = temp;

                // 새 콘텐츠 컨트롤 초기화
                _nextContent.Content = null;
                _nextContent.Opacity = 0;

                _isAnimating = false;
            };

            // 애니메이션 시작
            storyboard.Begin();
        }
    }
}