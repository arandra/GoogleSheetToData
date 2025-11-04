프로젝트 개요

Google sheet로 data class와 resource를 생성한다.

# GSheetToDataCore
- 이 프로젝트는 라이브러리 프로젝트로 다른 실행프로젝트의 요청을 받아 Google sheet에 접속하여 json string를 리턴하거나, cs파일을 생성할 수 있는 class구조가 담긴 data를 리턴한다.
- 요청 받을 때, Google sheet이름, 파일 아이디, 인증에 필요한 json path를 받는다.
- sheet(탭)이름이 class 이름이 된다.
- sheet의 1행은 필드의 type을 결정한다.
- sheet의 2행은 필드의 name을 결정한다.
- sheet의 3행 ~ N행은 실제 데이터이다.
- json은 class의 array 형식이며, array의 크기는 (N -2)이다.
- class name은 Pascal notation을 사용한다.
- field name은 Pascal notation을 사용한다.

# GSheetToDataConsole
- 이 프로젝트에서는 GSheetToDataCore에서 생성한 자산을 기반으로 json 파일을 저장하고 cs파일을 생성한다.

# GSheetToDataForUnity(예정)
- 이 프로젝트에서는 GSheetToDataCore에서 생성한 자산을 기반으로 ScriptableObject asset을 생성한다.