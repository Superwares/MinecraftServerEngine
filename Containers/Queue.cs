using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class Queue<T>
    {
        protected class Node(T value)
        {
            private T _value = value;
            public T Value => _value;

            public Node? NextNode = null;

        }

        protected Node? _outNode = null, _inNode = null;

        protected int _count = 0;
        public int Count => _count;
        public bool Empty => (_count == 0);

        public Queue() { }

        protected void _Enqueue(T value)
        {
            Node newNode = new(value);

            if (_count == 0)
            {
                Debug.Assert(_outNode == null && _inNode == null);

                _outNode = _inNode = newNode;
            }
            else
            {
                Debug.Assert(_outNode != null && _inNode != null);

                _inNode.NextNode = newNode;
                _inNode = newNode;
            }

            _count++;
        }

        public void Enqueue(T value) => _Enqueue(value);

        protected T _Dequeue()
        {
            if (_count == 0)
                throw new EmptyQueueException();

            Debug.Assert(_inNode != null);
            Debug.Assert(_outNode != null);
            Node resultNode = _outNode;

            if (_count == 1)
            {
                _inNode = _outNode = null;
            }
            else
            {
                Debug.Assert(_count > 1);
                _outNode = _outNode.NextNode;
            }

            _count--;

            return resultNode.Value;
        }

        public T Dequeue() => _Dequeue();

    }

    public class SyncQueue<T> : Queue<T>
    {
        private readonly object _sharedObject = new();

        public SyncQueue() { }

        public new void Enqueue(T value)
        {
            lock (_sharedObject)
            {
                _Enqueue(value);

                // TODO signal
            }
        }

        public new T Dequeue()
        {
            lock (_sharedObject)
            {
                return _Dequeue();
            }
        }

    }

    public class Channel<T> : Queue<T>
    {
        private readonly object _sharedObject = new();
        private bool _wait = false;

        public new void Enqueue(T value)
        {
            lock (_sharedObject)
            {
                if (_wait == true)
                {
                    Debug.Assert(Empty == true);

                    Monitor.Pulse(_sharedObject);
                }

                _Enqueue(value);
            }
        }

        public new T Dequeue(int millisecondsTimeout)
        {
            T? value;

            lock (_sharedObject)
            {

                if (Empty == true)
                {
                    Debug.Assert(_wait == false);
                    _wait = true;

                    Monitor.Wait(_sharedObject, millisecondsTimeout);
                    if (Empty == true)
                    {
                        _wait = false;
                        throw new TimeoutException();
                    }
                }

                value = _Dequeue();

            }

            Debug.Assert(value != null);
            return value;
        }

    }

}
