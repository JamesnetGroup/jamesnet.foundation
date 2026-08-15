# Jamesnet.Foundation

`Jamesnet.Foundation` is a XAML-based cross-platform library. **One package covers every platform** — install `Jamesnet.Foundation` and the right assembly for your platform is selected automatically at build time.

> **v10.0 — Single Package.** The former `Jamesnet.Platform.*` packages have been merged into `Jamesnet.Foundation`. See [docs/ARCHITECTURE.ko.md](docs/ARCHITECTURE.ko.md) for the design and the migration guide.

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **Note:** NuGet publish date information is not available, so it is not included.

## Supported Platforms

All of these are served by the single `Jamesnet.Foundation` package.

| Platform | Baseline |
|---|---|
| WPF | .NET 10 |
| OpenSilver | 3.3.3 |
| Uno Platform | Uno.Sdk 6.6.42 |
| WinUI 3 | Windows App SDK 2.4.0 |
| UWP | UniversalWindowsPlatform 6.2.15 |

## Installation

One package, whichever platform you target:

```powershell
Install-Package Jamesnet.Foundation
```

That's it — no platform-specific package to add.

## Usage

`Jamesnet.Foundation` supports application initialization through the `AppBootstrapper` class. The following example demonstrates how to extend `AppBootstrapper` in a WPF project for initialization.

> **Note:** There is no need to explain the `virtual` keyword or the old `SetMainWindow` approach.

### Example: Using AppBootstrapper in a WPF Project

```csharp
using Jamesnet.Foundation; // this single using covers every platform

namespace MyApp
{
    public class MyAppBootstrapper : AppBootstrapper
    {
        protected override void RegisterViewModels()
        {
            // Map the view to the view model.
            ViewModelMapper.Register<MainContent, MainViewModel>();
        }

        protected override void RegisterDependencies(IContainer container)
        {
            // Register MainContent as the IView implementation with the key "MainContent".
            Container.RegisterSingleton<IView, MainContent>("MainContent");
        }

        protected override void SettingsLayer(ILayerManager layer, IContainer container)
        {
            // Perform mapping using the "Main" key.
            Mapping("Main", container.Resolve<IView>("MainContent"));
        }
    }

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new MyAppBootstrapper();
            bootstrapper.Run(); // Execute application initialization
        }
    }
}
```

In this example:

- **Dependency Registration:**    `Container.RegisterSingleton<IView, MainContent>("MainContent");` registers `MainContent` as an implementation of the `IView` interface with the key `"MainContent"`.
- **View-ViewModel Mapping:**    `ViewModelMapper.Register<MainContent, MainViewModel>();` maps the `MainContent` view to the `MainViewModel`.
- **Layer Settings:**    `Mapping("Main", container.Resolve<IView>("MainContent"));` sets up the mapping for the "Main" layer using the registered view.
