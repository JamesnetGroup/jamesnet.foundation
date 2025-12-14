using Jamesnet.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace Jamesnet.Platform.Wpf;

public class DockingCompass : Grid
{
    private const double ButtonSize = 32; // 버튼 크기
    private const double GapSize = 4;     // 버튼 간격

    // 시각적 요소들
    private Border _centerBtn;
    private Border _leftBtn;
    private Border _topBtn;
    private Border _rightBtn;
    private Border _bottomBtn;

    public CompassTarget CurrentHover { get; private set; } = CompassTarget.None;

    public DockingCompass()
    {
        // 전체 크기 (3칸 + 간격)
        Width = (ButtonSize * 3) + (GapSize * 2);
        Height = (ButtonSize * 3) + (GapSize * 2);

        // 레이아웃 정의 (5x5 Grid로 간격까지 표현)
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonSize) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GapSize) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonSize) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GapSize) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ButtonSize) });

        RowDefinitions.Add(new RowDefinition { Height = new GridLength(ButtonSize) });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(GapSize) });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(ButtonSize) });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(GapSize) });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(ButtonSize) });

        // 버튼 생성 (Chevron 스타일 아이콘)

        // Top (Row 0, Col 2)
        _topBtn = CreateCompassButton(
            Geometry.Parse("M 4,12 L 12,4 L 20,12"),
            2, 0);

        // Left (Row 2, Col 0)
        _leftBtn = CreateCompassButton(
            Geometry.Parse("M 12,4 L 4,12 L 12,20"),
            0, 2);

        // Center (Row 2, Col 2) - 네모 박스
        _centerBtn = CreateCompassButton(
            Geometry.Parse("M 6,6 L 18,6 L 18,18 L 6,18 Z"),
            2, 2);

        // Right (Row 2, Col 4)
        _rightBtn = CreateCompassButton(
            Geometry.Parse("M 4,4 L 12,12 L 4,20"),
            4, 2);

        // Bottom (Row 4, Col 2)
        _bottomBtn = CreateCompassButton(
            Geometry.Parse("M 4,4 L 12,12 L 20,4"),
            2, 4);

        Children.Add(_topBtn);
        Children.Add(_leftBtn);
        Children.Add(_centerBtn);
        Children.Add(_rightBtn);
        Children.Add(_bottomBtn);
    }

    private Border CreateCompassButton(Geometry iconData, int col, int row)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(230, 245, 245, 245)), // 평소 색상
            BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204)), // 테두리 (VS Blue)
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Child = new Path
            {
                Data = iconData,
                Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                StrokeThickness = 2,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false // 마우스 이벤트는 Border가 받도록
            }
        };

        Grid.SetColumn(border, col);
        Grid.SetRow(border, row);
        return border;
    }

    // 마우스 위치에 따라 상태 업데이트
    public bool UpdateHover(Point mouseP)
    {
        // 모든 버튼 스타일 초기화
        ResetStyle(_centerBtn);
        ResetStyle(_leftBtn);
        ResetStyle(_topBtn);
        ResetStyle(_rightBtn);
        ResetStyle(_bottomBtn);

        CurrentHover = CompassTarget.None;

        // HitTest 수행
        if (IsOver(_centerBtn, mouseP)) { CurrentHover = CompassTarget.Center; Highlight(_centerBtn); }
        else if (IsOver(_leftBtn, mouseP)) { CurrentHover = CompassTarget.Left; Highlight(_leftBtn); }
        else if (IsOver(_topBtn, mouseP)) { CurrentHover = CompassTarget.Top; Highlight(_topBtn); }
        else if (IsOver(_rightBtn, mouseP)) { CurrentHover = CompassTarget.Right; Highlight(_rightBtn); }
        else if (IsOver(_bottomBtn, mouseP)) { CurrentHover = CompassTarget.Bottom; Highlight(_bottomBtn); }

        return CurrentHover != CompassTarget.None;
    }

    private bool IsOver(FrameworkElement elem, Point p)
    {
        // Grid 내 상대 좌표 계산 (Canvas.Left 등이 아닌 실제 레이아웃 좌표)
        var transform = elem.TransformToAncestor(this);
        var rootP = transform.Transform(new Point(0, 0));

        return p.X >= rootP.X && p.X <= rootP.X + elem.ActualWidth &&
               p.Y >= rootP.Y && p.Y <= rootP.Y + elem.ActualHeight;
    }

    private void Highlight(Border elem)
    {
        // 마우스 오버 시 색상 변경 (진한 파랑 배경 + 흰색 아이콘)
        elem.Background = new SolidColorBrush(Color.FromRgb(0, 122, 204));
        if (elem.Child is Path p)
        {
            p.Stroke = Brushes.White;
        }
    }

    private void ResetStyle(Border elem)
    {
        // 평소 상태 복구
        elem.Background = new SolidColorBrush(Color.FromArgb(230, 245, 245, 245));
        if (elem.Child is Path p)
        {
            p.Stroke = new SolidColorBrush(Color.FromRgb(0, 122, 204));
        }
    }
}
