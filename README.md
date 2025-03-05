# Jamesnet.Foundation

`Jamesnet.Foundation` is a core library that provides common functionality across various platforms. It is referenced by several platform-specific projects, which utilize its essential features.

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **Note:** NuGet publish date information is not available, so it is not included.

## Supported Platforms

- **[Jamesnet.Platform.OpenSilver](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)
- **[Jamesnet.Platform.Wpf](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)
- **[Jamesnet.Platform.Uno](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)
- **[Jamesnet.Platform.Uwp](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)
- **[Jamesnet.Platform.WinUI3](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)

## Installation

Install the package using the NuGet Package Manager:

```powershell
Install-Package Jamesnet.Foundation
```

For WPF projects, you should also install the following package:

```powershell
Install-Package Jamesnet.Platform.Wpf
```

## Usage

`Jamesnet.Foundation` supports application initialization through the `AppBootstrapper` class. The following example demonstrates how to extend `AppBootstrapper` in a WPF project for initialization.

> **Note:** There is no need to explain the `virtual` keyword or the old `SetMainWindow` approach.

### Example: Using AppBootstrapper in a WPF Project

```csharp
using Jamesnet.Foundation;
using Jamesnet.Platform.Wpf; // WPF platform package reference

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
