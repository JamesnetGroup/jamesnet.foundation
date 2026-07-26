# Jamesnet.Foundation 단일 NuGet 패키지 통합 계획

> 작성: Kimi (AI Assistant) · 2026-07-26
> 목표: `Jamesnet.Foundation` 단일 NuGet 패키지 하나로 **WPF, OpenSilver, WinUI3, UWP** 앱에서 모두 사용 가능한 구조로 전환

---

## 1. 배경과 목표

### 현재 구조
- `Jamesnet.Foundation` — 플랫폼 중립 코어 (`netstandard2.0`): DI 컨테이너, AppBootstrapper, LayerManager, ViewModelMapper 등 "계약 + 순수 로직"
- `Jamesnet.Platform.{Wpf,OpenSilver,WinUI3,Uwp,Uno,...}` — 플랫폼별 `ILayer` 구현체, 테마, 컨트롤

### 분리된 이유 (기술적 제약)
각 플랫폼은 상호 배타적인 SDK/TFM을 요구하므로 **하나의 csproj로 빌드 불가**:
- WPF: `net8.0-windows` + `<UseWPF>true</UseWPF>`
- WinUI3: `net8.0-windows10.0.19041.0` + `<UseWinUI>true</UseWinUI>` + WindowsAppSDK
- OpenSilver: `netstandard2.0` + OpenSilver 3.2.0 패키지 (`DefineSilverlight`)
- UWP: 구형(non-SDK) 프로젝트, UAP `10.0.19041` / min `10.0.18362`, msbuild 전용
- Uno: `Uno.Sdk` (SDK 자체가 다름)

### 목표
프로젝트(빌드 단위)는 유지하되, **배포 단위(NuGet 패키지)를 `Jamesnet.Foundation` 하나로 통합**한다.
소비자는 `Install-Package Jamesnet.Foundation` 하나만 하면 NuGet이 자신의 TFM에 맞는 구현을 자동 선택한다.

이번 통합 범위: **WPF, OpenSilver, WinUI3, UWP** (Uno/AvaloniaUI/MAUI는 이번 범위 제외, 추후 동일 방식으로 추가 가능)

---

## 2. 핵심 원리: NuGet TFM별 lib 폴드

NuGet 패키지 안에 TFM별 폴드를 두면, 소비자 프로젝트의 TargetFramework에 **가장 잘 맞는 폴드**가 자동 선택된다.

```
Jamesnet.Foundation.<version>.nupkg
├── lib/
│   ├── netstandard2.0/                     ← OpenSilver + 순수 코어 소비자용
│   │   ├── Jamesnet.Foundation.dll
│   │   └── Jamesnet.Platform.OpenSilver.dll
│   ├── net8.0-windows/                     ← WPF용
│   │   ├── Jamesnet.Foundation.dll
│   │   └── Jamesnet.Platform.Wpf.dll
│   ├── net8.0-windows10.0.19041.0/         ← WinUI3용
│   │   ├── Jamesnet.Foundation.dll
│   │   └── Jamesnet.Platform.WinUI3.dll
│   └── uap10.0.19041/                      ← UWP용
│       ├── Jamesnet.Foundation.dll
│       └── Jamesnet.Platform.Uwp.dll
└── Jamesnet.Foundation.nuspec              ← TFM별 dependency group 포함
```

### TFM 선택 규칙 (왜 충돌이 없는가)
- WPF 앱(`net8.0-windows`) → `net8.0-windows` 폴드 선택
- WinUI3 앱(`net8.0-windows10.0.19041.0`) → 더 구체적인 `net8.0-windows10.0.19041.0` 폴드 선택 (NuGet은 "가장 가까운 구체 TFM" 우선)
- UWP 앱(`uap10.0.xxxx`) → `uap10.0.19041` 폴드 선택
- OpenSilver(`netstandard2.0`) → `netstandard2.0` 폴드 선택

### 각 폴드에 Foundation.dll을 함께 넣는 이유
플랫폼 dll이 `Jamesnet.Foundation.dll`을 참조하는데, 같은 패키지 안에서는 자기 자신을 dependency로 둘 수 없으므로 **모든 TFM 폴드에 코어 dll을 중복 포함**한다. (크로스플랫폼 라이브러리의 표준 방식)

---

## 3. TFM별 의존성(dependency group) 설계

nuspec에서 TFM 그룹별로 다른 의존성을 선언한다:

| TFM 그룹 | 의존성 |
|---|---|
| `netstandard2.0` | `System.Text.Json` (8.0.5), `OpenSilver` (3.2.0) |
| `net8.0-windows` | `System.Text.Json` (8.0.5) |
| `net8.0-windows10.0.19041.0` | `System.Text.Json` (8.0.5), `Microsoft.WindowsAppSDK` (1.5.240311000), `Microsoft.Windows.SDK.BuildTools` (10.0.22621.756) |
| `uap10.0.19041` | `System.Text.Json` (8.0.5), `Microsoft.NETCore.UniversalWindowsPlatform` (6.2.14) |

### 알려진 주의점: OpenSilver의 netstandard2.0 겹침
OpenSilver 플랫폼 dll이 코어와 같은 `netstandard2.0` 폴드에 들어가므로, OpenSilver를 쓰지 않는 순수 netstandard2.0 소비자도 `OpenSilver` 패키지 의존성을 물게 된다.
- 영향: 현재 OpenSilver 플랫폼 소비자만 netstandard2.0으로 받으므로 실질 영향은 작음
- 대안이 필요하면: OpenSilver만 별도 패키지로 유지하는 하이브리드도 가능하나, 이번 계획에서는 단일 패키지 원칙을 우선

---

## 4. 구현 방안

### 4-1. 패키징: nuspec + 수집 스크립트 (권장)

`dotnet pack`만으로는 여러 프로젝트 출력을 하나로 묶기 어려우므로 **nuspec 파일**을 사용한다.

1. `build/Jamesnet.Foundation.nuspec` 신규 작성
   - `<files>`에 각 플랫폼 프로젝트의 Release 출력 dll을 TFM별 `lib/` 경로로 매핑
   - 위 표의 dependency group 선언
   - 기존 csproj의 메타데이터(Authors, Version, Tags 등) 이관
2. `build/pack-single.ps1` (또는 bash) 스크립트 작성
   - Foundation/WPF/OpenSilver: `dotnet build -c Release`
   - WinUI3: `dotnet build -c Release` (Windows 필요)
   - UWP: `msbuild /p:Configuration=Release` (구형 프로젝트, Windows + VS 필요)
   - `nuget pack build/Jamesnet.Foundation.nuspec` (또는 `dotnet pack`이 아닌 `NuGet.exe pack`)
3. 로컬 피드(또는 `NuGet.Config`의 로컬 소스)에서 Demo 앱 4종(WPF/OpenSilver/WinUI3/UWP) 설치·빌드·실행 검증

### 4-2. CI 통합 (`.github/workflows/nuget-publish.yml` 수정)

- 기존: Foundation + Platform.WPF만 개별 pack/push, `windows-latest` + .NET 8
- 변경:
  - windows-latest에서 4개 플랫폼 모두 빌드 (UWP는 msbuild 사용 — `microsoft/setup-msbuild` 액션 추가)
  - nuspec으로 단일 nupkg 생성 → 1개만 push
  - 기존 `Jamesnet.Platform.*` push 단계는 제거 또는 유지(병행 배포 기간 동안)

### 4-3. 버전 전략

- 단일 패키지 버전은 4개 플랫폼 중 최대치 이상으로 통일 (현재: Foundation 9.1.13.3, WPF 9.1.13.18, OpenSilver 9.1.13.2, WinUI3 9.1.13)
- **락스텝(lockstep) 버저닝**: 어느 한 플랫폼만 수정돼도 전체 패키지 버전 상승 → 통합의 대가로 수용
- 통합 첫 릴리스는 `9.2.0` 같은 마이너 범프 권장 (소비자에게 변화 신호)

### 4-4. 기존 `Jamesnet.Platform.*` 패키지 처리

1. 전환기(1~2개 마이너 버전): 병행 배포
2. 이후 NuGet.org의 **Deprecate** 기능으로 deprecated 표시, 메시지에 "Jamesnet.Foundation으로 통합됨" 안내
3. README의 설치 안내를 `Install-Package Jamesnet.Foundation` 단일로 갱신

---

## 5. 소비자(leagueoflegends 시리즈) 영향

같은 루트의 `leagueoflegends-{wpf,opensilver,uno,winui3}` 레포가 이 패키지의 주요 소비자.

- 전환 전 확인 사항: 각 레포가 `Jamesnet.Platform.*`을 **NuGet 참조**로 쓰는지 **ProjectReference**로 쓰는지 확인 필요
- NuGet 참조라면: 패키지 하나만 바꾸면 되므로 전환 비용 매우 낮음
- `PackageReference`가 `Jamesnet.Platform.Wpf` → `Jamesnet.Foundation`으로 ID만 바뀌고 using/코드는 동일 (어셈블리·네임스페이스 이름 유지)
- Uno는 이번 통합 범위 밖이므로 `leagueoflegends-uno`는 기존 `Jamesnet.Platform.Uno` 유지

---

## 6. 작업 체크리스트

- [ ] `build/Jamesnet.Foundation.nuspec` 작성 (TFM lib 매핑 + dependency groups)
- [ ] `build/pack-single` 스크립트 작성 (4개 플랫폼 빌드 + pack)
- [ ] 로컬 nupkg 생성 및 내용물 검증 (`lib/` 구조, 의존성)
- [ ] DemoWpfApp / DemoOpenSilverApp에서 로컬 피드로 설치·동작 검증
- [ ] WinUI3/UWP 소비 앱 검증 (Windows 환경 필요)
- [ ] `nuget-publish.yml` CI 수정 (msbuild 스텝 포함)
- [ ] leagueoflegends 레포들의 참조 방식 확인 및 전환
- [ ] README.md 설치 안내 갱신
- [ ] 기존 Platform 패키지 deprecated 처리 계획 수립

---

## 7. 리스크와 트레이드오프

- **버전 락스텝**: 한 플랫폼 수정이 전체 릴리스를 유발 → WPF 사용자도 불필요한 업데이트 알림 수신
- **릴리스 결합도**: 한 플랫폼 빌드 실패 시 전체 배포 블록 (CI에서 플랫폼별 빌드 검증 필수)
- **OpenSilver netstandard2.0 겹침**: 위 3절 참고
- **UWP 빌드 환경**: 구형 프로젝트라 CI에서 msbuild/VS 툴체인 필요, macOS/Linux 로컬 패킹 불가 (UWP 제외 부분 패킹은 가능)
- **패키지 크기 증가**: 모든 플랫폼 dll 포함 — 수백 KB 수준으로 실질 영향은 미미
