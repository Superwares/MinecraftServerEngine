# Server

## TODO

### Planned
* Compress Packets
* To reduce garbage as much as possible 
* Unit testing for Containers. (NumberList)
* Check leaks of Allocation and deallocation about NumberList.
* Make chunk section with indirect or direct palette.
* Mask variables as private in all packet class
* Create own containers library. (Table, Set, NumList...)

### Done

## Conventions

### Dispose Pattern

```c#
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

### Disposable Instances

Disposable instances must be handled by 'using' statement except members of class and struct.
If disposable objects were used as members of class and struct, they must be disposed at the current object Dispose(bool disposing) fucntion.

