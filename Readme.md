# Minecraft Server Engine
The Minecraft Server Engine is a powerful and versatile game engine designed to significantly enhance the gameplay experience in Minecraft.

## Features

### Comprehensive Customization of Game Objects
The Minecraft Server Engine offers extensive customization options for game objects, 
allowing server developers to tailor the game to their specific needs. 

* World: TODO description
* Entities: TODO description
* Skill Objects: TODO description

### Parallel Computing
One of the standout features of the Minecraft Server Engine is its support for parallel computing, 
which significantly enhances server performance and scalability.


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

