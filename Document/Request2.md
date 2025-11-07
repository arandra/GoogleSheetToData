# GSheetToDataForUnity
- 이 프로젝트에서는 GSheetToDataCore에서 생성한 자산을 기반으로 ScriptableObject asset을 생성한다.
- 생성할 파일
  * 기초 데이터 cs파일(unity 종속성 없이 생성한다. ClassGenerator를 활용)
  * ScriptableObject class를 위한 cs파일

- 필수 파라미터 : 경로, 네임스페이스, sheet ID, sheet name, client_secret 경로, token 저장 위치
- 파라미터는 한번 세팅하면 변경할 필요가 없는 것과 자주 변경하는 것은 구분할 필요가 있다.
- 1회성 세팅(저장 및 로딩 필요)은 경로, 네임스페이스, client_secret 경로, token 저장 위치이고, sheet ID, sheet name은 매번 입력받는다.
- sheet ID, sheet name은 ScriptableObject 클래스 에디터에 저장하여 추적가능하도록 한다.
- ScriptableObject 클래스 이름은 순수 ClassName에 Pluralize 적용. 필드명은 Values.
- ScriptableObject 인스턴스는 하나의 리스트(예: List<ItemData> Values)를 들고 있는 단일 에셋으로 만듦.
- 생성된 .asset 파일은 사용자가 지정한 경로에 저장.
