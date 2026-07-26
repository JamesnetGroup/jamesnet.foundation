# Jamesnet.Foundation Windows 검증 보고서

> 이 파일을 직접 덮어쓰지 말고  
> `reports/windows-validation/YYYY-MM-DD-<commit-short-sha>.md`로 복사하여 작성한다.

## 1. 식별 정보

| 항목 | 값 |
|---|---|
| 검증 일시 | 미작성 |
| 검증 담당자 | 미작성 |
| 컴퓨터/CI | 미작성 |
| Git 브랜치 | 미작성 |
| 전체 Commit SHA | 미작성 |
| 검증 대상 패키지 버전 | 미작성 또는 해당 없음 |
| 보고서 커밋 SHA | 작성 후 기입 |

## 2. Windows 및 도구 환경

| 항목 | 값 |
|---|---|
| Windows 버전/빌드 | 미작성 |
| 시스템 아키텍처 | 미작성 |
| Visual Studio 버전 | 미작성 |
| MSBuild 버전 | 미작성 |
| .NET SDK | 미작성 |
| .NET Runtime | 미작성 |
| Windows SDK | 미작성 |
| PowerShell 버전 | 미작성 |
| NuGet CLI 버전 | 미작성 또는 미사용 |

`dotnet --info` 요약:

```text
미작성
```

설치된 Windows SDK:

```text
미작성
```

## 3. 시작 시 Git 상태

```text
git status --short 결과
```

검증 전 사용자 변경사항:

```text
없음 또는 파일과 보존 방법 기록
```

## 4. 기준선 빌드 결과

결과 값은 `성공`, `실패`, `미실시` 중 하나를 사용한다.

| 대상 | Restore | Release Build | 실행 | 종료 코드 | 로그/비고 |
|---|---:|---:|---:|---:|---|
| Foundation | 미작성 | 미작성 | 해당 없음 | 미작성 | 미작성 |
| WPF 라이브러리 | 미작성 | 미작성 | 해당 없음 | 미작성 | 미작성 |
| WPF 데모 | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| OpenSilver 라이브러리 | 미작성 | 미작성 | 해당 없음 | 미작성 | 미작성 |
| OpenSilver Browser | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| OpenSilver Simulator | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| WinUI 3 라이브러리 | 미작성 | 미작성 | 샘플 없음 | 미작성 | 미작성 |
| UWP AnyCPU | 미작성 | 미작성 | 샘플 없음 | 미작성 | 미작성 |
| UWP x86 | 미작성 | 미작성 | 샘플 없음 | 미작성 | 미작성 |
| UWP x64 | 미작성 | 미작성 | 샘플 없음 | 미작성 | 미작성 |
| UWP ARM | 미작성 | 미작성 | 샘플 없음 | 미작성 | 미작성 |
| UWP ARM64 | 미작성 | 미작성 | 샘플 없음 | 미작성 | 미작성 |

## 5. 실행 확인

### WPF

- [ ] 메인 창 표시
- [ ] View–ViewModel 연결
- [ ] 메뉴/View 전환
- [ ] Layer Mapping
- [ ] Responsive/Docking 핵심 기능
- [ ] 처리되지 않은 예외 없음

비고:

```text
미작성
```

### OpenSilver Browser

- [ ] 앱 로딩
- [ ] 메인 View 표시
- [ ] View 전환
- [ ] Responsive 기능
- [ ] 새로고침
- [ ] 브라우저 콘솔 오류 없음

비고:

```text
미작성
```

### OpenSilver Simulator

- [ ] 앱 로딩
- [ ] 메인 View 표시
- [ ] 핵심 상호작용
- [ ] 처리되지 않은 예외 없음

비고:

```text
미작성
```

## 6. 단일 NuGet 결과

구현 전 기준선 검증이라면 `해당 없음`으로 표시한다.

| 항목 | 값 |
|---|---|
| 패키지 파일명 | 미작성 |
| 패키지 ID | 미작성 |
| 패키지 버전 | 미작성 |
| 파일 크기 | 미작성 |
| SHA-256 | 미작성 |
| 생성 명령 | 미작성 |
| 생성 결과 | 미작성 |

패키지 내부 주요 파일:

```text
미작성
```

Dependency group:

```text
미작성
```

의도하지 않은 Debug/obj/비밀정보 포함 여부:

```text
미작성
```

## 7. 소비 프로젝트 자산 선택

| 소비 플랫폼 | Foundation | 선택된 플랫폼 구현 | 잘못 포함된 구현 | 결과 |
|---|---|---|---|---|
| WPF | 미작성 | 미작성 | 미작성 | 미작성 |
| OpenSilver | 미작성 | 미작성 | 미작성 | 미작성 |
| WinUI 3 | 미작성 | 미작성 | 미작성 | 미작성 |
| UWP | 미작성 | 미작성 | 미작성 | 미작성 |

`project.assets.json` 분석:

```text
플랫폼별 compile/runtime/frameworkReferences 결과를 요약
```

OpenSilver가 아닌 `netstandard2.0` 소비자의 OpenSilver 의존성 유입 여부:

```text
미작성
```

## 8. XAML 및 리소스 검증

| 플랫폼 | XAML 컴파일 | Generic.xaml | ResourceDictionary | 디자인 타임 | 런타임 |
|---|---:|---:|---:|---:|---:|
| WPF | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| OpenSilver | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| WinUI 3 | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| UWP | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |

비고:

```text
미작성
```

## 9. League of Legends 회귀 검증

| 저장소 | 대상 SHA | Restore | Build | 실행 | 핵심 화면 | 결과 |
|---|---|---:|---:|---:|---:|---|
| leagueoflegends-wpf | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| leagueoflegends-opensilver | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |
| leagueoflegends-winui3 | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 | 미작성 |

검증한 화면/기능:

```text
미작성
```

## 10. 실패 상세

실패가 없으면 `없음`으로 작성한다.

### 실패 1

명령:

```powershell
미작성
```

종료 코드:

```text
미작성
```

최초 오류:

```text
미작성
```

오류 전후 로그:

```text
미작성
```

재현 여부:

```text
미작성
```

추정 원인과 미확인 사항:

```text
미작성
```

## 11. 경고

새로 발생한 경고:

```text
없음 또는 경고 코드/발생 프로젝트/횟수
```

기존 경고와 구분한 방법:

```text
미작성
```

## 12. 첨부 자료

| 자료 | 파일 또는 CI artifact | SHA-256/비고 |
|---|---|---|
| 빌드 로그 | 미작성 | 미작성 |
| 패키지 파일 목록 | 미작성 | 미작성 |
| project.assets.json | 미작성 | 미작성 |
| WPF 스크린샷 | 미작성 | 개인정보 확인 |
| OpenSilver 스크린샷 | 미작성 | 개인정보 확인 |
| WinUI 3 스크린샷 | 미작성 | 미작성 |
| UWP 스크린샷 | 미작성 | 미작성 |

## 13. 종합 판정

다음 중 하나를 선택한다.

- [ ] 통과
- [ ] 조건부 통과
- [ ] 실패
- [ ] 환경 미충족

판정 근거:

```text
미작성
```

차단 이슈:

```text
없음 또는 이슈 목록
```

권장 다음 작업:

```text
미작성
```

## 14. 검증 후 Git 상태

```text
git status --short 결과
```

검증 중 생성하거나 변경한 파일:

```text
미작성
```

검증 담당자 확인:

```text
본 보고서에는 비밀정보가 없으며, 실행하지 않은 항목을 성공으로 표시하지 않았습니다.
이름/날짜: 미작성
```
