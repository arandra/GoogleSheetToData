데이터 처리의 효율성을 위해, enum을 표기 하고 사용할 수 있도록 확장한다.

enum 전용 시트를 구성하여 'enum 이름'.cs파일을 생성하고 enum을 정의한다.

enum 시트의 구성
* 1행 : enum, value
    - enum이나 value가 표기되지 않은 열은 무시한다.
* 2행 : enum의 이름
* 3행 이후 : enum의 값
* 테이블 예 ) 
    enum | value
    UnitType | UnitType
    Monster | 0
    Player | 1
* 코드 예 ) 
    enum UnitType
    {
        Monster = 0,
        Player = 1
    }

하나의 시트에 여러 enum을 정의할 수 있다.
* 예) 1, 2열은 UnitType, 3, 4열은 MonsterType
* 2행이 동일한 enum과 value를 짝지어 정의한다.
* value가 비어 있으면 코드에 value를 강제하지 않는다.

enum만 있고 value는 없는 경우 value는 무시한다.
* 테이블 예 ) 
    enum 
    MonsterType
    Goblin
    Slime
* 코드 예 ) 
    enum MonsterType
    {
        Goblin,
        Slime
    }


Table과 Const에서의 사용
* 필드 타입에 enum('enum 이름')  으로 표기
* 예) 
    enum(MonsterType)
    Target
    Goblin
* 'MonsterType'이 존재하는 지 'Goblin'이 존재하는 지 등의 에러 검사는 하지 않음.