# Jamesnet.Foundation

[![NuGet](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg)](https://www.nuget.org/packages/Jamesnet.Foundation)  
[![License](https://img.shields.io/github/license/JamesnetGroup/jamesnet.foundation)](https://opensource.org/licenses/MIT)

**Jamesnet.Foundation** 是基于 **.NET Standard 2.0** 的核心框架，旨在简化基于 XAML 的应用程序开发。它支持依赖注入、视图注入以及模块化架构，并以 NuGet 包形式发布。此框架可通过以下平台专用扩展包进行扩展：

## 平台扩展包

- **Jamesnet.Platform.OpenSilver**
- **Jamesnet.Platform.Wpf**
- **Jamesnet.Platform.Uno**
- **Jamesnet.Platform.WinUI3**
- **Jamesnet.Platform.Uwp**

## 安装

### 安装核心 Foundation

```bash
dotnet add package Jamesnet.Foundation
```

### 安装平台扩展包

```bash
dotnet add package Jamesnet.Platform.OpenSilver
dotnet add package Jamesnet.Platform.Wpf
dotnet add package Jamesnet.Platform.Uno
dotnet add package Jamesnet.Platform.WinUI3
dotnet add package Jamesnet.Platform.Uwp
```

## 快速开始

1. **初始化依赖注入**

   ```csharp
   var services = new ServiceCollection();
   services.AddFoundation();         // 设置核心框架
   services.AddPlatformSpecific();   // 注册平台相关服务
   ```

2. **注册视图和视图模型**

   ```csharp
   services.AddTransient<IMainView, MainView>();
   services.AddTransient<IMainViewModel, MainViewModel>();
   ```

3. **启动视图**

   ```csharp
   var mainView = serviceProvider.GetRequiredService<IMainView>();
   mainView.Show();
   ```

## 许可证

本项目采用 MIT 许可证，详情请参阅 [LICENSE](LICENSE) 文件。

## 联系与贡献

如有疑问或建议，请在 [GitHub Issues](https://github.com/JamesnetGroup/jamesnet.foundation/issues) 中提交问题。
