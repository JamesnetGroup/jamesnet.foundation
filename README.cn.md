# Jamesnet.Foundation

[English](README.md) | [한국어](README.ko.md) | [中文](README.cn.md)

`Jamesnet.Foundation` 是一个基于 XAML 的跨平台 MVVM 库。**一个软件包支持所有平台**——只需安装 `Jamesnet.Foundation`,构建时会自动选择与当前平台匹配的程序集。

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **v10 — 单一软件包。** 原来的 `Jamesnet.Platform.*` 软件包已合并为 `Jamesnet.Foundation`。请参阅[从 9.x 升级](#从-9x-升级)。

## 支持的平台

| 平台 | 基准版本 |
|---|---|
| WPF | .NET 10 |
| OpenSilver | 3.3.3 |
| Uno Platform | Uno.Sdk 6.7.30 |
| WinUI 3 | Windows App SDK 2.6.4 |
| UWP | UniversalWindowsPlatform 6.2.15 |

## 安装

```powershell
dotnet add package Jamesnet.Foundation
```

无需再添加任何平台专用软件包。

## 使用方法

### 1. 引导程序

在应用启动时注册视图、视图模型和初始界面。

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

应用启动时调用 `new MyAppBootstrapper().Run();`(WPF 在 `App` 构造函数中,Uno / WinUI 3 在 `OnLaunched` 中)。

### 2. XAML 中的图层

图层是一个带名称的占位区域。加载后会自动注册,并显示映射到该名称的视图。

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

`XamlLayer` 立即切换内容,`AnimatedXamlLayer` 以淡入淡出方式切换。

### 3. 视图模型

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

    [CommandTrigger(nameof(SaveCommand))] // Name 变化时重新计算 SaveCommand.CanExecute
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public RelayCommand SaveCommand { get; }

    public void OnLoaded(object view) { }

    private void Save() => _layers.Show("Content", _container.Resolve<IView>("DetailContent"));
}
```

视图类需实现 `IView`(`public partial class MainContent : UserControl, IView`)。从容器解析视图时,会自动创建映射的视图模型并设置为 `DataContext`。

### 4. 响应式属性

根据窗口宽度切换值(默认断点 768):

```xml
<ListView james:ResponsiveWidth.Desktop="200"
          james:ResponsiveWidth.Mobile="100"
          james:ResponsiveVisibility.Mobile="Collapsed"/>
```

所有平台可用:`ResponsiveWidth`、`ResponsiveHeight`、`ResponsiveMinHeight`、`ResponsiveMargin`、`ResponsiveBorderThickness`、`ResponsiveVisibility`、`ResponsiveFontSize`、`ResponsiveFontWeight`、`ResponsiveContent`、`ResponsiveChild`、`ResponsiveAnimationTo`。`ResponsiveColumns`(用于 `UniformGrid`)仅限 WPF / OpenSilver。在 Uno、WinUI 3、UWP 中请声明 `xmlns:responsive="using:Jamesnet.Foundation.Responsive"`。

## 示例

同一界面在不同平台上的演示。

| 示例 | 运行 |
|---|---|
| `samples/DemoWpfApp` | `dotnet run --project samples/DemoWpfApp` |
| `samples/DemoOpenSilverApp` | `dotnet run --project samples/DemoOpenSilverApp/DemoOpenSilverApp.Simulator` |
| `samples/DemoUnoApp` | `dotnet run --project samples/DemoUnoApp` |
| `samples/DemoWinUI3App` | `dotnet run --project samples/DemoWinUI3App` |

## 从 9.x 升级

- 将所有 `Jamesnet.Platform.*` 软件包引用替换为 `Jamesnet.Foundation`。
- `WPFLayer`、`OpenSilverLayer`、`UnoLayer`、`WinUI3Layer`、`UwpLayer` → `XamlLayer`;`AnimatedWPFLayer`、`AnimatedOpenSilverLayer` → `AnimatedXamlLayer`。旧名称仍可使用,但已标记为 `[Obsolete]`。
- XAML 中使用 `http://jamesnet.dev/xaml`(WPF / OpenSilver)或 `using:Jamesnet.Foundation`(Uno / WinUI 3 / UWP),替代 `clr-namespace:...;assembly=Jamesnet.Platform.*`。
- 目标框架升级到 .NET 10。

## 仓库结构

```
src/        库(共享源码 Shared + 各平台 flavor 项目 + 打包项目)
tests/      单元测试与 WPF UI 测试
samples/    演示应用
docs/       架构与路线图(韩文)
```

构建与测试:`dotnet test --project tests/Jamesnet.Foundation.Tests`。设计文档:[docs/ARCHITECTURE.ko.md](docs/ARCHITECTURE.ko.md)。
