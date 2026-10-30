# Jamesnet.Foundation 방향성

> 이 문서는 "이 프로젝트가 어디로 가는가, 무엇을 지키는가"를 다룬다.
> 구체적인 작업 목록은 [ROADMAP.ko.md](ROADMAP.ko.md), 내부 구조는 [ARCHITECTURE.ko.md](ARCHITECTURE.ko.md)를 참고한다.

## 한 문장 요약

**어떤 XAML 플랫폼을 쓰든, 패키지 하나·같은 타입 이름·같은 코드로 MVVM 앱을 만들 수 있게 한다.**

---

## 1. 지향점

### 패키지는 하나

소비자는 `Jamesnet.Foundation` 하나만 설치한다. WPF, OpenSilver, Uno, WinUI3, UWP 중
무엇을 쓰는지는 패키지가 프로젝트를 보고 스스로 판단한다(`buildTransitive` targets).
플랫폼별 패키지(`Jamesnet.Platform.Wpf` 등)를 다시 만들지 않는다.

### 타입 이름도 하나

`XamlLayer`, `AnimatedXamlLayer`, `ResponsivePanel` … 어느 플랫폼에서나 같은 이름이다.
플랫폼 이름이 들어간 타입(`WPFLayer`, `UnoLayer` 등)은 새로 만들지 않는다.

### 코드도 하나

라이브러리 소스는 `src/Shared` 한 벌이다. 플랫폼 차이는 `#if WPF` 같은 조건부 컴파일로
최소한만 흡수한다. 플랫폼 폴더에는 **그 플랫폼에만 존재할 수 있는 것**만 남긴다.

궁극적으로는 소비자의 View/ViewModel 코드도 플랫폼 간에 그대로 옮겨갈 수 있어야 한다.

---

## 2. 지키는 원칙

| 원칙 | 의미 |
|---|---|
| **플랫폼을 버리지 않는다** | 지원 중인 XAML 플랫폼은 계속 지원한다. 오래된 플랫폼(UWP 등)도 유지한다. |
| **WPF · OpenSilver · Uno가 우선** | 검증과 기능 작업은 이 셋을 먼저 한다. WinUI3 · UWP는 그다음이다. |
| **기존 사용자를 깨지 않는다** | 9.x 이름은 `[Obsolete]` 별칭으로 남겨 경고만 띄운다. 네임스페이스와 DLL 이름은 유지한다. |
| **하한선은 함부로 올리지 않는다** | WinUI3/UWP의 최소 Windows 버전 같은 "소비자가 맞춰야 하는 기준"은 이유 없이 올리지 않는다. 도구와 의존성은 최신으로 유지한다. |
| **검증 후 배포** | 모든 플랫폼에서 빌드·테스트·데모 실행이 확인되기 전에는 NuGet에 올리지 않는다. |
| **의존성을 늘리지 않는다** | DI 컨테이너, YAML 파서 등은 직접 가진 작은 구현을 쓴다. 외부 패키지는 플랫폼 SDK 외에는 들이지 않는다. |

---

## 3. 정해진 방침 (Phase 2)

로드맵 Phase 2에서 남아 있던 정책 질문에 대한 답이다.

### 3-1. 플랫폼 고유 기능 → 해당 플랫폼에만 둔다

Docking(`DockingManager`, `DockingPanel`, `DockingCompass`, `FloatingWindow`)은 WPF에만 둔다.

- 여러 창(Window)을 띄우는 Docking은 브라우저(OpenSilver)나 모바일(Uno)에서 의미가 다르다.
  억지로 이식하면 모든 플랫폼에서 어중간한 기능이 된다.
- 공통 개념(`IDockable`, `DockPosition`, `CompassTarget`)은 이미 `Shared/Core/Docking`에 있으므로,
  다른 플랫폼에서 필요해지면 그때 구현을 추가할 수 있다.
- OpenSilver 전용(`ContentSwitcher`, `DeviceInfo` 등)도 같은 원칙을 따른다.

**규칙:** 모든 플랫폼에서 같은 의미를 가질 수 있는 기능만 `Shared`에 둔다.

### 3-2. 명명 규칙 → `Xaml` 접두사는 `XamlLayer` 계열만

- `XamlLayer`, `AnimatedXamlLayer`는 9.x의 `WPFLayer`/`UnoLayer` 등을 대체하기 위한 이름이라 예외로 둔다.
- 그 밖의 타입은 접두사 없이 역할만 드러낸다(`ResponsivePanel`, `RecursiveControl`, `LayerManager`).
  (`XamlNamespace`는 접두사가 아니라 XAML 네임스페이스 URI 자체를 담는 이름이라 규칙과 충돌하지 않는다.)
- 플랫폼 이름(`Wpf`, `Uno` 등)을 타입 이름에 넣지 않는다.

### 3-3. 9.x 패키지 → v10 출시 후 deprecate

v10.0.0이 NuGet에 올라간 뒤 `Jamesnet.Platform.Wpf`, `.OpenSilver`, `.Uno`, `.WinUI3`, `.Uwp`를
deprecate 표시하고 대체 패키지로 `Jamesnet.Foundation`을 지정한다. 삭제(unlist)는 하지 않는다.
기존 버전을 쓰는 프로젝트는 계속 복원할 수 있어야 한다.

---

## 4. 앞으로의 흐름

```
v10.0  단일 패키지 출시      ← 지금 여기 (CI 전체 검증만 남음)
  │
v10.x  신뢰성                 Uno 런타임 테스트 자동화, flavor 스모크 테스트,
  │                           데모들이 같은 View/ViewModel 코드를 공유하도록 정리
  │
v11    확장                   Avalonia, MAUI flavor 추가
                              (추가해도 소비자가 받는 패키지는 여전히 하나)
```

---

## 5. 기여자를 위한 판단 기준

새 기능이나 변경을 제안할 때 다음을 먼저 확인한다.

1. **모든 플랫폼에서 같은 이름·같은 동작으로 제공할 수 있는가?**
   그렇다면 `src/Shared`에, 아니라면 해당 flavor 폴더에 둔다.
2. **기존 사용자의 코드가 깨지는가?** 깨진다면 `[Obsolete]` 별칭이나 이행 경로를 함께 제공한다.
3. **새 외부 의존성이 필요한가?** 가능하면 피한다.
4. **WPF · OpenSilver · Uno에서 실제로 실행해 확인했는가?**
   테스트(`tests/`)와 데모(`samples/`)로 확인할 수 있어야 한다.
