# Jamesnet.Foundation

XAML-based cross-platform MVVM library. **One package for WPF, OpenSilver, Uno Platform, WinUI 3 and UWP** — the right assembly for your project is selected automatically at build time.

```powershell
dotnet add package Jamesnet.Foundation
```

- `AppBootstrapper`, a small DI `Container`, `ViewModelBase`, `RelayCommand`, `[CommandTrigger]`
- Named layers (`XamlLayer`, `AnimatedXamlLayer`) managed by `ILayerManager`
- Responsive attached properties (`ResponsiveWidth`, `ResponsiveVisibility`, `ResponsiveFontSize`, …) that switch values by window width

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

Upgrading from 9.x: replace every `Jamesnet.Platform.*` package with this one. Old type names such as `WPFLayer` keep working as `[Obsolete]` aliases of `XamlLayer`.

Documentation and samples: https://github.com/jamesnetgroup/jamesnet.foundation
