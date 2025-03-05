# Jamesnet.Foundation

`Jamesnet.Foundation`은 다양한 플랫폼에서 공통적으로 사용할 수 있는 기반 라이브러리입니다. 여러 플랫폼별 프로젝트에서 핵심 기능을 제공하며, 아래에 나열된 각 플랫폼 패키지가 모두 이 `Jamesnet.Foundation`을 참조하고 있습니다.

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

> **참고:** NuGet 날짜 정보는 제공되지 않으므로 포함되지 않았습니다.

## 지원 플랫폼

- **[Jamesnet.Platform.OpenSilver](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)**  [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)

- **[Jamesnet.Platform.Wpf](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)**  [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)

- **[Jamesnet.Platform.Uno](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)**  [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)

- **[Jamesnet.Platform.Uwp](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)**  [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)

- **[Jamesnet.Platform.WinUI3](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)**  [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)  [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)

## 설치 방법

NuGet 패키지 매니저를 사용하여 설치하세요:

```powershell
Install-Package Jamesnet.Foundation
```

WPF 프로젝트의 경우, 추가로 다음 패키지를 설치합니다:

```powershell
Install-Package Jamesnet.Platform.Wpf
```

## 사용 방법

`Jamesnet.Foundation`은 `AppBootstrapper` 클래스를 통해 애플리케이션 초기화를 지원합니다. 아래 예시는 WPF 프로젝트에서 `AppBootstrapper`를 상속받아 초기화를 진행하는 방법을 보여줍니다.

> **참고:** `virtual` 키워드나 기존의 `SetMainWindow` 방식은 언급하지 않습니다.

### 예제: WPF에서 사용하기

```csharp
using Jamesnet.Foundation;
using Jamesnet.Platform.Wpf; // WPF 플랫폼 패키지 참조

namespace MyApp
{
    public class MyAppBootstrapper : AppBootstrapper
    {
        protected override void RegisterViewModels()
        {
            // View와 ViewModel을 매핑합니다.
            ViewModelMapper.Register<MainContent, MainViewModel>();
        }

        protected override void RegisterDependencies(IContainer container)
        {
            Container.RegisterSingleton<IView, MainContent>("MainContent");
        }

        protected override void SettingsLayer(ILayerManager layer, IContainer container)
        {
            // "Main" 키로 Mapping을 수행합니다.
            Mapping("Main", container.Resolve<IView>("MainContent"));
        }
    }

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var bootstrapper = new MyAppBootstrapper();
            bootstrapper.Run(); // 애플리케이션 초기화 실행
        }
    }
}
```

위 예제에서는 다음과 같이 구성됩니다:

- **의존성 등록:**  `Container.RegisterSingleton<IView, MainContent>("MainContent");`를 통해 `MainContent`를 IView 인터페이스로 등록합니다.

- **뷰-뷰모델 매핑:**  `ViewModelMapper.Register<MainContent, MainViewModel>();`를 통해 `MainContent`와 `MainViewModel`을 매핑합니다.

- **레이어 설정:**  `Mapping("Main", container.Resolve<IView>("MainContent"));`를 통해 "Main" 키로 해당 뷰를 Mapping 합니다.
