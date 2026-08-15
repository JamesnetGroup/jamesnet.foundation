# Jamesnet.Foundation 로드맵

> 설계 배경과 원칙은 [ARCHITECTURE.ko.md](ARCHITECTURE.ko.md)를 참고한다.
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
- [x] 기준선 최신화 (.NET 10 / OpenSilver 3.3.3 / Uno.Sdk 6.6.42 / WindowsAppSDK 2.4.0)
- [x] 버전 일원화 (`src/Directory.Build.props`)
- [x] 기존 타입명을 `[Obsolete]` 별칭으로 유지
- [x] XAML 네임스페이스 통합 (WPF·OpenSilver는 `http://jamesnet.dev/xaml`,
      Uno·WinUI3·UWP는 `using:Jamesnet.Foundation` — 양쪽 다 어셈블리명이 드러나지 않음)

---

## Phase 1 — 중복 제거 (다음 작업)

지금도 같은 코드가 플랫폼마다 흩어져 있다. `XamlLayer`에 적용한 방식
(`Shared/` + `#if`)을 그대로 확대 적용한다.

### 1-1. Responsive 통합

현재 분포가 심하게 어긋나 있다.

| 플랫폼 | 보유 개수 |
|---|---|
| OpenSilver | 12 (가장 완전함 — 통합 기준) |
| WPF | 5 (+ `ResponsiveHelper`) |
| Uno | 1 |
| WinUI3 | 1 |
| UWP | 0 |

OpenSilver의 12종을 `Shared/Xaml/Responsive/`로 올린다. 완료되면 **모든 플랫폼이
동시에 12종 전부를 갖게 된다.** 이 작업 하나로 얻는 기능 이득이 가장 크다.

- [ ] `ResponsiveVisibility` 먼저 (5개 플랫폼 모두 있어 차이 비교가 쉽다) — 파일럿
- [ ] `ResponsiveWidth` / `Height` / `MinHeight` / `Margin` / `BorderThickness`
- [ ] `ResponsiveFontSize` / `FontWeight`
- [ ] `ResponsiveColumns` / `Content` / `Child` / `AnimationTo`
- [ ] `ResponsiveHelper`(WPF 전용) 처리 방침 결정 후 반영
- [ ] 플랫폼별 `Responsive/` 폴더 제거

주의: `ResponsiveAnimationTo`는 애니메이션 API가 플랫폼마다 차이가 크므로 마지막에
다룬다. 통합이 무리라면 이것만 플랫폼별로 남기고 그 이유를 코드에 남긴다.

### 1-2. 기타 중복

- [ ] `RecursiveControl` — WPF, Uno, WinUI3, UWP에 각각 존재
- [ ] `CustomWrapPanel` — WinUI3, UWP에 각각 존재

### 1-3. 정리 결과 목표

플랫폼 폴더에는 **그 플랫폼에만 존재하는 것**만 남긴다. 현재 파일 수는
WPF 13 / OpenSilver 18 / Uno 3 / WinUI3 4 / UWP 4이며, Phase 1이 끝나면
대부분이 `Shared/`로 올라가야 한다.

---

## Phase 2 — 정책 결정

Phase 1을 진행하면 반드시 부딪히는 결정들이다. 미리 정해두면 매번 고민하지 않아도 된다.

### 2-1. 플랫폼 고유 기능을 어떻게 다룰 것인가

WPF에만 Docking 계열(`DockingManager`, `DockingPanel`, `DockingCompass`,
`FloatingWindow`)이 있다. 세 가지 선택지 중 하나를 정해야 한다.

1. 전 플랫폼에 이식한다
2. WPF flavor에만 두고 다른 플랫폼에는 아예 없다 (현재 상태)
3. 인터페이스는 `Shared/`에 공통으로, 구현은 WPF만

- [ ] 방침 결정 후 ARCHITECTURE.ko.md 원칙 항목에 추가

### 2-2. 명명 규칙

`XamlLayer`는 정했지만 나머지 타입의 규칙이 없다. `Xaml` 접두사를 일반 규칙으로
삼을지, `XamlLayer`만 예외로 둘지 정한다.

- [ ] 규칙 확정 후 문서화
- [ ] 확정된 규칙에 맞춰 Phase 1 결과물 이름 정리

### 2-3. 9.x 패키지 처리

NuGet에 `Jamesnet.Platform.OpenSilver`, `.Wpf`, `.Uno`, `.Uwp`, `.WinUI3`가 그대로 남아 있다.

- [ ] deprecate 표시 여부 결정
- [ ] 표시한다면 `Jamesnet.Foundation`을 대체 패키지로 지정

---

## Phase 3 — 신뢰성

### 3-1. 테스트

**현재 테스트 프로젝트가 하나도 없다.** 5개 플랫폼에 같은 코드를 공유하는 구조라서
회귀를 놓치기 쉽다.

- [ ] 코어 단위 테스트 (`Container`, `LayerManager`, `ViewModelMapper`, `RelayCommand`, Yaml)
      — 플랫폼 무관이므로 netstandard 타겟 하나로 충분
- [ ] flavor 스모크 테스트: 각 flavor DLL이 `XamlLayer`를 노출하는지 확인
- [ ] 소비자 검증 자동화 — 지금은 수동으로 4개 소비자 프로젝트를 만들어 확인했다.
      이 절차를 CI 스크립트로 고정한다

### 3-2. CI

- [ ] UWP flavor가 CI에서 실제로 빌드되는지 확인 (로컬에서는 VS MSBuild가 없어 미검증)
- [ ] 태그 없이도 PR마다 전체 flavor 빌드가 도는 워크플로우 추가

### 3-3. 데모

- [ ] Uno 데모 앱 (우선순위 플랫폼인데 데모가 없다)
- [ ] WinUI3 데모 앱
- [ ] 데모 3종이 **동일한 View/ViewModel 코드**를 공유하도록 구성 —
      통합이 실제로 성립하는지 보여주는 가장 좋은 증거가 된다

---

## Phase 4 — 확장

- [ ] AvaloniaUI flavor (`src/Jamesnet.Platform.AvaloniaUI`에 자리만 있음)
- [ ] MAUI flavor (`src/Jamesnet.Platform.MAUI`에 자리만 있음)

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

- [ ] Phase 1 완료 (중복 제거)
- [ ] Phase 2 결정 사항 문서 반영
- [ ] Phase 3-1 코어 테스트 통과
- [ ] WPF · OpenSilver · Uno 데모 3종 정상 동작 확인
- [ ] CI에서 전체 flavor(UWP 포함) 포함 pack 성공
- [ ] `JAMESNETPACK001` 경고 없이 패키지 생성
