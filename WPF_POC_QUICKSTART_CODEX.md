# Jamesnet.Foundation 단일 NuGet WPF PoC 빠른 실행

> 작성: Codex (OpenAI)  
> 목적: 기존 `Jamesnet.Foundation` + `Jamesnet.Platform.Wpf` 사용 프로젝트를 단일 로컬 `Jamesnet.Foundation` 패키지로 즉시 검증  
> 주의: 이 PoC 패키지는 NuGet.org에 게시하지 않는다.

## 1. 브랜치 내려받기

```powershell
git clone https://github.com/JamesnetGroup/jamesnet.foundation.git
Set-Location jamesnet.foundation
git switch feat/unified-foundation-wpf-poc
git pull
```

이미 저장소가 있다면:

```powershell
git fetch origin
git switch feat/unified-foundation-wpf-poc
git pull
```

## 2. 준비물

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022의 `.NET 데스크톱 개발`
- `nuget.exe`

확인:

```powershell
dotnet --info
nuget help
```

`nuget` 명령이 없으면 <https://www.nuget.org/downloads>에서 `nuget.exe`를 내려받아 PATH에 추가한다.

## 3. WPF PoC 패키지 생성

저장소 루트에서 실행한다.

```powershell
pwsh ./build/pack-wpf-poc.ps1
```

Windows PowerShell을 사용한다면:

```powershell
powershell -ExecutionPolicy Bypass -File .\build\pack-wpf-poc.ps1
```

성공하면 다음 패키지가 생성된다.

```text
artifacts/wpf-poc/Jamesnet.Foundation.10.0.0-wpf-poc.1.nupkg
```

패키지에는 두 DLL이 함께 들어 있다.

```text
lib/net8.0-windows/Jamesnet.Foundation.dll
lib/net8.0-windows/Jamesnet.Platform.WPF.dll
```

## 4. 자동 Smoke Test

```powershell
pwsh ./build/test-wpf-poc.ps1 `
    -PackagePath ./artifacts/wpf-poc/Jamesnet.Foundation.10.0.0-wpf-poc.1.nupkg
```

이 검사는 다음을 자동 확인한다.

- 로컬 패키지 restore
- Foundation DLL 선택
- WPF DLL 선택
- OpenSilver/WinUI 3/UWP DLL 미선택
- 임시 WPF 프로젝트 Release 빌드

## 5. 기존 WPF 프로젝트에 로컬 피드 연결

테스트할 WPF 솔루션에서 `NuGet.Config`에 로컬 패키지 폴더를 추가한다. 절대경로는 실제 clone 경로로 변경한다.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add
      key="Jamesnet WPF PoC"
      value="C:\path\to\jamesnet.foundation\artifacts\wpf-poc" />
    <add
      key="NuGet.org"
      value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
```

CLI로 소스를 추가해도 된다.

```powershell
dotnet nuget add source `
    C:\path\to\jamesnet.foundation\artifacts\wpf-poc `
    --name "Jamesnet WPF PoC" `
    --configfile .\NuGet.Config
```

## 6. PackageReference 변경

기존:

```xml
<PackageReference Include="Jamesnet.Foundation" Version="9.1.13" />
<PackageReference Include="Jamesnet.Platform.Wpf" Version="9.1.13.1" />
```

PoC:

```xml
<PackageReference
    Include="Jamesnet.Foundation"
    Version="10.0.0-wpf-poc.1" />
```

`Jamesnet.Platform.Wpf` 참조는 제거한다. 코드의 기존 `using`과 XAML namespace는 이번 PoC에서 변경하지 않는다. 플랫폼 DLL 이름과 공개 타입은 그대로 포함된다.

솔루션 전체에서 기존 참조를 확인한다.

```powershell
rg -n "Jamesnet\.Foundation|Jamesnet\.Platform\.Wpf" -g "*.csproj"
```

모든 관련 프로젝트가 같은 PoC 버전을 사용해야 한다.

## 7. 캐시를 피하고 Restore/Build

같은 버전으로 패키지를 다시 만들었다면 NuGet 캐시 때문에 이전 파일이 선택될 수 있다. 가능하면 재생성할 때 버전을 올린다.

```powershell
pwsh ./build/pack-wpf-poc.ps1 -Version 10.0.0-wpf-poc.2
```

소비 프로젝트에서:

```powershell
dotnet restore --force --no-cache
dotnet build -c Release --no-restore
```

Visual Studio를 사용하면 솔루션을 다시 열고 Restore 후 Rebuild한다.

## 8. 실행 확인

다음을 확인한다.

- 앱 시작
- 최초 View와 ViewModel 연결
- Container 등록과 Resolve
- Layer Mapping
- 메뉴 및 화면 이동
- ResourceDictionary와 스타일
- Responsive 기능
- Docking/FloatingWindow 사용 시 해당 기능
- 처리되지 않은 예외 없음

## 9. 결과 보고

[Windows 검증 보고서 템플릿](./WINDOWS_VALIDATION_REPORT_TEMPLATE_CODEX.md)을 복사해 작성한다.

```text
reports/windows-validation/YYYY-MM-DD-<commit-short-sha>.md
```

최소한 다음 정보를 남긴다.

- jamesnet.foundation 커밋 SHA
- 테스트한 WPF 프로젝트 커밋 SHA
- 생성한 패키지 버전과 SHA-256
- 변경 전/후 PackageReference
- Restore/Build/실행 결과
- 최초 오류와 로그
- 주요 화면 스크린샷

## 10. 원상복구

PoC 브랜치 또는 테스트 커밋을 버리고 기존 PackageReference로 되돌린다.

```xml
<PackageReference Include="Jamesnet.Foundation" Version="9.1.13" />
<PackageReference Include="Jamesnet.Platform.Wpf" Version="9.1.13.1" />
```

`NuGet.Config`에서 `Jamesnet WPF PoC` 소스를 제거한다.

```powershell
dotnet nuget remove source "Jamesnet WPF PoC" `
    --configfile .\NuGet.Config
```

PoC는 NuGet.org의 기존 패키지를 변경하거나 폐기하지 않는다.
