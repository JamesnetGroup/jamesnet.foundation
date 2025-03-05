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

---

**中文版本**

# Jamesnet.Foundation

`Jamesnet.Foundation` 是一个提供跨多个平台通用功能的核心库。多个平台特定项目都引用了该库，并利用其基本功能。

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **注意：** NuGet 发布日期信息不可用，因此未包含该信息。

## 支持平台

- **[Jamesnet.Platform.OpenSilver](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)
- **[Jamesnet.Platform.Wpf](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)
- **[Jamesnet.Platform.Uno](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)
- **[Jamesnet.Platform.Uwp](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)
- **[Jamesnet.Platform.WinUI3](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)**    [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)    [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)

## 安装方法

使用 NuGet 包管理器安装该软件包：

```powershell
Install-Package Jamesnet.Foundation
```

对于 WPF 项目，还需安装以下软件包：

```powershell
Install-Package Jamesnet.Platform.Wpf
```

## 使用方法

`Jamesnet.Foundation` 通过 `AppBootstrapper` 类支持应用程序初始化。下面的示例展示了如何在 WPF 项目中扩展 `AppBootstrapper` 进行初始化。

> **注意：** 无需解释 `virtual` 关键字或旧的 `SetMainWindow` 方法。

### 示例：在 WPF 项目中使用 AppBootstrapper

```csharp
using Jamesnet.Foundation;
using Jamesnet.Platform.Wpf; // 引用 WPF 平台软件包

namespace MyApp
{
    public class MyAppBootstrapper : AppBootstrapper
    {
        protected override void RegisterViewModels()
        {
            // 映射视图与视图模型。
            ViewModelMapper.Register<MainContent, MainViewModel>();
        }

        protected override void RegisterDependencies(IContainer container)
        {
            // 将 MainContent 以键 "MainContent" 注册为 IView 接口的实现。
            Container.RegisterSingleton<IView, MainContent>("MainContent");
        }

        protected override void SettingsLayer(ILayerManager layer, IContainer container)
        {
            // 使用 "Main" 键进行映射设置。
            Mapping("Main", container.Resolve<IView>("MainContent"));
        }
    }

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new MyAppBootstrapper();
            bootstrapper.Run(); // 执行应用程序初始化
        }
    }
}
```

在此示例中：

- **依赖注册：**    `Container.RegisterSingleton<IView, MainContent>("MainContent");` 将 `MainContent` 作为 `IView` 接口的实现注册，并使用键 `"MainContent"`。
- **视图-视图模型映射：**    `ViewModelMapper.Register<MainContent, MainViewModel>();` 将 `MainContent` 视图映射到 `MainViewModel`。
- **图层设置：**    `Mapping("Main", container.Resolve<IView>("MainContent"));` 使用注册的视图通过键 `"Main"` 进行映射设置。
