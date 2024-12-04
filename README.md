# **Jamesnet.Foundation**

[![NuGet](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg)](https://www.nuget.org/packages/Jamesnet.Foundation)  
[![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg)](https://www.nuget.org/packages/Jamesnet.Foundation)  
[![License](https://img.shields.io/github/license/JamesnetGroup/jamesnet.foundation)](https://opensource.org/licenses/MIT)  
[![Build Status](https://img.shields.io/github/actions/workflow/status/JamesnetGroup/jamesnet.foundation/build.yml?branch=main)](https://github.com/JamesnetGroup/jamesnet.foundation/actions)  
[![Issues](https://img.shields.io/github/issues/JamesnetGroup/jamesnet.foundation)](https://github.com/JamesnetGroup/jamesnet.foundation/issues)  
[![Stars](https://img.shields.io/github/stars/JamesnetGroup/jamesnet.foundation?style=social)](https://github.com/JamesnetGroup/jamesnet.foundation)

---

`Jamesnet.Foundation` is a core framework designed to simplify the development of **XAML-based applications** across multiple platforms, including **WPF**, **OpenSilver**, **Uno**, **WinUI 3**, and **UWP**. By focusing on **dependency injection**, **view injection**, and **modular architecture**, it enables developers to build scalable and maintainable applications with ease.

---

## **Features**

- **Centralized Architecture**: Unified framework for managing dependency injection, view injection, and interface-based design.
- **Platform-Specific Extensions**: Lightweight extensions tailored for each platform, enabling seamless integration and customization.
- **Scalability**: Built to handle enterprise-level applications with a focus on maintainability.

---

## **Installation**

Install the core foundation package:

```bash
dotnet add package Jamesnet.Foundation
```

Add platform-specific extensions as needed:

```bash
dotnet add package Jamesnet.Platform.WPF
dotnet add package Jamesnet.Platform.OpenSilver
dotnet add package Jamesnet.Platform.Uno
dotnet add package Jamesnet.Platform.WinUI3
dotnet add package Jamesnet.Platform.UWP
```

---

## **Quick Start**

### 1. **Set Up Dependency Injection**

Initialize `Jamesnet.Foundation` in your platform-specific project:

```csharp
var services = new ServiceCollection();
services.AddFoundation(); // Core Foundation setup
services.AddPlatformSpecific(); // Add platform-specific services
```

### 2. **Register Views and ViewModels**

Register views and viewmodels for dependency injection:

```csharp
services.AddTransient<IMainView, MainView>();
services.AddTransient<IMainViewModel, MainViewModel>();
```

### 3. **Launch a View**

Inject and display views dynamically:

```csharp
var mainView = serviceProvider.GetRequiredService<IMainView>();
mainView.Show();
```

---

## **Platform Extensions**

### **WPF: The Core Desktop Platform**
The cornerstone of `Jamesnet.Foundation`. WPF remains a powerful and widely adopted desktop application framework, making it a central focus of this project.

### **OpenSilver: Web-Based XAML Applications**
`OpenSilver` is a robust, web-based XAML platform designed as a modern migration path for **Silverlight** and **WPF** applications. It builds on the legacy of `CSHTML5` to deliver high-performance, web-ready applications, leveraging XAML and C# for seamless integration with modern web technologies.

### **Uno: Multi-Platform Support**
`Uno Platform` enables the creation of native applications across **Windows**, **macOS**, **Linux**, **iOS**, **Android**, and **WebAssembly** using a single codebase. It is ideal for targeting diverse platforms with consistent UI and shared logic.

### **WinUI 3: Modern Windows Applications**
`WinUI 3` represents the next generation of Windows application development, building upon the foundation of **UWP** with modern APIs, improved performance, and more flexibility. It is the go-to choice for creating desktop applications within the Windows ecosystem.

### **UWP: Universal Windows Platform**
`UWP` enables developers to build apps for a wide range of Windows devices. While its functionality is foundational to **WinUI 3**, it remains a viable option for developing modern Windows Store applications.

---

## **Roadmap**

- Expand support for additional platforms like **MAUI** and **Avalonia**.
- Introduce advanced UI components optimized for each platform.
- Enhance documentation with real-world use cases and advanced examples.

---

## **Contributing**

We welcome contributions! Please check the [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

1. Fork this repository.
2. Create a feature branch: `git checkout -b feature/new-feature`.
3. Commit your changes: `git commit -m "Add new feature"`.
4. Push to the branch: `git push origin feature/new-feature`.
5. Submit a pull request.

---

## **License**

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## **Contact**

For questions or support, please open a [GitHub Issue](https://github.com/JamesnetGroup/jamesnet.foundation/issues).

---

### **Key Updates**
1. `UWP` is included while emphasizing its foundational role in `WinUI 3` development.
2. `OpenSilver` and `Uno` platforms are described with a focus on their distinct use cases.
3. Badges are fully updated to match the `Jamesnet.Foundation` repository.  

Feel free to share additional feedback! 😊
