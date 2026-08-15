# Jamesnet.Foundation 아키텍처와 방향성

## 한 줄 요약

**어떤 XAML 플랫폼을 쓰든 `Jamesnet.Foundation` 패키지 하나만 설치하면 된다.**

---

## 1. 우리가 풀려는 문제

Jamesnet.Foundation은 원래 코어 하나와 플랫폼별 패키지 다섯 개로 나뉘어 있었다.

```
Jamesnet.Foundation            (코어)
Jamesnet.Platform.WPF
Jamesnet.Platform.OpenSilver
Jamesnet.Platform.Uno
Jamesnet.Platform.WinUI3
Jamesnet.Platform.Uwp
```

이 분리는 의도한 설계가 아니었다. `WPFLayer`, `OpenSilverLayer`, `UnoLayer`, `WinUI3Layer`,
`UwpLayer`를 하나로 합칠 방법을 찾지 못해서 생긴 결과였다.

실제로 이 다섯 클래스를 나란히 놓고 보면 본문 로직은 한 글자도 다르지 않았다.
달랐던 것은 네 가지뿐이다.

| 차이 | 내용 |
|---|---|
| 클래스명·네임스페이스 | `WPFLayer` vs `UnoLayer` … |
| using | `System.Windows` vs `Microsoft.UI.Xaml` vs `Windows.UI.Xaml` |
| 부모 클래스 | OpenSilver만 `UserControl`, 나머지는 `ContentControl` |
| 세부사항 | 디자인 모드 판별 방식, `Content` 캐스팅 유무 |

즉 **코드가 다른 것이 아니라, 컴파일 대상 프레임워크가 달랐을 뿐**이다.

## 2. 왜 어려웠나

WPF의 `ContentControl`과 OpenSilver의 `ContentControl`은 이름만 같고 서로 다른
어셈블리의 다른 타입이다. `DependencyProperty`도 마찬가지다.

| 플랫폼 | XAML 타입이 사는 곳 |
|---|---|
| WPF | `System.Windows` (WindowsBase) |
| OpenSilver | `System.Windows` (OpenSilver.dll) |
| Uno / WinUI3 | `Microsoft.UI.Xaml` |
| UWP | `Windows.UI.Xaml` |

그래서 XAML 타입을 건드리는 코드는 **하나의 어셈블리로 컴파일할 수 없다.**
어셈블리는 플랫폼마다 따로 만들 수밖에 없다.

여기에 더해 NuGet은 패키지 안의 `lib/` 폴더 중 **정확히 하나만** 고른다.
OpenSilver 앱도 `net10.0`, Uno Desktop 앱도 `net10.0`이라 NuGet 입장에서는 이 둘을
구분할 방법이 없다. 표준 `lib/` 방식만 고집하면 둘 중 하나는 별도 패키지로
빠져야 한다.

## 3. 해결책

두 가지를 분리해서 생각하면 풀린다.

> **어셈블리는 플랫폼마다 따로 만든다. 하지만 소스는 한 벌이고, 패키지도 하나다.**

### 3-1. 소스는 한 벌 — `src/Shared/`

모든 코드는 `src/Shared/` 아래에 한 벌만 존재하고, 각 flavor 프로젝트가 이를
링크 컴파일한다. 플랫폼 차이는 `#if`로 흡수한다.

```
src/Shared/
    Core/          코어 (DI 컨테이너, MVVM, LayerManager, …)
    Xaml/          플랫폼 공통 XAML 코드 (XamlLayer, …)
```

```csharp
// src/Shared/Xaml/XamlLayer.cs — 이 파일 하나가 5개 플랫폼 전부를 담당한다
#if WINUI3 || UNO
using Microsoft.UI.Xaml;
#elif UWP
using Windows.UI.Xaml;
#else // WPF, OPENSILVER
using System.Windows;
#endif

namespace Jamesnet.Foundation;

public class XamlLayer :
#if OPENSILVER
    UserControl
#else
    ContentControl
#endif
    , ILayer
{ /* 본문 한 벌 */ }
```

각 flavor 프로젝트는 `<DefineConstants>`로 자기 심볼을 켜고 `Shared/**`를 컴파일한다.
그 결과 **모든 flavor가 어셈블리명과 네임스페이스가 똑같이 `Jamesnet.Foundation`인**
자기완결형 DLL을 만든다. 코어까지 각 DLL에 함께 담기므로 별도 코어 참조가 필요 없다.

### 3-2. 패키지는 하나 — flavor 선택을 MSBuild가 한다

NuGet의 TFM 해석으로는 OpenSilver와 Uno를 구분할 수 없다. 그래서 flavor 선택을
NuGet이 아니라 **MSBuild**에게 맡긴다. MSBuild 레벨에서는 각 플랫폼이 자기만의
속성을 켜기 때문에 완벽히 구분된다.

```
Jamesnet.Foundation.nupkg          ← 소비자가 받는 단 하나의 패키지
├── lib/netstandard2.0/_._          ← 호환성 표식만 (DLL 없음)
├── flavors/
│   ├── wpf/Jamesnet.Foundation.dll
│   ├── opensilver/Jamesnet.Foundation.dll
│   ├── winui3/Jamesnet.Foundation.dll
│   ├── uno/Jamesnet.Foundation.dll
│   └── uwp/Jamesnet.Foundation.dll
└── buildTransitive/Jamesnet.Foundation.targets   ← 여기서 flavor를 고른다
```

| 플랫폼 | 판별 근거 |
|---|---|
| UWP | `TargetPlatformIdentifier == 'UAP'` |
| WPF | `UseWPF == true` |
| OpenSilver | `OpenSilverType != ''` |
| WinUI3 | `UseWinUI == true` |
| Uno | `UnoSingleProject == true` |

Uno는 `UseWinUI`도 함께 켜므로, targets 파일에서 **Uno 판정이 WinUI3보다 뒤에 와서
이기도록** 순서가 잡혀 있다. 이 순서는 바꾸면 안 된다.

자동 판별이 실패하면 조용히 넘어가지 않고 `JAMESNET001` 오류로 즉시 알린다.
특이한 빌드 구성을 위해 소비자가 `<JamesnetFlavor>`를 직접 지정하는 탈출구도 있다.

## 4. 소비자가 보는 모습

플랫폼과 무관하게 완전히 동일하다.

```xml
<PackageReference Include="Jamesnet.Foundation" Version="10.0.0" />
```

```csharp
using Jamesnet.Foundation;

public class Shell : XamlLayer { }
public class MainViewModel : ViewModelBase { }
```

WPF 프로젝트에서든 OpenSilver 프로젝트에서든 **위 코드를 그대로 복사해서 쓸 수 있다.**
빌드 시점에 자기 플랫폼 DLL이 자동으로 선택된다.

## 5. 원칙

이 저장소가 지키는 규칙들이다.

1. **플랫폼은 폐기하지 않는다.** 지원 가능한 모든 XAML 플랫폼을 유지한다.
   `Jamesnet.Platform.AvaloniaUI`, `Jamesnet.Platform.MAUI`는 아직 자리만
   잡혀 있지만 같은 방식으로 합류할 예정이다.
2. **패키지는 하나다.** 새 플랫폼이 늘어도 `flavors/<name>/` 폴더와 targets 조건
   한 줄이 추가될 뿐, 소비자가 받는 패키지 개수는 계속 하나다.
3. **중복 구현을 만들지 않는다.** 플랫폼별 프로젝트에 같은 코드를 두 번 쓰게
   되거든, `Shared/`로 올리고 `#if`로 가르는 것이 맞는지 먼저 검토한다.
4. **버전은 한 곳에서.** `src/Directory.Build.props`의 `JamesnetVersion` 하나만
   고친다.
5. **누락을 조용히 넘기지 않는다.** 일부 flavor만 담긴 패키지가 만들어지면
   `JAMESNETPACK001` 경고로 반드시 드러난다.

## 6. 빌드와 배포

```bash
# 개발 중 (UWP 제외 — dotnet CLI로는 UWP를 빌드할 수 없다)
dotnet pack src/Jamesnet.Foundation.Package -c Release

# 배포용 (전체 flavor). Visual Studio MSBuild + UWP 워크로드가 필요하다.
dotnet pack src/Jamesnet.Foundation.Package -c Release \
  -p:JamesnetFlavors="wpf;opensilver;winui3;uno;uwp"
```

UWP는 레거시 non-SDK 프로젝트라 `dotnet build`로 빌드할 수 없다. 이는 도구의
제약일 뿐 폐기된 것이 아니며, CI(`windows-latest`)에는 Visual Studio가 있으므로
배포 패키지에는 UWP flavor가 정상적으로 포함된다.

## 7. 기준 버전

| 항목 | 버전 |
|---|---|
| .NET | 10.0 |
| OpenSilver | 3.3.3 |
| Uno.Sdk | 6.6.42 |
| WindowsAppSDK (WinUI3) | 2.4.0 |
| Windows SDK BuildTools | 10.0.28000.2526 |
| UWP (UniversalWindowsPlatform) | 6.2.15 |

## 8. 9.x에서 올라올 때

- 패키지 참조를 `Jamesnet.Platform.*`에서 **`Jamesnet.Foundation` 하나로** 바꾼다.
- `WPFLayer`, `OpenSilverLayer`, `UnoLayer`, `WinUI3Layer`, `UwpLayer`는
  `XamlLayer`로 대체한다. 기존 이름은 `[Obsolete]` 별칭으로 남아 있어 당장은
  동작하지만, 다음 메이저 버전에서 제거된다.
- XAML의 `assembly=Jamesnet.Platform.Wpf` 같은 선언은
  `assembly=Jamesnet.Foundation`으로 바꾼다.
- 타겟 프레임워크를 .NET 10으로 올린다.

## 9. 남은 일

- `Responsive` 계열이 아직 플랫폼별로 흩어져 있다
  (OpenSilver 11종 / WPF 5종 / Uno·WinUI3 각 1종 / UWP 0종).
  `Shared/`로 올리면 모든 플랫폼이 동시에 전체 기능을 갖게 된다.
- `RecursiveControl`, `CustomWrapPanel`도 플랫폼별 중복이 남아 있다.
- AvaloniaUI, MAUI flavor 추가.
