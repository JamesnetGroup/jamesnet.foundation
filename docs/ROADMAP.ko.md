# Jamesnet.Foundation 로드맵

> 프로젝트 방향과 확정된 방침은 [DIRECTION.ko.md](DIRECTION.ko.md), 설계 배경은 [ARCHITECTURE.ko.md](ARCHITECTURE.ko.md)를 참고한다.
> 이 문서는 "앞으로 무엇을 할 것인가"만 다룬다.

## 우선순위 원칙

**WPF · OpenSilver · Uno 세 플랫폼이 제대로 동작하는 것이 최우선이다.**
WinUI3와 UWP도 계속 지원하지만 검증 순서는 뒤다. 어떤 플랫폼도 폐기하지 않는다.

릴리스는 모든 항목이 완성되고 검증된 뒤에 한다. 중간 상태로 NuGet에 올리지 않는다.

---

## 현재 위치

**v10.0 — 단일 패키지 통합 완료**

- [x] 5개 플랫폼 패키지를 `Jamesnet.Foundation` 하나로 통합
- [x] `WPFLayer`/`OpenSilverLayer`/`UnoLayer`/`WinUI3Layer`/`UwpLayer` → `XamlLayer` 한 벌
- [x] 코어 소스를 `src/Shared/Core/`로 이동, 각 flavor가 자기완결형
- [x] flavor 자동 선택 (`buildTransitive/Jamesnet.Foundation.targets`)
- [x] 기준선 최신화 (.NET 10 / OpenSilver 3.3.3 / Uno.Sdk 6.7.30 / WindowsAppSDK 2.6.4)
- [x] 버전 일원화 (루트 `Directory.Build.props`)
- [x] 기존 타입명을 `[Obsolete]` 별칭으로 유지
- [x] XAML 네임스페이스 통합 (WPF·OpenSilver는 `http://jamesnet.dev/xaml`,
      Uno·WinUI3·UWP는 `using:Jamesnet.Foundation` — 양쪽 다 어셈블리명이 드러나지 않음)

---

## Phase 1 — 중복 제거 ✅ 완료

플랫폼마다 흩어져 있던 같은 코드를 `Shared/Xaml/` 한 벌로 올렸다.

- [x] Responsive 12종 → `Shared/Xaml/Responsive/`. 모든 플랫폼이 같은 이름·같은 속성명을 갖는다.
      (`ResponsiveColumns`만 예외: UniformGrid가 없는 WinUI3/Uno/UWP에는 없다)
- [x] 플랫폼별 창 너비 추적을 공용 `ResponsiveHelper` 하나로 통합
      — WPF는 소속 Window, WinUI3/Uno는 XamlRoot, OpenSilver/UWP는 Window.Current.
      Loaded 동안만 구독하므로 누수가 없고, 다시 Loaded되면 자동 재구독한다.
      이에 따라 쓰이지 않게 된 `ResponsiveManager`/`IResponsiveHost`/`IResponsiveElement`는 제거했다.
- [x] `RecursiveControl`, `CustomWrapPanel` → `Shared/Xaml/`
- [x] `AnimatedWPFLayer`/`AnimatedOpenSilverLayer` → `AnimatedXamlLayer` 한 벌 (전 플랫폼)
- [x] 9.x 이름(`WPFLayer`, `AnimatedWPFLayer`, `UnoLayer` …)은 `Shared/Xaml/LegacyAliases.cs`의 `[Obsolete]` 별칭으로 유지
- [x] 플랫폼별 `Responsive/` 폴더와 레이어 파일 제거

이제 플랫폼 폴더에 남은 것은 **그 플랫폼에만 있는 것**뿐이다.

| 플랫폼 | 남은 코드 | 이유 |
|---|---|---|
| WPF | Docking 4종 | WPF 전용 기능 (Phase 2-1에서 방침 결정) |
| OpenSilver | `ContentSwitcher`, `ContentCase`, `IMobileView`, `DeviceInfo` | 브라우저(HtmlPage) 의존 또는 OpenSilver 전용 |
| Uno / WinUI3 | 없음 (프로젝트 파일만) | |
| UWP | `AssemblyInfo` | 레거시 프로젝트 형식 |

## Phase 2 — 정책 결정

Phase 1을 진행하면 반드시 부딪히는 결정들이다. 미리 정해두면 매번 고민하지 않아도 된다.

### 2-1. 플랫폼 고유 기능을 어떻게 다룰 것인가

WPF에만 Docking 계열(`DockingManager`, `DockingPanel`, `DockingCompass`,
`FloatingWindow`)이 있다. 세 가지 선택지 중 하나를 정해야 한다.

1. 전 플랫폼에 이식한다
2. WPF flavor에만 두고 다른 플랫폼에는 아예 없다 (현재 상태)
3. 인터페이스는 `Shared/`에 공통으로, 구현은 WPF만

- [x] WPF 전용 유지로 결정 ([DIRECTION.ko.md](DIRECTION.ko.md) 3-1)

### 2-2. 명명 규칙

`XamlLayer`는 정했지만 나머지 타입의 규칙이 없다. `Xaml` 접두사를 일반 규칙으로
삼을지, `XamlLayer`만 예외로 둘지 정한다.

- [x] `Xaml` 접두사는 `XamlLayer` 계열만으로 결정 ([DIRECTION.ko.md](DIRECTION.ko.md) 3-2)
- [x] Phase 1 결과물 이름이 규칙에 맞음을 확인

### 2-3. 9.x 패키지 처리

NuGet에 `Jamesnet.Platform.OpenSilver`, `.Wpf`, `.Uno`, `.Uwp`, `.WinUI3`가 그대로 남아 있다.

- [x] v10 출시 후 deprecate하기로 결정 ([DIRECTION.ko.md](DIRECTION.ko.md) 3-3)
- [ ] 표시한다면 `Jamesnet.Foundation`을 대체 패키지로 지정

---

## Phase 3 — 신뢰성

### 3-1. 테스트

`tests/Jamesnet.Foundation.Tests` (xUnit, WPF flavor로 공용 소스를 검사).

- [x] 코어 단위 테스트 (`Container`, `LayerManager`, `RelayCommand`, `ViewModelBase`, Yaml)
- [x] WPF 실제 창 테스트 (`XamlLayer`, `AnimatedXamlLayer`, Responsive*, 레이어 자동 등록)
- [ ] Uno 런타임 테스트 — 로컬 임시 앱으로는 통과 확인. 헤드리스 실행 방법을 정해 저장소로 옮긴다
- [ ] flavor 스모크 테스트: 각 flavor DLL이 `XamlLayer`를 노출하는지 확인
- [ ] 소비자 검증 자동화 — 지금은 수동으로 4개 소비자 프로젝트를 만들어 확인했다.
      이 절차를 CI 스크립트로 고정한다

### 3-2. CI

- [ ] UWP flavor가 CI에서 실제로 빌드되는지 확인 (로컬에서는 VS MSBuild가 없어 미검증)
- [x] 태그 없이도 PR마다 전체 flavor 빌드가 도는 워크플로우 추가 (`.github/workflows/ci.yml`: 테스트 + 전체 flavor pack + 데모 빌드)

### 3-3. 데모

- [x] Uno 데모 앱 (`samples/DemoUnoApp`, 데스크톱. 메뉴 전환과 반응형 메뉴 폭까지 실행 확인)
- [x] WinUI3 데모 앱 (`samples/DemoWinUI3App`, MSIX 없이 실행. 메뉴 전환 실행 확인)
- [ ] 데모 3종이 **동일한 View/ViewModel 코드**를 공유하도록 구성 —
      통합이 실제로 성립하는지 보여주는 가장 좋은 증거가 된다

### 3-4. 저장소 정리

- [x] `src/`(라이브러리) · `tests/` · `samples/`로 분리, 솔루션은 루트의 `Jamesnet.Foundation.slnx`
- [x] flavor 프로젝트 이름을 `Jamesnet.Platform.*` → `Jamesnet.Foundation.*`로 통일
      (빌드 결과물·네임스페이스는 그대로라 소비자 영향 없음)
- [x] 쓰지 않는 잔재 제거 (빈 `WpfApp1`, UWP 9.x nuspec과 저장소에 들어 있던 `nuget.exe`,
      어디서도 컴파일되지 않던 `InjectAttribute`)
- [x] 전 의존성 최신화 (Windows App SDK 2.6.4, Uno.Sdk 6.7.30, xUnit v3, Actions 최신 메이저)

---

## Phase 4 — 확장

- [ ] AvaloniaUI flavor (`src/Jamesnet.Foundation.Avalonia`에 자리만 있음)
- [ ] MAUI flavor (`src/Jamesnet.Foundation.Maui`에 자리만 있음)

새 플랫폼 추가 절차는 이미 고정되어 있다.

1. flavor 프로젝트 생성 — `AssemblyName`을 `Jamesnet.Foundation`으로, 자기 `DefineConstants` 지정
2. `Shared/**` 링크 컴파일
3. `Shared/Xaml/*.cs`의 `#if`에 새 심볼 반영
4. `Jamesnet.Foundation.Package.csproj`의 `JamesnetFlavor` 항목에 추가
5. `buildTransitive/Jamesnet.Foundation.targets`에 판별 조건 한 줄 추가

**소비자가 받는 패키지 개수는 계속 하나다.**

---

## 릴리스 기준

v10.0.0을 NuGet에 올리기 위한 조건이다.

- [x] Phase 1 완료 (중복 제거)
- [x] Phase 2 결정 사항 문서 반영
- [x] Phase 3-1 코어 테스트 통과
- [x] WPF · OpenSilver · Uno 데모 3종 정상 동작 확인 (WinUI3 데모 포함, 메뉴 전환까지 실행 확인)
- [ ] CI에서 전체 flavor(UWP 포함) 포함 pack 성공
- [ ] `JAMESNETPACK001` 경고 없이 패키지 생성
