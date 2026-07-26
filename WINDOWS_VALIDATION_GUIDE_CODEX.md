# Jamesnet.Foundation Windows 빌드·검증 지침

> 작성: Codex (OpenAI)  
> 작성일: 2026-07-26  
> 대상: Windows 환경에서 WPF, OpenSilver, WinUI 3, UWP 빌드 및 단일 NuGet을 검증하는 작업자  
> 관련 설계: [JAMESNET_FOUNDATION_UNIFIED_NUGET_CODEX.md](./JAMESNET_FOUNDATION_UNIFIED_NUGET_CODEX.md)

## 1. 목적

macOS에서 작성한 변경을 Windows에서 재현하고, 플랫폼별 빌드·XAML 컴파일·패키지 선택·실행 결과를 Codex가 검토할 수 있는 형태로 남기는 절차다.

검증 결과는 반드시 다음 템플릿을 복사하여 저장한다.

```text
WINDOWS_VALIDATION_REPORT_TEMPLATE_CODEX.md
    ↓ 복사
reports/windows-validation/YYYY-MM-DD-<commit-short-sha>.md
```

단순히 "성공" 또는 "실패"라고 기록하지 않는다. 사용한 커밋, SDK, 명령, 종료 코드, 첫 오류, 산출물과 패키지 자산을 함께 기록해야 한다.

## 2. 현재 가능한 검증 범위

현재 저장소에는 다음 실행 샘플이 있다.

- WPF: `src/DemoWpfApp`
- OpenSilver Browser: `src/DemoOpenSilverApp/DemoOpenSilverApp.Browser`
- OpenSilver Simulator: `src/DemoOpenSilverApp/DemoOpenSilverApp.Simulator`

현재 저장소에는 WinUI 3와 UWP 실행 샘플이 없다. 따라서 현 단계에서는 두 플랫폼의 라이브러리 빌드만 확인할 수 있다. 단일 NuGet 구현 단계에서는 최소 WinUI 3/UWP Smoke App을 추가해야 XAML 및 런타임 검증이 완료된다.

## 3. 권장 Windows 환경

### 운영체제

- Windows 11 x64 권장
- Windows 10을 사용한다면 저장소의 최소 Windows 버전과 설치 SDK를 확인
- 관리자 권한은 SDK 설치에만 사용하고 일반 빌드에는 사용하지 않음

### 필수 도구

Visual Studio 2022 Installer에서 다음 워크로드/구성 요소를 설치한다.

- .NET 데스크톱 개발
- 유니버설 Windows 플랫폼 개발
- Windows 앱 SDK/WinUI 관련 C# 도구
- MSBuild
- Windows 10 SDK 10.0.19041.0
- 저장소가 요구하는 최신 Windows SDK 및 BuildTools
- .NET 8 SDK
- .NET 9 SDK

추가 도구:

- Git
- PowerShell 7 권장
- NuGet CLI가 필요한 패킹 단계에서는 `nuget.exe`

현재 프로젝트의 주요 요구사항:

| 대상 | 요구사항 |
|---|---|
| Foundation | `netstandard2.0`, C# 12 |
| WPF 라이브러리 | .NET 8 + WPF |
| WPF 데모 | .NET 9 + WPF |
| OpenSilver 라이브러리 | OpenSilver 3.2.0 |
| OpenSilver Browser | .NET 9 + OpenSilver WebAssembly |
| OpenSilver Simulator | .NET 9 Windows |
| WinUI 3 | .NET 8, Windows App SDK 1.5 계열 |
| UWP | MSBuild, UWP 도구, Windows SDK 10.0.19041.0 |

## 4. 보안 및 작업 원칙

- NuGet API 키, GitHub 토큰, 인증서 비밀번호를 보고서나 로그에 기록하지 않는다.
- 정식 NuGet.org 게시 명령은 명시적인 승인 없이 실행하지 않는다.
- 검증 중 소스 코드를 임의로 수정하지 않는다.
- 실패를 숨기기 위해 경고를 비활성화하거나 프로젝트 설정을 바꾸지 않는다.
- 기존 작업 파일을 삭제하거나 `git reset --hard`, `git clean -fd`를 실행하지 않는다.
- 패키지 검증은 NuGet.org가 아닌 로컬 폴더 피드를 우선 사용한다.
- 각 명령의 종료 코드를 기록한다.

## 5. 시작 전 저장소 상태 기록

Visual Studio의 **Developer PowerShell for VS 2022**를 열고 저장소 루트로 이동한다.

```powershell
Set-Location C:\path\to\jamesnet.foundation
git status --short
git branch --show-current
git rev-parse HEAD
git log -1 --oneline
```

다음 조건을 확인한다.

- 검증하려는 커밋 SHA가 사용자/Codex가 지정한 SHA와 같음
- 의도하지 않은 수정 파일이 없음
- 서브모듈을 도입했다면 해당 상태도 기록

환경 정보를 수집한다.

```powershell
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
msbuild -version
git --version
```

UWP 빌드에 필요한 SDK 경로도 확인한다.

```powershell
Get-ChildItem 'C:\Program Files (x86)\Windows Kits\10\Lib' |
    Select-Object -ExpandProperty Name
```

`10.0.19041.0`이 없으면 UWP 검증을 시작하지 말고 보고서에 환경 미충족으로 기록한다.

## 6. 검증 결과 디렉터리

보고서만 Git에 커밋하는 것이 기본이다. 원시 로그가 필요하면 다음 경로에 보관한다.

```text
artifacts/windows-validation/<commit-short-sha>/
```

로그가 지나치게 크면 Git에 넣지 않고 CI artifact로 업로드한 뒤 보고서에 artifact 이름과 SHA-256을 기록한다.

PowerShell에서 작업 디렉터리를 준비하는 예:

```powershell
$CommitSha = git rev-parse --short HEAD
$ResultRoot = Join-Path $PWD "artifacts\windows-validation\$CommitSha"
New-Item -ItemType Directory -Force -Path $ResultRoot | Out-Null
```

## 7. 기준선 빌드

단일 NuGet 구현 전후를 비교하려면 먼저 기존 프로젝트들의 기준선 빌드를 수행한다.

### 7.1 Foundation

```powershell
dotnet restore src/Jamesnet.Foundation/Jamesnet.Foundation.csproj
dotnet build src/Jamesnet.Foundation/Jamesnet.Foundation.csproj `
    -c Release --no-restore
```

확인:

- 종료 코드 `0`
- `Jamesnet.Foundation.dll` 생성
- 컴파일 오류 없음

### 7.2 WPF 라이브러리

```powershell
dotnet restore src/Jamesnet.Platform.WPF/Jamesnet.Platform.WPF.csproj
dotnet build src/Jamesnet.Platform.WPF/Jamesnet.Platform.WPF.csproj `
    -c Release --no-restore
```

확인:

- WPF XAML/ResourceDictionary 컴파일 성공
- Foundation 참조 성공
- `Jamesnet.Platform.WPF.dll` 생성

### 7.3 WPF 데모

```powershell
dotnet restore src/DemoWpfApp/DemoWpfApp.csproj
dotnet build src/DemoWpfApp/DemoWpfApp.csproj `
    -c Release --no-restore
dotnet run --project src/DemoWpfApp/DemoWpfApp.csproj -c Release
```

실행 확인:

- 메인 창이 열림
- 최초 화면이 표시됨
- 메뉴 이동/View–ViewModel 연결 동작
- Layer Mapping 동작
- 콘솔/이벤트 뷰어에 처리되지 않은 예외가 없음

앱은 수동으로 종료하고 보고서에 실행 확인 항목을 기록한다.

### 7.4 OpenSilver 라이브러리

```powershell
dotnet restore src/Jamesnet.Platform.OpenSilver/Jamesnet.Platform.OpenSilver.csproj
dotnet build src/Jamesnet.Platform.OpenSilver/Jamesnet.Platform.OpenSilver.csproj `
    -c Release --no-restore
```

확인:

- `Generic.xaml` 컴파일 성공
- OpenSilver 3.2.0 복원 성공
- 플랫폼 DLL 생성

### 7.5 OpenSilver Browser

```powershell
dotnet restore `
    src/DemoOpenSilverApp/DemoOpenSilverApp.Browser/DemoOpenSilverApp.Browser.csproj
dotnet build `
    src/DemoOpenSilverApp/DemoOpenSilverApp.Browser/DemoOpenSilverApp.Browser.csproj `
    -c Release --no-restore
dotnet run `
    --project src/DemoOpenSilverApp/DemoOpenSilverApp.Browser/DemoOpenSilverApp.Browser.csproj `
    -c Release
```

터미널에 표시된 로컬 URL을 브라우저에서 열고 확인한다.

- 앱 로딩 완료
- 브라우저 콘솔 오류 없음
- 메인 View 표시
- View 전환 및 Responsive 기능 확인
- 새로고침 후에도 정상 로드

### 7.6 OpenSilver Simulator

```powershell
dotnet restore `
    src/DemoOpenSilverApp/DemoOpenSilverApp.Simulator/DemoOpenSilverApp.Simulator.csproj
dotnet build `
    src/DemoOpenSilverApp/DemoOpenSilverApp.Simulator/DemoOpenSilverApp.Simulator.csproj `
    -c Release --no-restore
dotnet run `
    --project src/DemoOpenSilverApp/DemoOpenSilverApp.Simulator/DemoOpenSilverApp.Simulator.csproj `
    -c Release
```

Browser 테스트와 같은 주요 화면을 확인한다.

### 7.7 WinUI 3 라이브러리

```powershell
dotnet restore src/Jamesnet.Platform.WinUI3/Jamesnet.Platform.WinUI3.csproj
dotnet build src/Jamesnet.Platform.WinUI3/Jamesnet.Platform.WinUI3.csproj `
    -c Release --no-restore
```

확인:

- Windows App SDK 복원
- WinUI XAML 컴파일
- x86/x64/ARM64 관련 오류 없음
- 플랫폼 DLL 생성

현재 실행 앱이 없으므로 런타임 검증은 `미실시(샘플 없음)`로 기록한다. 빌드 성공을 런타임 성공으로 표시하면 안 된다.

### 7.8 UWP 라이브러리

Developer PowerShell에서 실행한다.

```powershell
msbuild src/Jamesnet.Platform.UWP/Jamesnet.Platform.Uwp.csproj `
    /restore `
    /t:Rebuild `
    /p:Configuration=Release `
    /p:Platform=AnyCPU `
    /m
```

필요하면 지원 아키텍처도 각각 검증한다.

```powershell
$Platforms = 'x86', 'x64', 'ARM', 'ARM64'
foreach ($PlatformName in $Platforms) {
    msbuild src/Jamesnet.Platform.UWP/Jamesnet.Platform.Uwp.csproj `
        /restore `
        /t:Rebuild `
        /p:Configuration=Release `
        /p:Platform=$PlatformName `
        /m

    if ($LASTEXITCODE -ne 0) {
        throw "UWP $PlatformName build failed with exit code $LASTEXITCODE"
    }
}
```

현재 실행 앱이 없으므로 런타임 검증은 `미실시(샘플 없음)`로 기록한다.

## 8. 단일 NuGet PoC 검증

패키징 프로젝트와 스크립트가 추가된 후 수행한다. 정확한 스크립트 이름은 구현 결과를 따른다. 임의로 NuGet.org에 게시하지 않는다.

예상 흐름:

```powershell
# 예시이며 실제 구현된 스크립트 이름을 우선한다.
pwsh ./build/pack-single.ps1 -Configuration Release
```

### 8.1 패키지 파일 확인

```powershell
Get-ChildItem -Recurse -Filter 'Jamesnet.Foundation*.nupkg'
```

하나의 통합 버전에 대해 기본 `.nupkg`이 정확히 하나인지 확인한다. Symbol 패키지를 생성했다면 `.snupkg`은 별도로 허용한다.

SHA-256을 기록한다.

```powershell
Get-FileHash path\to\Jamesnet.Foundation.10.0.0-preview.1.nupkg `
    -Algorithm SHA256
```

### 8.2 패키지 내부 구조 검사

`.nupkg`은 ZIP이므로 임시 경로에 압축을 풀어 확인한다.

```powershell
$PackagePath = 'path\to\Jamesnet.Foundation.10.0.0-preview.1.nupkg'
$InspectPath = Join-Path $ResultRoot 'package-inspection'
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory(
    $PackagePath,
    $InspectPath
)
Get-ChildItem -Recurse $InspectPath |
    Select-Object FullName, Length
```

확인:

- 공통 Foundation DLL 존재
- WPF/OpenSilver/WinUI 3/UWP 구현 DLL 존재
- Debug 출력이 포함되지 않음
- `.nuspec` dependency group 확인
- `build`/`buildTransitive` 자산이 설계대로 존재
- 비밀정보와 로컬 절대경로가 포함되지 않음
- 불필요한 `obj`, 캐시, 사용자 설정 파일이 없음

### 8.3 로컬 피드 생성

```powershell
$LocalFeed = Join-Path $ResultRoot 'local-feed'
New-Item -ItemType Directory -Force -Path $LocalFeed | Out-Null
Copy-Item $PackagePath $LocalFeed
```

소비 앱은 테스트용 임시 폴더 또는 전용 Smoke App에서 로컬 피드만 우선 사용한다. 기존 프로젝트의 패키지 참조를 바꿨다면 변경 내용을 반드시 보고서에 기록하고 별도 커밋으로 관리한다.

### 8.4 플랫폼별 Restore 자산 검사

각 소비 앱에서 restore 후 `obj/project.assets.json`을 보관한다.

통합 패키지 자동 검사는 다음 스크립트를 사용한다.

```powershell
./build/test-package-consumers.ps1 `
    -PackagePath path\to\Jamesnet.Foundation.10.0.0-preview.1.nupkg `
    -EvidenceDirectory artifacts\validation\consumer-assets
```

이 스크립트는 임시 WPF/OpenSilver/WinUI 3 소비 프로젝트를 만들고 restore한 뒤, 각 `project.assets.json`에 필요한 구현만 선택되었는지 검사한다. UWP는 구형 프로젝트 시스템과 Windows SDK가 필요하므로 별도 Smoke App 단계에서 검증한다.

검사 목표:

- WPF: WPF 구현만 선택
- OpenSilver: OpenSilver 구현만 선택
- WinUI 3: WinUI 3 구현만 선택
- UWP: UWP 구현만 선택
- Foundation 공통 DLL 정상 선택
- 다른 XAML 플랫폼 구현이 Compile/Runtime 자산에 들어오지 않음

PowerShell 예:

```powershell
Select-String `
    -Path path\to\consumer\obj\project.assets.json `
    -Pattern 'Jamesnet|OpenSilver|WindowsAppSDK|UniversalWindowsPlatform'
```

단순 문자열 존재만으로 합격 처리하지 않는다. 해당 패키지가 `compile`, `runtime`, `frameworkReferences` 중 어디에 들어왔는지 확인한다.

## 9. 반드시 수행할 교차 오염 테스트

| 소비 프로젝트 | 있어야 하는 구현 | 없어야 하는 구현 |
|---|---|---|
| WPF | WPF | OpenSilver, WinUI 3, UWP |
| OpenSilver | OpenSilver | WPF, WinUI 3, UWP |
| WinUI 3 | WinUI 3 | WPF, OpenSilver, UWP |
| UWP | UWP | WPF, OpenSilver, WinUI 3 |

OpenSilver는 특히 주의한다. 단일 패키지의 `netstandard2.0` 그룹에 OpenSilver 의존성을 무조건 넣으면 일반 Foundation 소비자까지 OpenSilver 자산을 받게 된다. 이 결과는 성공으로 숨기지 말고 보고서의 설계 이슈로 기록한다.

## 10. League of Legends 회귀 검증

단일 패키지 PoC가 최소 샘플에서 통과한 뒤 수행한다.

기준:

- `leagueoflegends-wpf`가 최초 기준 구현
- OpenSilver 및 다른 플랫폼은 마이그레이션 결과
- 각 저장소의 현재 수정사항을 보존
- 패키지 참조 변경은 독립 브랜치/커밋으로 관리

확인 항목:

- Restore 및 Release Build
- 앱 시작
- Main/Home/Navigate/Profile/Collection/Shop/TFT 등 핵심 View
- View–ViewModel 매핑
- Container 등록/Resolve
- Layer Mapping
- ResourceDictionary 및 이미지/폰트
- Responsive 동작
- 플랫폼별 예외

현재 동일 루트에서 확인된 저장소 이름은 다음과 같다.

```text
leagueoflegends-wpf
leagueoflegends-opensilver
leagueoflegends-uno
leagueoflegends-winui3
```

이번 1차 범위에는 Uno가 포함되지 않는다. `leagueoflegends-uno`는 기존 기준선만 보존하고 통합 전환 대상으로 처리하지 않는다.

## 11. 실패 기록 방법

실패 시 전체 로그만 붙이지 말고 다음 순서로 정리한다.

1. 실패한 정확한 명령
2. 종료 코드
3. 최초 오류 코드와 메시지
4. 최초 오류 전후 약 30줄
5. 같은 오류가 재현되는지
6. 해당 단계 이전 빌드가 성공했는지
7. 설치된 SDK/워크로드
8. 추정 원인과 아직 확인하지 않은 사항

오류가 연쇄적으로 발생하면 가장 첫 오류를 우선한다. 마지막에 나온 다수의 `type not found` 오류만 보고하지 않는다.

## 12. 스크린샷과 로그

스크린샷에 포함할 것:

- 실행된 앱과 확인한 화면
- 앱 버전/커밋을 식별할 수 있는 정보가 있으면 함께 표시
- 오류가 UI에서만 보일 경우 오류 화면

스크린샷에 포함하지 말 것:

- API 키
- 개인 계정/이메일
- 사설 서버 주소
- 인증 토큰
- 불필요한 바탕화면 개인정보

로그 파일 이름 예:

```text
foundation-build.log
wpf-library-build.log
wpf-demo-build.log
opensilver-browser-build.log
opensilver-simulator-build.log
winui3-library-build.log
uwp-anycpu-build.log
package-file-list.txt
wpf-project-assets.json
```

## 13. 결과 제출 절차

1. 템플릿으로 보고서 작성
2. 보고서의 모든 `미작성` 항목을 완료 또는 `미실시 + 이유`로 변경
3. 비밀정보 포함 여부 확인
4. `git diff --check` 실행
5. 보고서와 필요한 작은 로그만 커밋
6. 커밋 SHA를 Codex에게 전달
7. 큰 로그는 CI artifact 이름/URL과 SHA-256을 전달

권장 커밋 메시지:

```text
docs: add Windows validation report for <short-sha>
```

Codex는 보고서를 검토할 때 다음을 확인한다.

- 대상 커밋과 환경 일치
- 명령과 종료 코드
- 플랫폼별 자산 선택
- 빌드와 런타임 결과의 구분
- OpenSilver 의존성 오염
- XAML 리소스 및 실제 앱 동작
- 재현 가능한 실패 정보

## 14. 완료 판정

### 기준선 완료

- Foundation/WPF/OpenSilver/WinUI 3/UWP 라이브러리 Release 빌드 결과 기록
- WPF/OpenSilver 실행 결과 기록
- WinUI 3/UWP 실행 미검증 사유 명시

### 단일 NuGet PoC 완료

- 단일 `.nupkg` 생성
- 패키지 내부 구조와 SHA-256 기록
- 네 플랫폼 Restore/Build
- 플랫폼별 자산 선택 검사
- WPF/OpenSilver 실행
- WinUI 3/UWP Smoke App 추가 후 실행
- 교차 오염 없음 또는 알려진 문제와 해결 계획 기록

### 정식 배포 준비 완료

- League of Legends 대상 앱 회귀 검증
- Windows CI에서 깨끗한 환경 재현
- 프리뷰 패키지 검증
- 마이그레이션 문서 완료
- 사용자의 명시적 배포 승인

## 15. CI 워크플로 설치

검증 워크플로 원본은 다음 템플릿에 있다.

```text
build/workflows/unified-nuget-validation.template.yml
```

현재 macOS Git 인증 토큰에는 GitHub Actions workflow 수정 권한이 없으므로 템플릿 형태로 보관한다. `workflow` 권한이 있는 Git 클라이언트에서 다음 위치로 복사하고 커밋한다.

```text
.github/workflows/unified-nuget-validation.yml
```

이 워크플로는 Windows에서 빌드·패킹·패키지 구조 검사와 artifact 업로드만 수행한다. NuGet.org 게시 단계는 포함하지 않는다.
