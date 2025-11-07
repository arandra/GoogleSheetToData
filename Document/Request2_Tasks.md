# Request2 작업 리스트

1. **코어 라이브러리 호환성 검토**
   - GSheetToDataCore 및 SerializableTypes가 Unity(Editor)에서도 참조 가능하도록 대상 프레임워크/의존성 확인
   - 필요한 경우 멀티 타겟팅 등 조정 사항 정리

2. **Unity Editor 플러그인 구현**
   - `GSheetToDataForUnity/Editor` 폴더 구성
   - 1회성 설정(AppSetting) 저장/로드 로직 구현(경로, 네임스페이스, client_secret 경로, token 저장 위치)
   - EditorWindow UI에서 Sheet ID, Sheet Name 입력 및 ScriptableObject 클래스에 기록
   - Sheet 로딩 → ClassGenerator 활용 C# 데이터 클래스 생성 → ScriptableObject 클래스 및 `.asset` 생성 파이프라인 구현
   - 에셋/스크립트 저장 경로는 사용자 입력값 활용

3. **문서화**
   - Unity 플러그인 설치/사용 가이드 작성
   - 설정 방법, 실행 순서, 생성 산출물(스크립트/에셋) 경로 안내
