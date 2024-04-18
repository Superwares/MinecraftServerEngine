using System;
using System.Diagnostics;

namespace Containers
{
    public interface IReadOnlyList<T>
    {
        public int Length { get; }
        public bool Empty { get; }

        public T Get(int i);

    }

    public class List<T> : IReadOnlyList<T>, IDisposable
    {
        private bool _isDisposed = false;

        private readonly T[] _data;
        private readonly int _length;

        public int Length
        {
            get
            {
                Debug.Assert(!_isDisposed);
                return _length;
            }
        }

        public bool Empty => (Length == 0);

        List(int length)
        {
            _data = new T[length];
            _length = length;
        }

        ~List() => Dispose(false);

        public T Get(int i)
        {
            Debug.Assert(!_isDisposed);

            throw new NotImplementedException();
        }

        public void Set(int i, T value)
        {
            Debug.Assert(!_isDisposed);

            throw new NotImplementedException();
        }


        public T[] Flush()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            Debug.Assert(Length == 0);

            if (disposing == true)
            {
                // Release managed resources.
                
            }

            // Release unmanaged resources.

            _isDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close() => Dispose();

    }

}
