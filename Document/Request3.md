Const 타입 추가 개발

# Table type
- 이미 구현 됨.
- sheet의 1행은 필드의 type을 결정한다.
- sheet의 2행은 필드의 name을 결정한다.
- sheet의 3행 ~ N행은 실제 데이터이다.
- 같은 형식의 여러 데이터를 다루는 용도이다.

# Const type
- sheet의 1열은 필드의 type을 결정한다.
- sheet의 2열은 필드의 name을 결정한다.
- sheet의 3열은 실제 데이터이다.
- 단일 데이터를 다루는 용도 이다.

table 형과 const형은 행, 열이 뒤바뀐다.
table 형은 3행~n행까지 여러 행의 데이터를 다루는 반면, const 형은 3열만 사용한다.

unity의 경우 table과 마찬가지로 const의 순수 데이터 클래스를 사용하는 SO class를 추가로 만든다.
SO클래스의 이름은 table과 같이 plural형태를 사용한다.
SO class의 Value 프로퍼티를 통해 순수 데이터에 접근한다.

