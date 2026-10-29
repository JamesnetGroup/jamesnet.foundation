# Jamesnet.Foundation

[English](README.md) | [한국어](README.ko.md) | [中文](README.cn.md)

`Jamesnet.Foundation` is a XAML-based cross-platform MVVM library. **One package covers every platform** — install `Jamesnet.Foundation` and the right assembly for your platform is selected automatically at build time.

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **v10 — single package.** The former `Jamesnet.Platform.*` packages are merged into `Jamesnet.Foundation`. See [Upgrading from 9.x](#upgrading-from-9x).

## Supported platforms

| Platform | Baseline |
|---|---|
| WPF | .NET 10 |
| OpenSilver | 3.3.3 |
| Uno Platform | Uno.Sdk 6.7.30 |
| WinUI 3 | Windows App SDK 2.6.4 |
| UWP | UniversalWindowsPlatform 6.2.15 |

## Installation

```powershell
dotnet add package Jamesnet.Foundation
```

No platform-specific package is needed.

## Usage

### 1. Bootstrapper

Register views, view models and the initial layer content once at startup.

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

Call `new MyAppBootstrapper().Run();` when the app starts (WPF: `App` constructor, Uno/WinUI 3: `OnLaunched`).

### 2. Layers in XAML

A layer is a named placeholder. When it loads, it registers itself and shows the view mapped to its name.

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

`XamlLayer` swaps content instantly; `AnimatedXamlLayer` cross-fades.

### 3. View models

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

    [CommandTrigger(nameof(SaveCommand))] // re-evaluates SaveCommand.CanExecute when Name changes
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    public RelayCommand SaveCommand { get; }

    public void OnLoaded(object view) { }

    private void Save() => _layers.Show("Content", _container.Resolve<IView>("DetailContent"));
}
```

Views add `IView` to their class (`public partial class MainContent : UserControl, IView`). When a view is resolved from the container, its mapped view model is created and set as `DataContext` automatically.

### 4. Responsive properties

Switch values by window width (breakpoint 768 by default):

```xml
<ListView james:ResponsiveWidth.Desktop="200"
          james:ResponsiveWidth.Mobile="100"
          james:ResponsiveVisibility.Mobile="Collapsed"/>
```

Available on every platform: `ResponsiveWidth`, `ResponsiveHeight`, `ResponsiveMinHeight`, `ResponsiveMargin`, `ResponsiveBorderThickness`, `ResponsiveVisibility`, `ResponsiveFontSize`, `ResponsiveFontWeight`, `ResponsiveContent`, `ResponsiveChild`, `ResponsiveAnimationTo`. `ResponsiveColumns` (for `UniformGrid`) is WPF/OpenSilver only. On Uno, WinUI 3 and UWP, declare `xmlns:responsive="using:Jamesnet.Foundation.Responsive"`.

## Samples

Each sample shows the same screen on a different platform.

| Sample | Run |
|---|---|
| `samples/DemoWpfApp` | `dotnet run --project samples/DemoWpfApp` |
| `samples/DemoOpenSilverApp` | `dotnet run --project samples/DemoOpenSilverApp/DemoOpenSilverApp.Simulator` |
| `samples/DemoUnoApp` | `dotnet run --project samples/DemoUnoApp` |
| `samples/DemoWinUI3App` | `dotnet run --project samples/DemoWinUI3App` |

## Upgrading from 9.x

- Replace every `Jamesnet.Platform.*` package reference with `Jamesnet.Foundation`.
- `WPFLayer`, `OpenSilverLayer`, `UnoLayer`, `WinUI3Layer`, `UwpLayer` → `XamlLayer`; `AnimatedWPFLayer`, `AnimatedOpenSilverLayer` → `AnimatedXamlLayer`. The old names still work but are marked `[Obsolete]`.
- XAML: use `http://jamesnet.dev/xaml` (WPF/OpenSilver) or `using:Jamesnet.Foundation` (Uno/WinUI 3/UWP) instead of `clr-namespace:...;assembly=Jamesnet.Platform.*`.
- Target .NET 10.

## Repository layout

```
src/        library (Shared source + one flavor project per platform + the package project)
tests/      unit and WPF UI tests
samples/    demo apps
docs/       architecture and roadmap (Korean)
```

Build and test: `dotnet test --project tests/Jamesnet.Foundation.Tests`. Design notes: [docs/ARCHITECTURE.ko.md](docs/ARCHITECTURE.ko.md).
