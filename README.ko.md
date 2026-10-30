# Jamesnet.Foundation

[English](README.md) | [한국어](README.ko.md) | [中文](README.cn.md)

`Jamesnet.Foundation`은 XAML 기반 크로스 플랫폼 MVVM 라이브러리입니다. **패키지 하나로 모든 플랫폼을 지원합니다.** `Jamesnet.Foundation`만 설치하면 빌드할 때 플랫폼에 맞는 어셈블리가 자동으로 선택됩니다.

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **v10 — 단일 패키지.** 기존 `Jamesnet.Platform.*` 패키지들은 `Jamesnet.Foundation` 하나로 합쳐졌습니다. [9.x에서 올라오기](#9x에서-올라오기)를 참고하세요.

## 지원 플랫폼

| 플랫폼 | 기준 버전 |
|---|---|
| WPF | .NET 10 |
| OpenSilver | 3.3.3 |
| Uno Platform | Uno.Sdk 6.7.30 |
| WinUI 3 | Windows App SDK 2.6.4 |
| UWP | UniversalWindowsPlatform 6.2.15 |

## 설치

```powershell
dotnet add package Jamesnet.Foundation
```

플랫폼별 패키지를 따로 추가할 필요가 없습니다.

## 사용법

### 1. 부트스트래퍼

앱 시작 시 한 번, 뷰·뷰모델·첫 화면을 등록합니다.

```csharp
using Jamesnet.Foundation;

public class MyAppBootstrapper : AppBootstrapper
{
    protected override void RegisterViewModels(IViewModelMapper viewModelMapper)
    {
        viewModelMapper.Register<MainContent, MainViewModel>();
    }

    protected override void RegisterDependencies(IContainer container)
    {
        container.RegisterSingleton<IView, MainContent>(nameof(MainContent));
    }

    protected override void SettingsLayer(ILayerManager layer, IContainer container)
    {
        layer.Mapping("Main", container.Resolve<IView>(nameof(MainContent)));
    }
}
```

앱이 시작될 때 `new MyAppBootstrapper().Run();`을 호출합니다. (WPF는 `App` 생성자, Uno·WinUI 3는 `OnLaunched`)

### 2. XAML 레이어

레이어는 이름이 붙은 자리입니다. 화면에 올라오면 스스로 등록되고, 그 이름에 매핑된 뷰를 보여줍니다.

```xml
<!-- WPF, OpenSilver -->
<Window xmlns:james="http://jamesnet.dev/xaml">
    <james:AnimatedXamlLayer LayerName="Main"/>
</Window>

<!-- Uno, WinUI 3, UWP -->
<Page xmlns:james="using:Jamesnet.Foundation">
    <james:AnimatedXamlLayer LayerName="Main"/>
</Page>
```

`XamlLayer`는 내용을 즉시 바꾸고, `AnimatedXamlLayer`는 페이드로 전환합니다.

### 3. 뷰모델

```csharp
public class MainViewModel : ViewModelBase, IViewLoadable
{
    private readonly ILayerManager _layers;
    private readonly IContainer _container;
    private string _name;

    public MainViewModel(ILayerManager layers, IContainer container)
    {
        _layers = layers;
        _container = container;
        SaveCommand = new RelayCommand(Save, () => !string.IsNullOrEmpty(Name));
    }

    [CommandTrigger(nameof(SaveCommand))] // Name이 바뀌면 SaveCommand.CanExecute를 다시 평가
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public RelayCommand SaveCommand { get; }

    public void OnLoaded(object view) { }

    private void Save() => _layers.Show("Content", _container.Resolve<IView>("DetailContent"));
}
```

뷰는 클래스에 `IView`를 붙입니다(`public partial class MainContent : UserControl, IView`). 컨테이너에서 뷰를 꺼내면 매핑된 뷰모델이 자동으로 만들어져 `DataContext`에 들어갑니다.

### 4. 반응형 속성

창 너비에 따라 값을 바꿉니다(기본 기준 768).

```xml
<ListView james:ResponsiveWidth.Desktop="200"
          james:ResponsiveWidth.Mobile="100"
          james:ResponsiveVisibility.Mobile="Collapsed"/>
```

모든 플랫폼에서 쓸 수 있는 속성: `ResponsiveWidth`, `ResponsiveHeight`, `ResponsiveMinHeight`, `ResponsiveMargin`, `ResponsiveBorderThickness`, `ResponsiveVisibility`, `ResponsiveFontSize`, `ResponsiveFontWeight`, `ResponsiveContent`, `ResponsiveChild`, `ResponsiveAnimationTo`. `ResponsiveColumns`(`UniformGrid`용)는 WPF·OpenSilver 전용입니다. Uno·WinUI 3·UWP에서는 `xmlns:responsive="using:Jamesnet.Foundation.Responsive"`를 선언합니다.

## 샘플

같은 화면을 플랫폼별로 만든 데모입니다.

| 샘플 | 실행 |
|---|---|
| `samples/DemoWpfApp` | `dotnet run --project samples/DemoWpfApp` |
| `samples/DemoOpenSilverApp` | `dotnet run --project samples/DemoOpenSilverApp/DemoOpenSilverApp.Simulator` |
| `samples/DemoUnoApp` | `dotnet run --project samples/DemoUnoApp` |
| `samples/DemoWinUI3App` | `dotnet run --project samples/DemoWinUI3App` |

## 9.x에서 올라오기

- `Jamesnet.Platform.*` 패키지 참조를 모두 `Jamesnet.Foundation` 하나로 바꿉니다.
- `WPFLayer`, `OpenSilverLayer`, `UnoLayer`, `WinUI3Layer`, `UwpLayer` → `XamlLayer`, `AnimatedWPFLayer`, `AnimatedOpenSilverLayer` → `AnimatedXamlLayer`. 기존 이름도 동작하지만 `[Obsolete]`로 표시됩니다.
- XAML은 `clr-namespace:...;assembly=Jamesnet.Platform.*` 대신 `http://jamesnet.dev/xaml`(WPF·OpenSilver) 또는 `using:Jamesnet.Foundation`(Uno·WinUI 3·UWP)을 씁니다.
- 대상 프레임워크를 .NET 10으로 올립니다.

## 저장소 구조

```
src/        라이브러리 (공용 소스 Shared + 플랫폼별 flavor 프로젝트 + 패키지 프로젝트)
tests/      단위 테스트와 WPF UI 테스트
samples/    데모 앱
docs/       아키텍처와 로드맵
```

빌드·테스트: `dotnet test --project tests/Jamesnet.Foundation.Tests`. 방향성: [docs/DIRECTION.ko.md](docs/DIRECTION.ko.md), 설계 문서: [docs/ARCHITECTURE.ko.md](docs/ARCHITECTURE.ko.md), 로드맵: [docs/ROADMAP.ko.md](docs/ROADMAP.ko.md).
