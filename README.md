# Idle Soccer Club MVP

Unity 기반 모바일 방치형 축구 구단 매니지먼트 MVP 프로젝트 골격입니다.

## 현재 포함된 범위
- 몸풀기 파밍
- 오프라인 보상
- 리그 자동 경기 도전 및 승패 전이
- 선수 레벨업 / 성급 상승
- 선수 스카우트 1회 / 10회
- 시설 4종 업그레이드
- 포메이션 / 전술 / 팀컬러 적용
- 저장 / 불러오기
- 디버그 로그와 결과 패널

## 구조 원칙
- `Assets/Scripts/Core`: 부트스트랩 및 앱 진입점
- `Assets/Scripts/Data`: 상태 모델, 설정 모델, 저장 모델
- `Assets/Scripts/Services`: 인터페이스와 로컬 구현체
- `Assets/Scripts/Systems`: 실제 계산 로직
- `Assets/Scripts/UI`: 런타임 uGUI 구성
- `Assets/Resources/Configs`: JSON 기반 더미 밸런스 데이터

## 라이브 운영 대비
- UI는 서비스 인터페이스만 참조합니다.
- 재화, 진행도, 스카우트는 로컬 구현체 뒤에 숨겨져 있어 향후 서버 구현체로 교체 가능합니다.
- 저장 모델과 도메인 상태 모델을 분리했습니다.

## 열기 방법
1. Unity Hub에서 이 폴더를 프로젝트로 추가합니다.
2. Unity 2021 LTS 이상에서 엽니다.
3. 빈 씬이어도 실행 가능합니다. 런타임 부트스트랩이 자동으로 UI와 시스템을 생성합니다.

## 참고
- 이 환경에는 완전한 Unity Editor 바이너리가 없어서, 텍스트 기반 프로젝트 골격과 코드 자산을 우선 생성했습니다.
- Unity에서 처음 열면 `.meta`, `Library`, `Packages/packages-lock.json` 등은 에디터가 생성합니다.
