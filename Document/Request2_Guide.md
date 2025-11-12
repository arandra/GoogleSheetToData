# Request2 Unity 플러그인 가이드

## 1. 준비사항
- `GSheetToDataCore`와 `SerializableTypes` 프로젝트를 `netstandard2.1` 타겟으로 빌드하여 Unity 프로젝트(`Assets/Plugins` 등)에서 참조 가능하도록 DLL을 복사한다.
- `Google.Apis.Sheets.v4`, `Newtonsoft.Json` DLL도 Unity 프로젝트 내에서 접근 가능해야 한다. (Unity 2021+ 기본 패키지 또는 UPM `com.unity.nuget.newtonsoft-json` 사용)
- `GSheetToDataForUnity/Editor` 폴더를 Unity 프로젝트의 `Assets` 하위에 그대로 복사한다.

## 2. EditorWindow 설정
1. Unity 메뉴 `Tools > GSheetToData > Settings`를 열어 전역 설정을 입력한다.  
   - `스크립트 출력 경로`: 생성된 기초 데이터/ScriptableObject 스크립트를 저장할 `Assets` 하위 경로  
   - `에셋 출력 경로`: ScriptableObject `.asset` 파일을 저장할 `Assets` 하위 경로  
   - `네임스페이스`: 생성되는 두 스크립트에 적용할 namespace  
   - `client_secret.json 경로`, `Token 저장 경로`: Google Sheets API 인증에 필요한 절대경로  
   - `Save Settings` 버튼으로 `ProjectSettings/GSheetToDataSettings.json`에 저장
2. Unity 메뉴 `Tools > GSheetToData > Asset Manager`를 열어 시트를 등록한다.  
   - `Sheet ID`: Google Spreadsheet ID  
   - `Sheet Name`: 시트(탭) 이름 → 기초 데이터 클래스 이름에 그대로 사용  
   - `Sheet Type`: Table / Const  
   - 필요 시 항목별 Override(스크립트 경로, 네임스페이스 등)를 지정한다.

## 3. 생성 절차
1. Asset Manager에서 `Generate / Re-sync` 버튼 클릭  
2. 플러그인이 Google Sheet를 로딩하고 `ClassGenerator`로 기초 데이터 클래스를 생성한다.  
3. 동일한 네임스페이스에 `Pluralize` 규칙이 적용된 ScriptableObject 클래스를 생성하며, `Values` 리스트와 `SheetId/SheetName` 필드를 포함한다.  
4. 두 스크립트 파일은 설정한 스크립트 경로에 저장되고 `AssetDatabase.Refresh()`로 컴파일을 트리거한다.  
5. 파싱된 데이터(JSON 형식)는 작업 큐(`Library/GSheetToDataJobs.json`)에 저장된다.
6. 컴파일이 끝나면 `GSheetToDataJobProcessor`가 새 클래스를 찾아 ScriptableObject 인스턴스를 만들고 `Values` 리스트를 채운 뒤, 지정한 에셋 경로에 `.asset` 파일을 생성/갱신한다.

## 4. 결과물
- `Assets/.../<ClassName>.cs`: 순수 C# 기초 데이터 클래스 (Unity 종속성 없음)
- `Assets/.../<PluralClassName>.cs`: ScriptableObject 클래스 (`Values` 리스트, `SheetId`, `SheetName`)
- `Assets/.../<PluralClassName>.asset`: 데이터가 채워진 ScriptableObject 에셋
- `Library/GSheetToDataJobs.json`: 컴파일 이후 처리할 작업 큐(자동 관리)

## 5. 주의사항 및 팁
- 모든 출력 경로는 반드시 Unity 프로젝트(`Assets`) 내부여야 하며, 절대경로/상대경로 모두 지원한다.
- ScriptableObject 클래스명은 기초 데이터 클래스명을 `ClassGenerator.Pluralize` 규칙으로 변환한 값이다. 예: `ItemData` → `ItemDatas`.
- `Values` 필드는 항상 `List<기초데이터>` 이름으로 유지된다.
- `SheetId`/`SheetName`은 ScriptableObject에 저장되어 나중에 어떤 시트에서 왔는지 추적 가능하다.
- 작업 도중 에디터가 재시작되더라도 `Library/GSheetToDataJobs.json` 덕분에 에셋 생성 큐가 유지된다.
