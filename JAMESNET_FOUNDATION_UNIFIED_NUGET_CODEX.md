# Jamesnet.Foundation 단일 NuGet 통합 설계

> 작성: Codex (OpenAI)  
> 작성일: 2026-07-26  
> 대상 저장소: `jamesnet.foundation`  
> 목표 플랫폼: WPF, OpenSilver, WinUI 3, UWP

## 1. 문서의 결론

`Jamesnet.Foundation`이라는 **단일 NuGet 패키지**로 WPF, OpenSilver, WinUI 3, UWP를 모두 지원하는 것은 기술적으로 가능하다.

최종 사용자는 플랫폼별 패키지를 따로 선택하지 않고 다음 참조 하나만 추가한다.

```xml
<PackageReference Include="Jamesnet.Foundation" Version="10.0.0" />
```

그러나 여기서 말하는 "하나"는 다음과 같이 정의해야 한다.

| 항목 | 통합 여부 | 설명 |
|---|---:|---|
| NuGet 패키지 ID | 통합 | `Jamesnet.Foundation` 하나 |
| 설치 방법 | 통합 | `PackageReference` 하나 |
| 버전과 릴리스 | 통합 | 모든 플랫폼이 동일 버전 |
| 공개 API와 사용 방식 | 통합 가능 | 공통 계약과 명명 규칙 유지 |
| 소스 프로젝트 | 분리 유지 | 플랫폼 SDK와 XAML 컴파일러가 다름 |
| 빌드 결과 DLL | 플랫폼별 | 플랫폼마다 참조하는 UI 어셈블리가 다름 |

즉, **제품과 사용자 경험은 하나로 통합하되 내부 구현과 빌드는 플랫폼 경계에 맞게 분리한다.**

모든 플랫폼 구현을 물리적으로 동일한 DLL 하나로 만드는 것은 이 계획의 목표가 아니다. WPF의 `System.Windows`, OpenSilver의 `System.Windows`, WinUI 3의 `Microsoft.UI.Xaml`, UWP의 `Windows.UI.Xaml`은 서로 다른 어셈블리와 런타임을 사용하기 때문이다.

## 2. 통합하려는 이유

현재 사용자는 공통 패키지와 플랫폼 패키지를 함께 설치해야 한다.

```xml
<!-- WPF 예 -->
<PackageReference Include="Jamesnet.Foundation" Version="9.x.x" />
<PackageReference Include="Jamesnet.Platform.Wpf" Version="9.x.x" />
```

이 구조에는 다음 문제가 있다.

- 사용자가 자신의 플랫폼 패키지까지 알아야 한다.
- `Jamesnet.Foundation`과 `Jamesnet.Platform.*`의 버전이 달라질 수 있다.
- 잘못된 조합을 설치할 수 있다.
- 문서, 샘플, 릴리스 및 호환성 표를 패키지별로 관리해야 한다.
- 하나의 프레임워크라는 제품 정체성이 여러 NuGet ID로 나뉜다.

단일 패키지로 통합하면 설치·버전·문서·릴리스 기준을 하나로 만들 수 있다.

## 3. 현재 저장소 상태

현재 저장소에는 다음 프로젝트가 있다.

| 프로젝트 | 현재 타깃/방식 | 현재 역할 |
|---|---|---|
| `Jamesnet.Foundation` | `netstandard2.0` | DI, View/ViewModel 매핑, Bootstrapper, Layer 계약 등 공통 기능 |
| `Jamesnet.Platform.WPF` | `net8.0-windows`, `UseWPF=true` | WPF Layer, Docking, Responsive 등 |
| `Jamesnet.Platform.OpenSilver` | `netstandard2.0`, OpenSilver 전용 속성 | OpenSilver Layer, ContentSwitcher, Responsive 등 |
| `Jamesnet.Platform.WinUI3` | `net8.0-windows10.0.19041.0`, `UseWinUI=true` | WinUI 3 Layer와 UI 도우미 |
| `Jamesnet.Platform.UWP` | 구형 UWP 프로젝트, `uap10.0` | UWP Layer와 UI 도우미 |

현재 태그 배포 워크플로는 Foundation과 WPF만 각각 패킹하고 각각 NuGet.org에 게시한다. 통합 후에는 태그 하나가 단일 `Jamesnet.Foundation.{version}.nupkg`만 만들어 게시해야 한다.

### 현재 구조에서 반드시 해결할 문제

1. Foundation과 OpenSilver가 모두 `netstandard2.0`이다.
2. WPF와 WinUI 3는 모두 Windows 계열 TFM이지만 사용하는 XAML 프레임워크가 다르다.
3. UWP는 SDK 스타일 프로젝트가 아니고 별도 `.nuspec`을 사용한다.
4. 플랫폼별 XAML 리소스(`Themes/Generic.xaml`)도 함께 전달되어야 한다.
5. 플랫폼별 의존성은 서로 다르며 불필요한 의존성이 다른 플랫폼으로 유출되면 안 된다.
6. 현재 각 프로젝트의 버전이 서로 다르므로 단일 버전 소스가 필요하다.

## 4. 권장 아키텍처

### 4.1 논리 구조

```text
Jamesnet.Foundation.nupkg
├── 공통 런타임
│   └── Jamesnet.Foundation.dll
├── WPF 구현
│   └── Jamesnet.Foundation.Wpf.dll
├── OpenSilver 구현
│   └── Jamesnet.Foundation.OpenSilver.dll
├── WinUI 3 구현
│   └── Jamesnet.Foundation.WinUI3.dll
├── UWP 구현
│   └── Jamesnet.Foundation.Uwp.dll
├── XAML 리소스
└── 플랫폼 선택용 MSBuild props/targets
```

DLL 이름은 내부 구현 세부사항이다. 공개 네임스페이스를 반드시 DLL 이름과 동일하게 만들 필요는 없다. 기존 API 호환성이 중요하면 기존 네임스페이스를 우선 유지하고, 장기적으로 `Jamesnet.Foundation` 아래의 공통 API로 정리한다.

### 4.2 권장 소스 구조

첫 통합에서는 기존 플랫폼 프로젝트를 무리하게 합치지 않는다.

```text
src/
├── Jamesnet.Foundation/
├── Jamesnet.Platform.WPF/
├── Jamesnet.Platform.OpenSilver/
├── Jamesnet.Platform.WinUI3/
├── Jamesnet.Platform.UWP/
└── Jamesnet.Foundation.Package/
    ├── Jamesnet.Foundation.Package.csproj 또는 .nuspec
    ├── build/
    │   ├── Jamesnet.Foundation.props
    │   └── Jamesnet.Foundation.targets
    └── buildTransitive/
        ├── Jamesnet.Foundation.props
        └── Jamesnet.Foundation.targets
```

`Jamesnet.Foundation.Package`는 코드를 다시 구현하는 프로젝트가 아니라, 각 빌드 산출물을 하나의 `.nupkg`으로 조립하는 패키징 프로젝트다.

이 방식의 장점:

- 현재 검증된 플랫폼 코드를 그대로 유지할 수 있다.
- 통합 작업과 리팩터링 작업을 분리할 수 있다.
- 플랫폼 하나의 빌드 실패가 어느 단계에서 발생했는지 명확하다.
- 기존 League of Legends 포팅 프로젝트로 플랫폼별 회귀 검증이 가능하다.

## 5. 왜 단순 `TargetFrameworks`만으로 부족한가

일반적인 멀티 타깃 라이브러리는 다음처럼 구성할 수 있다.

```xml
<TargetFrameworks>netstandard2.0;net8.0-windows;net8.0-windows10.0.19041.0</TargetFrameworks>
```

하지만 이 저장소에는 두 가지 충돌이 있다.

### 5.1 OpenSilver 판별 문제

OpenSilver 라이브러리도 `netstandard2.0`을 사용한다. 따라서 TFM만 보면 일반 `netstandard2.0` 소비자와 OpenSilver 소비자를 구분할 수 없다.

`lib/netstandard2.0`에 OpenSilver 구현을 무조건 넣으면 일반 .NET 프로젝트에도 OpenSilver 전용 자산과 의존성이 노출될 수 있다.

### 5.2 Windows XAML 플랫폼 판별 문제

WPF와 WinUI 3는 Windows 계열 TFM을 사용하지만 다음 속성으로 실제 UI 플랫폼이 나뉜다.

```xml
<UseWPF>true</UseWPF>
<UseWinUI>true</UseWinUI>
```

UWP는 `TargetPlatformIdentifier=UAP`와 UWP 프로젝트 시스템을 사용한다.

따라서 패키지 선택 로직은 TFM뿐 아니라 프로젝트 속성도 확인해야 한다.

## 6. 단일 패키지의 플랫폼 선택 전략

### 6.1 공통 DLL

공통 `Jamesnet.Foundation.dll`은 호환되는 `lib` 또는 `ref` 경로에 둔다. 이 DLL은 플랫폼 UI 어셈블리를 직접 참조하지 않아야 한다.

### 6.2 플랫폼 DLL

플랫폼 DLL은 서로 충돌하지 않는 패키지 내부 경로에 둔다.

예시:

```text
platforms/wpf/Jamesnet.Foundation.Wpf.dll
platforms/opensilver/Jamesnet.Foundation.OpenSilver.dll
platforms/winui3/Jamesnet.Foundation.WinUI3.dll
platforms/uwp/Jamesnet.Foundation.Uwp.dll
```

`buildTransitive/Jamesnet.Foundation.targets`가 소비 프로젝트의 속성을 확인하여 정확히 하나의 플랫폼 DLL만 `Reference`로 추가한다.

개념적인 선택 순서는 다음과 같다.

```text
UseWPF == true
    → WPF 구현
UseWinUI == true
    → WinUI 3 구현
TargetPlatformIdentifier == UAP
    → UWP 구현
OpenSilverType 존재 또는 OpenSilver 전용 속성 확인
    → OpenSilver 구현
그 외
    → 공통 Foundation만 사용하거나 명확한 오류 발생
```

실제 조건은 네 플랫폼의 최소 샘플 프로젝트에서 평가된 MSBuild 속성을 수집한 뒤 확정해야 한다. 특히 OpenSilver의 판별 속성은 버전별로 검증해야 하며, 추측으로 고정하면 안 된다.

### 6.3 중복 또는 미판별 방지

다음 검증을 `.targets`에 넣는 것이 좋다.

- 둘 이상의 플랫폼 조건이 동시에 참이면 빌드 오류
- 플랫폼 UI API를 사용하는데 어떤 플랫폼도 판별되지 않으면 설명 가능한 빌드 오류
- 지원하지 않는 TFM이면 지원 목록이 포함된 오류
- 플랫폼 구현 어셈블리 파일이 없으면 패키지 손상 오류

오류 예:

```text
Jamesnet.Foundation could not determine the XAML platform.
Set one of UseWPF, UseWinUI, UAP, or use a supported OpenSilver project.
```

## 7. 플랫폼별 고려사항

### 7.1 WPF

- 현재 `net8.0-windows`와 `UseWPF=true`를 사용한다.
- `Generic.xaml`을 비롯한 ResourceDictionary의 pack URI와 assembly name을 검증해야 한다.
- Docking/FloatingWindow처럼 WPF `Window`에 의존하는 기능은 WPF 구현에만 남긴다.
- 첫 통합 대상 및 기준 구현으로 삼는 것이 좋다.

### 7.2 OpenSilver

- `netstandard2.0`만으로 플랫폼을 판별할 수 없다.
- `OpenSilverType`, OpenSilver SDK import, 알려진 OpenSilver 속성 중 안정적인 판별 기준을 실험으로 정해야 한다.
- `Page`로 컴파일되는 `Themes/Generic.xaml`이 단일 패키지에서도 정상 등록되는지 검증한다.
- 브라우저와 Simulator를 모두 테스트한다.
- `OpenSilver` 패키지 의존성을 다른 플랫폼에 노출하지 않는 방법을 검증한다.
- 필요하면 OpenSilver 소비 앱이 이미 참조하는 OpenSilver SDK를 전제로 하고, 지원 최소 버전을 문서화한다.

### 7.3 WinUI 3

- `UseWinUI=true`와 Windows App SDK를 사용한다.
- `net8.0-windows10.0.19041.0` 자산이 WPF 프로젝트에 잘못 선택되지 않아야 한다.
- Windows App SDK 및 Windows SDK BuildTools 버전 정책을 정한다.
- unpackaged/packaged 앱과 x86/x64/ARM64를 확인한다.

### 7.4 UWP

- 현재 비 SDK 스타일 프로젝트와 별도 `.nuspec`을 사용한다.
- UWP 산출물은 Windows 환경과 Visual Studio Build Tools에서 빌드해야 한다.
- 현재 `.nuspec`이 `bin\Debug`를 패킹하므로 Release 산출물로 수정해야 한다.
- `uap10.0` 자산 및 `Microsoft.NETCore.UniversalWindowsPlatform` 호환성을 검증한다.
- UWP는 다른 세 플랫폼보다 패킹 도구 체인이 오래되었으므로 별도 빌드 후 최종 패키지 조립 방식이 안전하다.

## 8. 의존성 전략

단일 패키지의 가장 어려운 부분은 DLL을 넣는 일이 아니라 **플랫폼별 의존성을 격리하는 일**이다.

현재 주요 의존성:

| 플랫폼 | 주요 의존성 |
|---|---|
| 공통 | `System.Text.Json` |
| WPF | .NET Desktop/WPF |
| OpenSilver | `OpenSilver` |
| WinUI 3 | `Microsoft.WindowsAppSDK`, `Microsoft.Windows.SDK.BuildTools` |
| UWP | `Microsoft.NETCore.UniversalWindowsPlatform` |

NuGet의 일반 dependency group은 주로 TFM을 기준으로 선택된다. OpenSilver처럼 일반 `netstandard2.0`과 TFM이 겹치는 플랫폼은 dependency group만으로 완전한 격리가 어려울 수 있다.

권장 원칙:

1. 앱 SDK가 이미 제공하는 참조를 플랫폼 DLL에서 중복 전달하지 않는 방안을 우선 검토한다.
2. 반드시 전달해야 하는 패키지는 해당 플랫폼에만 적용되는지 복원 테스트로 확인한다.
3. `buildTransitive`에서 의존성을 동적으로 추가하는 방식은 NuGet restore 시점과 MSBuild 평가 순서 때문에 신중히 검증한다.
4. 필요하면 단일 공개 패키지가 내부 전용 패키지에 의존하는 구조를 사용할 수 있다.

내부 전용 패키지를 사용하는 절충안:

```text
Jamesnet.Foundation                 공개 진입점
Jamesnet.Foundation.Runtime.Wpf     내부 구현
Jamesnet.Foundation.Runtime.OpenSilver
Jamesnet.Foundation.Runtime.WinUI3
Jamesnet.Foundation.Runtime.Uwp
```

다만 NuGet은 임의의 `UseWPF`/`UseWinUI` 속성에 따라 dependency를 자동 선택하지 않으므로, 이 구조도 실제 복원 실험 후 채택해야 한다. 사용자가 직접 플랫폼 패키지를 설치하게 만들면 단일 패키지 목표를 달성하지 못한다.

## 9. API 설계 원칙

### 9.1 공통 계약 우선

Foundation에는 플랫폼 타입을 노출하지 않는 계약을 둔다.

```csharp
public interface ILayer
{
    string Name { get; }
}
```

플랫폼 구현은 이 계약을 구현한다.

```text
WPFLayer
OpenSilverLayer
WinUI3Layer
UwpLayer
```

### 9.2 플랫폼 타입이 필요한 API

플랫폼 UI 타입을 매개변수나 반환형으로 사용하는 API는 다음 중 하나를 선택한다.

- 동일한 개념과 이름을 플랫폼별 어셈블리에서 각각 컴파일
- 공통 인터페이스 뒤로 감춤
- 플랫폼 확장 API로 명확히 분리

조건부 컴파일을 지나치게 늘리는 것보다 플랫폼 구현 프로젝트에 코드를 두는 편이 유지보수에 유리하다.

### 9.3 네임스페이스

통합 즉시 모든 네임스페이스를 변경하면 기존 사용자의 소스 호환성이 깨진다. 권장 순서는 다음과 같다.

1. 버전 10에서 기존 네임스페이스를 유지하며 패키지만 통합
2. 통합 네임스페이스를 새 API에 적용
3. 필요한 경우 기존 타입을 obsolete 처리
4. 충분한 전환 기간 후 다음 메이저 버전에서 정리

## 10. 버전 정책

모든 플랫폼 산출물은 하나의 버전을 사용해야 한다.

권장:

```text
Directory.Build.props      공통 빌드 속성
Directory.Packages.props   중앙 패키지 버전(도입 시)
version.json 또는 태그     제품 버전의 단일 원천
```

첫 단일 패키지는 호환성 변화가 크므로 `10.0.0` 같은 메이저 버전이 적절하다.

버전 규칙 예:

- Major: 공개 API/지원 플랫폼/패키징 모델의 호환성 파괴
- Minor: 새 기능 또는 새 플랫폼
- Patch: 호환 가능한 버그 수정

## 11. 기존 패키지의 마이그레이션

기존 패키지를 즉시 삭제하면 안 된다.

### 단계 1: 통합 패키지 프리뷰

```text
Jamesnet.Foundation 10.0.0-preview.1
```

각 플랫폼 샘플과 League of Legends 프로젝트로 검증한다.

### 단계 2: 기존 패키지 동결

- `Jamesnet.Platform.Wpf`
- `Jamesnet.Platform.OpenSilver`
- `Jamesnet.Platform.WinUI3`
- `Jamesnet.Platform.Uwp`

이 패키지에는 신규 기능을 추가하지 않고 보안/중대 오류만 수정한다.

### 단계 3: 호환 메타 패키지

가능하면 기존 패키지의 새 버전은 `Jamesnet.Foundation`으로 이동하도록 안내하는 메타 패키지로 전환한다. 단, 순환 의존성이 생기지 않도록 설계해야 한다.

### 단계 4: 폐기 표시

NuGet.org에서 기존 패키지를 unlist하거나 삭제하기보다 deprecated 표시와 대체 패키지 안내를 사용한다.

### 소비 프로젝트 변경

기존:

```xml
<PackageReference Include="Jamesnet.Foundation" Version="9.1.13" />
<PackageReference Include="Jamesnet.Platform.Wpf" Version="9.1.13.18" />
```

변경:

```xml
<PackageReference Include="Jamesnet.Foundation" Version="10.0.0" />
```

## 12. 구현 순서

### Phase 0: 기준선 확보

- 현재 네 플랫폼 프로젝트의 Release 빌드 성공 여부 기록
- 공개 타입/API 목록 생성
- 현재 NuGet 패키지 내부 파일 목록 보관
- 각 플랫폼 최소 샘플 앱 준비
- League of Legends WPF를 기준 구현으로 지정

### Phase 1: WPF 패키징 PoC

- 패키징 프로젝트 생성
- 공통 DLL과 WPF DLL을 단일 `.nupkg`에 포함
- 새 WPF 앱에서 패키지 참조 한 줄만으로 빌드·실행
- ResourceDictionary, Layer, Docking, Responsive 검증

### Phase 2: WinUI 3 추가

- `UseWinUI` 판별
- WinUI 3 DLL만 선택되는지 확인
- Windows App SDK 의존성 복원 확인
- WPF/WinUI 교차 오염 테스트

### Phase 3: OpenSilver 추가

- 안정적인 OpenSilver 판별 조건 확정
- 브라우저/Simulator 빌드
- OpenSilver XAML 리소스 및 WASM 실행
- 일반 `netstandard2.0` 프로젝트에 OpenSilver 자산이 들어오지 않는지 확인

### Phase 4: UWP 추가

- Release UWP 빌드 자동화
- UWP 자산을 최종 `.nupkg`에 조립
- x86/x64/ARM/ARM64 및 최소 Windows 버전 검증 범위 결정

### Phase 5: API/네임스페이스 정리

- 중복 타입 비교
- 공통화 가능한 코드만 Foundation으로 이동
- 플랫폼 확장 API 문서화
- 호환성 shim 및 obsolete 정책 적용

### Phase 6: 프리뷰와 안정화

- `10.0.0-preview.*` 게시
- 네 개 League of Legends 포팅 프로젝트에 적용
- 사용자 설치 문서와 마이그레이션 문서 작성
- `10.0.0` 정식 게시

## 13. 테스트 매트릭스

단일 패키지는 "패킹 성공"만으로 검증되지 않는다. **복원 → 컴파일 → XAML 컴파일 → 실행** 전 과정을 확인해야 한다.

| 플랫폼 | Restore | Build | XAML | Runtime | 핵심 검증 |
|---|---:|---:|---:|---:|---|
| WPF | 필수 | 필수 | 필수 | 필수 | Layer, Docking, Responsive |
| OpenSilver Browser | 필수 | 필수 | 필수 | 필수 | WASM, 리소스, ContentSwitcher |
| OpenSilver Simulator | 필수 | 필수 | 필수 | 필수 | Simulator 동작 |
| WinUI 3 | 필수 | 필수 | 필수 | 필수 | Windows App SDK, 아키텍처 |
| UWP | 필수 | 필수 | 필수 | 필수 | UAP 자산, Release 패키징 |

추가 부정 테스트:

- WPF 앱에 WinUI/OpenSilver/UWP DLL이 참조되지 않음
- WinUI 앱에 WPF DLL이 참조되지 않음
- 일반 `netstandard2.0` 라이브러리에 OpenSilver 구현이 참조되지 않음
- 플랫폼 조건 두 개가 충돌하면 명확한 오류
- 지원하지 않는 TFM에서 명확한 오류 또는 공통 기능만 정상 사용
- 소비 프로젝트가 직접 플랫폼 구현 DLL을 선택할 필요가 없음

## 14. CI/CD 권장 구성

현재 Windows GitHub Actions 워크플로를 다음 단계로 확장한다.

```text
1. checkout
2. 버전 계산 및 검증
3. Foundation Release 빌드
4. WPF Release 빌드
5. OpenSilver Release 빌드
6. WinUI 3 Release 빌드
7. UWP Release 빌드
8. 단일 nupkg 조립
9. nupkg 구조 검사
10. 로컬 패키지 소스로 플랫폼별 샘플 restore/build
11. 서명(도입 시)
12. NuGet.org에 단일 패키지 push
```

권장 안전장치:

- 태그 버전과 패키지 버전 불일치 시 실패
- Debug 산출물이 패키지에 포함되면 실패
- 예상 플랫폼 DLL 또는 PDB가 없으면 실패
- 동일 버전의 플랫폼 산출물이 아니면 실패
- 프리뷰 단계에서는 NuGet.org 또는 별도 피드에 prerelease 게시
- `--skip-duplicate`에만 의존하지 말고 게시 전 패키지 ID/버전을 검사

## 15. 성공 기준

다음 조건을 모두 만족하면 단일 패키지 전환이 완료된 것이다.

1. 네 플랫폼의 새 샘플 앱이 패키지 참조 한 줄만 사용한다.
2. 사용자가 `Jamesnet.Platform.*` 패키지를 직접 설치하지 않는다.
3. 모든 플랫폼 산출물이 동일 버전을 사용한다.
4. 플랫폼마다 올바른 구현 DLL과 XAML 리소스만 선택된다.
5. 다른 플랫폼의 SDK/런타임 의존성이 유출되지 않는다.
6. WPF/OpenSilver/WinUI 3/UWP League of Legends 앱이 핵심 화면까지 실행된다.
7. 기존 9.x 사용자를 위한 마이그레이션 안내가 제공된다.
8. CI가 단일 `.nupkg`을 재현 가능하게 생성하고 검증한다.

## 16. 위험과 대응

| 위험 | 영향 | 대응 |
|---|---|---|
| OpenSilver와 공통 코드의 TFM 중복 | 잘못된 자산 선택 | `buildTransitive` 판별 PoC를 가장 먼저 수행 |
| WPF/WinUI 자산 혼선 | 컴파일 충돌 | `UseWPF`/`UseWinUI` 상호 배타 검증 |
| UWP 구형 빌드 체인 | CI/패킹 실패 | UWP 별도 빌드 후 패키징 프로젝트가 산출물 수집 |
| XAML 리소스 경로 변화 | 런타임 리소스 누락 | 각 플랫폼 실제 앱 실행 테스트 |
| 플랫폼 의존성 유출 | 복원 실패/패키지 비대화 | 플랫폼별 `project.assets.json` 검사 |
| 네임스페이스 일괄 변경 | 소스 호환성 파괴 | 첫 통합에서는 기존 네임스페이스 유지 |
| 버전 불일치 | 재현 불가능한 조합 | 단일 버전 원천과 CI 검증 |

## 17. 채택하지 말아야 할 접근

### 모든 코드를 하나의 프로젝트에 복사

플랫폼 SDK import, XAML 빌드 작업, UI 타입 충돌 때문에 프로젝트 파일이 과도하게 복잡해진다.

### 하나의 물리적 DLL에 모든 플랫폼 UI 참조 포함

사용하지 않는 플랫폼에서도 다른 UI 런타임을 요구하게 되며 어셈블리 로딩 실패 가능성이 있다.

### TFM만으로 OpenSilver 판별

OpenSilver가 `netstandard2.0`을 사용하므로 충분하지 않다.

### 기존 패키지를 즉시 폐기

기존 소비 앱의 복원과 빌드를 깨뜨린다. 프리뷰와 전환 기간이 필요하다.

### DLL 병합 도구로 플랫폼 DLL 결합

타입 참조와 런타임 차이는 파일을 병합한다고 사라지지 않으며 XAML 리소스 해석도 더 어려워진다.

## 18. 최초 PoC에서 답해야 할 질문

구현을 시작하기 전에 다음을 실제 패키지로 증명해야 한다.

1. OpenSilver 소비 프로젝트에서 안정적으로 관찰되는 MSBuild 판별 속성은 무엇인가?
2. `buildTransitive`에서 추가한 플랫폼 DLL이 디자인 타임과 XAML 컴파일에도 보이는가?
3. WPF와 WinUI 3가 서로의 플랫폼 DLL을 전혀 참조하지 않는가?
4. OpenSilver/WinUI/UWP의 필수 NuGet 의존성을 다른 플랫폼에 노출하지 않을 수 있는가?
5. 각 플랫폼 `Generic.xaml`과 pack URI가 새 assembly/package 구조에서 작동하는가?
6. UWP Release DLL을 자동으로 단일 패키지에 조립할 수 있는가?
7. 기존 공개 타입의 assembly identity 변화가 직렬화, XAML namespace, 리플렉션 코드에 영향을 주는가?

이 질문에 대한 자동화된 테스트가 만들어지기 전에는 정식 `10.0.0`을 게시하지 않는다.

## 19. 최종 권고

`Jamesnet.Foundation`을 단일 NuGet으로 만드는 방향은 기술적으로 타당하며 프로젝트의 원래 의도와도 맞는다. 다만 성공적인 통합은 "프로젝트를 하나로 합치는 작업"이 아니라 다음 세 가지를 함께 수행하는 제품화 작업이다.

1. 공통 계약과 플랫폼 구현의 경계를 유지한다.
2. 플랫폼별 빌드 산출물을 단일 `.nupkg`으로 조립한다.
3. MSBuild가 소비 프로젝트의 XAML 플랫폼을 판별해 올바른 구현만 선택하게 한다.

가장 안전한 출발점은 WPF 단일 패키징 PoC이며, 이후 WinUI 3, OpenSilver, UWP 순으로 추가한다. 특히 OpenSilver의 `netstandard2.0` 중복과 플랫폼별 의존성 격리가 전체 계획의 핵심 기술 검증 지점이다.

## 20. 참고 자료

- Microsoft Learn, Multi-targeting for NuGet Packages  
  <https://learn.microsoft.com/nuget/create-packages/supporting-multiple-target-frameworks>
- Microsoft Learn, Target frameworks in SDK-style projects  
  <https://learn.microsoft.com/dotnet/standard/frameworks>
- Microsoft Learn, PackageReference in project files  
  <https://learn.microsoft.com/nuget/consume-packages/package-references-in-project-files>
- Microsoft Learn, Cross-platform targeting for .NET libraries  
  <https://learn.microsoft.com/dotnet/standard/library-guidance/cross-platform-targeting>
- Microsoft Learn, `.nuspec` reference  
  <https://learn.microsoft.com/nuget/reference/nuspec>

## 21. Windows 검증 인계

macOS에서 수행할 수 없는 WPF, WinUI 3, UWP 빌드와 실제 실행 검증은 다음 문서에 따라 Windows 환경에서 수행한다.

- [Windows 빌드·검증 지침](./WINDOWS_VALIDATION_GUIDE_CODEX.md)
- [Windows 검증 보고서 템플릿](./WINDOWS_VALIDATION_REPORT_TEMPLATE_CODEX.md)

Windows 검증자는 템플릿을 `reports/windows-validation/YYYY-MM-DD-<commit-short-sha>.md`로 복사해 작성한다. Codex는 커밋 SHA, 환경, 명령, 종료 코드, 패키지 자산 선택 및 실행 결과를 기준으로 보고서를 검토한다.

---

이 문서는 구현 전 설계 기준이다. 실제 PoC 결과에 따라 패키지 내부 경로와 플랫폼 판별 조건은 갱신하되, **사용자는 `Jamesnet.Foundation` 하나만 참조한다**는 제품 목표는 유지한다.
