# Jamesnet.Foundation 단일 NuGet 진행 기록

> 기록 담당: Codex (OpenAI)  
> 목적: 구현 판단, 실험 결과, 실패와 해결 과정을 보존하여 검증 및 향후 기술 블로그 재구성에 사용

## 2026-07-26

### 방향 확정

- 공개 NuGet ID를 `Jamesnet.Foundation` 하나로 통합한다.
- WPF, OpenSilver, WinUI 3, UWP를 1차 범위로 한다.
- 소스/빌드 프로젝트는 플랫폼 SDK 경계 때문에 분리 유지한다.
- 사용자 설치 경험과 버전, 릴리스만 하나로 통합한다.
- 정식 NuGet.org 게시 전에는 `10.0.0-preview`/CI 버전으로만 검증한다.
- Codex가 구현을 주도하고 Kimi 문서는 독립 설계 교차검토 자료로 사용한다.

### 원격 체크포인트

| 브랜치 | 커밋 | 내용 |
|---|---|---|
| `feat/unified-foundation-nuget` | `ec75cc2` | 통합 설계, Kimi 계획, Windows 검증 지침/템플릿 |
| `feat/unified-foundation-nuget` | `8772001` | `.nuspec`, Windows 패킹 및 패키지 구조 검사 PoC |
| `feat/unified-foundation-ci` | `3930e0c` | Windows CI 검증 워크플로 템플릿 |

### 구현 내용

- `build/Jamesnet.Foundation.nuspec`
  - `netstandard2.0`: Foundation + OpenSilver
  - `net8.0-windows`: Foundation + WPF
  - `net8.0-windows10.0.19041.0`: Foundation + WinUI 3
  - `uap10.0`: Foundation + UWP
- `build/pack-single.ps1`
  - 네 플랫폼 Release 빌드
  - 필수 산출물 검사
  - 단일 `.nupkg` 생성
  - SHA-256 출력
- `build/test-single-package.ps1`
  - 패키지 필수 파일/TFM 검사
  - Debug/obj/사용자 파일 유입 방지
  - 패키지 ID와 dependency group 검사
- `build/test-package-consumers.ps1`
  - WPF/OpenSilver/WinUI 3 임시 소비 프로젝트 restore
  - `project.assets.json` 기반 올바른 플랫폼 DLL 선택 검사
  - 다른 플랫폼 DLL의 교차 오염 검사

### 확인된 환경 제약

- 현재 macOS 환경에는 `dotnet`과 `pwsh`가 설치되어 있지 않다.
- XML과 YAML 문법 및 `git diff --check`는 macOS에서 검증했다.
- 실제 .NET/PowerShell/MSBuild 실행은 Windows에서 수행해야 한다.
- 시스템 SDK를 임의로 설치하지 않고 Windows 재현 절차를 우선한다.

### GitHub Actions 권한 이슈

현재 HTTPS Git 토큰에 GitHub Actions `workflow` scope가 없어 `.github/workflows` 변경 푸시가 거부됐다.

시도 결과:

1. HTTPS push: workflow scope 부족으로 거부
2. SSH push: 등록된 GitHub 쓰기용 SSH key가 없어 거부
3. 해결: 워크플로를 `build/workflows/unified-nuget-validation.template.yml`로 보관하고 새 원격 단계 브랜치에 푸시

`workflow` 권한이 있는 환경에서 템플릿을 다음 경로로 복사해야 한다.

```text
.github/workflows/unified-nuget-validation.yml
```

이 템플릿에는 NuGet.org 게시 단계가 없으며 빌드, 패킹, 검사, artifact 업로드만 수행한다.

### 아직 검증되지 않은 핵심 가정

1. `uap10.0` dependency group과 UWP PackageReference 복원
2. WinUI 3 소비 프로젝트가 정확히 WinUI 3 TFM 자산을 선택하는지
3. OpenSilver가 `netstandard2.0` 그룹을 독점하면서 일반 Foundation 소비자에게 OpenSilver 의존성을 유입하는 문제
4. 플랫폼 DLL의 XAML 리소스가 통합 패키지에서도 정상 탐색되는지
5. WinUI 3/UWP 실행 Smoke App

### 다음 작업

1. Windows에서 CI 템플릿 설치
2. 패킹 스크립트 최초 실행 및 오류 수정
3. 패키지 소비 자산 검사
4. WPF/OpenSilver Demo를 단일 로컬 패키지로 전환한 Smoke Test
5. WinUI 3/UWP 최소 실행 샘플 추가
6. OpenSilver 의존성 격리 방안 PoC

