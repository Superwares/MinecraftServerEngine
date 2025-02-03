# Minecraft Server Engine
The Minecraft Server Engine is a powerful and versatile game engine designed to significantly enhance the gameplay experience in Minecraft.

## Features

### Object-Oriented Game Development
* Not ECS
* Not Event Handler

### Parallel Computing
One of the standout features of the Minecraft Server Engine is its support for parallel computing, 
which significantly enhances server performance and scalability.
* Non-blocking Network

### No Limit Ticks
- 기존 버킨 구현은 어떤 오브젝트를 탐색할때 오차를 가정하고 탐색을 해야됨.  (50 ms 씩 ticks 로 제한이 있으니 ) 
	- 예를 들어, 단순한 attack 구현에서 ray 를 이용해서 앞의 오브젝트를 찾는데도 플레이어들이 속도가 빠르게 움직이면 정확히 ray 에 걸친 object 를 찾기가 어려움. 그러므로 오차를 가정하고 탐색해야되는 해결책이 있음.

### Continuous Physics

### Single World

### Dynamic Entity Appearance (yet)
다른 Entity 또는 블럭으로 변환 가능 (Hitbox 는 변하지 않음))

### Player Skin Morphing (yet)

### Ghost Players
* 게임에 접속을 끊은 플레이어가 월드에서 사라지지 물리 작용을 받을 수 있는 오브젝트로 남아있음.

### .Net Core
.NET > JVM


## Code Conventions

### Types

The following list is available value types.

* bool, sbyte, byte, short, ushort, int, uint, long, ulong, char, float, double

The following list is available reference types.

* object, string

###
레퍼런스 타입은 레퍼런스 비교 (class)
Value Type 은 값 비교 (Built-in types, structures)
예외 (string)

#### Using Float & Double

Always use the F suffix for float literals and the D suffix for double literals to explicitly denote the type.

```c#
float floatValue = 3.14F;
double doubleValue = 3.14D;
```

###
<ImplicitUsings>disable</ImplicitUsings>
<Nullable>disable</Nullable>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<WarningsNotAsErrors>0162</WarningsNotAsErrors>

###
? 또는 ! 를 사용하지 않습니다.
```c#
Time? t;
Time t2 = t!;
```

Value Type에는 Nullable 을 사용하지 않습니다.

### Exceptions

In case of exceptions that must be thrown, they must be documented using XML tags on the methods,
except the exception "System.NotImplementedException()".
All exceptions must be handled without exception, except for the exception 'System.NotImplementedException()', 
which should not be handled as users need to be notified for confirmation.

Throwing exceptions is not allowed except for network-related functions, but the exception "System.NotImplementedException()" can be used anywhere.
This means that any network operations that may potentially throw exceptions should be handled appropriately, 
while exceptions should not be used for other purposes within the codebase.

Throwing exceptions are restricted to network-related functions due to several reasons. 
First, network conditions are often unpredictable, 
and errors like connection timeouts or server unavailability can occur unexpectedly. 
By allowing exceptions specifically for network operations, developers can handle these unforeseen circumstances more effectively.
Furthermore, excessive and indiscriminate use of exceptions can significantly impact performance, especially during debugging. 
When exceptions are thrown frequently outside of network-related contexts, 
it becomes more challenging to monitor and debug code effectively. 
Focusing exceptions on network operations helps developers to isolate and address issues more efficiently without sacrificing overall performance.
Overall, restricting exceptions to network-related functions enhances code clarity, improves performance, and allows for better management of unpredictable network conditions.

An additional reason for limiting exception handling is to eliminate the need for handling exceptional situations, 
such as with try-finally constructs, when using concurrency primitives.

### Dispose Pattern

#### Disposable Instances

Disposable instances must be handled by 'using' statement except members of class and struct.
If disposable objects were used as members of class and struct, they must be disposed at the current object Dispose(bool disposing) fucntion.

## Notes

### TODO
- Buffer 의 expandData 에서 capNew == buf.cap 일시 함수가 끝나야됨.
- PlayerList.Connect 이 실행되기 전에 PlayerList.Add 되는 문제
	- 같은 userId 와 username 으로 접속한 client 가 중복되지 않도록 방지하면 해결될 것 같음.
- Abstract Player 의 Guest 와 SuperPlayer 두 객체를 넘어갈 수 있도록 구현

- CanJoinWorld 는 이미 존재하는 player 객체에 연결하는 것이면 넘어가기. 새로운 client 에만 나가기

- 같은 username 과 userId 로 접속시 튕기도록
- BossBar 은 Player 개인 또는 World 에 할당할 수 있도록
- y < 0 으로 떨어지면 Arrert 해결하기
- Connection 대신 AbstractPlayer 안에서 Connection 에서의 로직을 모두 처리하기
	- 따로 분리해서 코드 중복이 늘어남
- 룰렛 돌릴때 (random seeker, winners) 에서 띵띵띵 소리 추가
	- abstract player 전용 play sound 기능 추가.
- 술래 사망시 라운드가 종료되는가. 
	- Progress Bar 가 잘 닫히는지
	- Title 이 잘띄어지는지
	- 중간에 사망하여도 잘 진행되는가

- Block Appearance 일떄 움직이면 이전 블럭은Air 로 바뀌는데 Air 가 아닌 해당 위치의 블럭으로 바뀌어야됨.
	- PhysicsWorld 가 World 일때만 블럭으로 변할수 있도록. 
		- World 에만 BlockContext가 있기에	``
- 어떤 플레이어의 인벤토리를 강제로 닫을 수 있도록.


- 클라이언트가 처음 접속할떄 느려짐
	- 아마 청크 로드와 관련있음.
		- region 파일 불러오고 청크 데이터로 만들때 BITS_PER_BLOCK 을 최대로 하여 처리함
		- 이를 최적화필요

- 게임 시작시 떨어진 모든 아이템을 정리하기

- 아이템을 Give 할때 이미 존재하는 slot 을 수색하고 꽉 채우기

- 아이템 구매하면 핫바로 가도록.

- turn off debug console print...

- Move objects task 도 Ensure one tick 을 풀수 있는지 검토하기...

- 패킷 전송시 압축하여 전송하기
	- SetCompressionPacket

- Block appearance 일때 머리에 블럭 씌우기
	- 실제 인벤토리에 아이템을 넣지 말고 보이는 것만 으로 처리
- Entity appearance 일떄 머리에 해당 entity 에 맞는 모자? 씌우기
	- 실제 인벤토리에 아이템을 넣지 말고 보이는 것만 으로 처리
### Done 

- DetermineToDespawnPlayerOnDisconnect 가 동적으로 적용되나 테스트해야됨.
- Send packet 할때 TryAgain 처리해야됨.