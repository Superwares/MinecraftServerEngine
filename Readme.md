# Server

```text
/*************************************************************
* Copyright © 2024 Jung Hyunseo. All rights reserved.
**************************************************************
* 
* This source code is owned exclusively by Jung Hyunseo.
* 
* While contributions may have been made by project participants,
* reproduction, distribution, or modification of the entire source 
* code or any part thereof is strictly prohibited without the 
* prior written consent of the copyright owner, Jung Hyunseo.
* 
* For inquiries regarding copyright or access permissions,
* please contact welcomehyunseo@gmail.com.
*************************************************************/
```


## TODO

### Planned
* Compress Packets
* To reduce garbage as much as possible 
* Unit testing for Containers. (NumberList)
* Check leaks of Allocation and deallocation about NumberList.
* Make chunk section with indirect or direct palette.
* Mask variables as private in all packet class
* Create own containers library. (Table, Set, NumList...)
* 마인크래프트 클라이언트에서 Refresh 를 누르다보면 해당 Client가 Queue 에서 소멸하지 않고 계속 루프를 돌게됨. 왜 그런지 이유를 찾아내야됨. 아마도 Nonblokcing 에 의해 넘어가는 코드인것 같지만 어떨때는 루프가 안끝나는 경우가 있음.
* WHat is difference between EndOfFileException and SocketError.WouldBlock in SocketException?
* Make Common library project.
* Make container library more optimized.
* If the user already exist on the server, when the client is joined with the same user id, the client will be kicked by the ClientListener.

### Done

## Conventions

### Exceptions
In case of exceptions that must be handled, they must be documented using XML tags.
If not, they don't need documentation. 
For instance, the exception NotImplementedException should not be handled internally, but should be passed on to the user.

Reduce using try/catch for optimization.

#### UnexpectedValueException
```
throw new UnexpectedValueException($"ClickWindowPacket.ModeNumber {packet.ModeNumber}");
```
위처럼 패킷.변수이름 {값} 의 형태로 해당 예외를 사용한다.

### Containers
All containers must be implemented as IDisposable interface, and have empty data if the container is disposed.

### Dispose Pattern

```C#
class Base : IDisposable
{
    ...

	private bool _disposed = false;

    ...

	~Base() => Dispose(false);

    ...

    public void DoWork()
    {
        Debug.Assert(!_disposed);

        ...
    }

    ...

	protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
             
        // Assertion.

        if (disposing == true)
        {
            // Release managed resources.
        }

        // Release unmanaged resources.

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}

class Derived : Base
{
    ...

    private bool _disposed = false;
    
    ...
    
    ~Derived() => Dispose(false):

    ...

    public void DoWork2()
    {
        Debug.Assert(!_disposed);

        ...
    }

    ...

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            // Assertion.

            if (disposing == true)
            {
                // Release managed resources.
            }

            // Release unmanaged resources.

            _disposed = true;
        }

        base.Dispose(disposing);
    }

}

```

```C#
public sealed class Object : IDisposable
{
    private bool _disposed = false;

    ~Object() => Dispose(false);

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        // Assertion.

        if (disposing == true)
        {
            // Release managed resources.
        }

        // Release unmanaged resources.

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}
```

### Disposable Instances

Disposable instances must be handled by 'using' statement except members of class and struct.
If disposable objects were used as members of class and struct, they must be disposed at the current object Dispose(bool disposing) fucntion.

