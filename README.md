# Jamesnet.Foundation

[![NuGet](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg)](https://www.nuget.org/packages/Jamesnet.Foundation)  
[![License](https://img.shields.io/github/license/JamesnetGroup/jamesnet.foundation)](https://opensource.org/licenses/MIT)

**Jamesnet.Foundation** is a core framework built on **.NET Standard 2.0** that simplifies the development of XAML-based applications. It offers support for dependency injection, view injection, and modular architecture. The framework is available as a NuGet package and can be extended with the following platform-specific packages:

## Platform Extensions

- **Jamesnet.Platform.OpenSilver**
- **Jamesnet.Platform.Wpf**
- **Jamesnet.Platform.Uno**
- **Jamesnet.Platform.WinUI3**
- **Jamesnet.Platform.Uwp**

## Installation

### Install the Core Foundation

```bash
dotnet add package Jamesnet.Foundation
```

### Install Platform Extensions

```bash
dotnet add package Jamesnet.Platform.OpenSilver
dotnet add package Jamesnet.Platform.Wpf
dotnet add package Jamesnet.Platform.Uno
dotnet add package Jamesnet.Platform.WinUI3
dotnet add package Jamesnet.Platform.Uwp
```

## Quick Start

1. **Initialize Dependency Injection**

   ```csharp
   var services = new ServiceCollection();
   services.AddFoundation();         // Core framework setup
   services.AddPlatformSpecific();   // Register platform-specific services
   ```

2. **Register Views and ViewModels**

   ```csharp
   services.AddTransient<IMainView, MainView>();
   services.AddTransient<IMainViewModel, MainViewModel>();
   ```

3. **Launch a View**

   ```csharp
   var mainView = serviceProvider.GetRequiredService<IMainView>();
   mainView.Show();
   ```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact and Contribution

For questions or issues, please open a [GitHub Issue](https://github.com/JamesnetGroup/jamesnet.foundation/issues).
