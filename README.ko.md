# Jamesnet.Foundation

`Jamesnet.Foundation`은 다양한 플랫폼에서 공통적으로 사용할 수 있는 기반 라이브러리입니다. 이 패키지는 여러 플랫폼별 프로젝트에서 핵심 기능을 제공하며, 아래 나열된 플랫폼별 패키지들이 모두 이 `Jamesnet.Foundation`을 참조하고 있습니다.

[![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Foundation.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Foundation/)

## 지원 플랫폼
- [Jamesnet.Platform.OpenSilver](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/)
- [Jamesnet.Platform.Wpf](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)
- [Jamesnet.Platform.Uno](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)
- [Jamesnet.Platform.Uwp](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)
- [Jamesnet.Platform.WinUI3](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)

| 플랫폼                         | Version                                                                                                                                           | Downloads                                                                                                                                           |
|--------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Jamesnet.Platform.OpenSilver** | [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/) | [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.OpenSilver.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.OpenSilver/) |
| **Jamesnet.Platform.Wpf**         | [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)             | [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Wpf.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Wpf/)             |
| **Jamesnet.Platform.Uno**         | [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)             | [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uno.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uno/)             |
| **Jamesnet.Platform.Uwp**         | [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)             | [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.Uwp.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.Uwp/)             |
| **Jamesnet.Platform.WinUI3**      | [![NuGet Version](https://img.shields.io/nuget/v/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)      | [![NuGet Downloads](https://img.shields.io/nuget/dt/Jamesnet.Platform.WinUI3.svg?style=flat-square)](https://www.nuget.org/packages/Jamesnet.Platform.WinUI3/)      |

## 설치 방법
NuGet 패키지 매니저를 통해 설치하세요:

    Install-Package Jamesnet.Foundation

WPF 프로젝트라면 추가로:

    Install-Package Jamesnet.Platform.Wpf

## 사용 방법
`Jamesnet.Foundation`은 `AppBootstrapper` 클래스를 통해 애플리케이션 초기화를 지원합니다. 이 클래스를 상속받아 구현한 뒤, 인스턴스를 생성하고 `.Run()`을 호출하면 됩니다. `.Run()`은 `MainWindow`가 생성되기 전에 호출되어 의존성 등록, 뷰모델 매핑, 레이어 설정 등을 처리합니다.

### 예제: WPF에서 사용하기
WPF에서 `AppBootstrapper`를 활용한 예제입니다:

    using Jamesnet.Foundation;
    using Jamesnet.Platform.Wpf; // WPF 플랫폼 패키지 참조

    namespace MyApp
    {
        public class MyAppBootstrapper : AppBootstrapper
        {
            protected override void ConfigureContainer()
            {
                base.ConfigureContainer();
                Container.RegisterSingleton<IMyService, MyService>();
            }

            protected override void RegisterViewModels()
            {
                ViewModelMapper.Map<MyViewModel, MyView>();
            }

            protected override void RegisterDependencies(IContainer container)
            {
                container.RegisterSingleton<IWindowManager, WpfWindowManager>();
            }

            protected override void SettingsLayer(ILayerManager layer, IContainer container)
            {
                var windowManager = container.Resolve<IWindowManager>();
                layer.SetMainWindow(windowManager.CreateMainWindow<MyViewModel>());
            }
        }

        public partial class App : Application
        {
            protected override void OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);
                var bootstrapper = new MyAppBootstrapper();
                bootstrapper.Run(); // MainWindow 생성 전에 초기화
            }
        }
    }

#### 동작 흐름
1. `ConfigureContainer`: 커스텀 의존성 등록.
2. `RegisterViewModels`: 뷰모델과 뷰 매핑.
3. `RegisterDependencies`: 플랫폼별 의존성 등록.
4. `SettingsLayer`: `MainWindow`와 같은 레이어 설정.
5. `Run`: `App` 클래스에서 호출해 애플리케이션 시작.

다른 플랫폼(UWP, WinUI3, Uno 등)에서도 비슷한 방식으로 `AppBootstrapper`를 상속받아 초기화하면 됩니다.

## 기여
버그 리포트나 기능 제안은 [이슈 트래커](https://github.com/username/Jamesnet.Foundation/issues)에 남겨주세요. 풀 리퀘스트도 환영합니다!

## 라이선스
이 프로젝트는 [MIT 라이선스](LICENSE) 하에 배포됩니다。

