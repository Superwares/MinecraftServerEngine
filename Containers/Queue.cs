using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class Queue<T> : IEnumerable<T>
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

        public IEnumerator<T> GetEnumerator()
        {
            if (_outNode == null)
                yield break;

            Debug.Assert(_inNode != null);
            Node? node = _outNode;

            while (node != null)
            {
                yield return node.Value;
                node = node.NextNode;
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

    public class SyncQueue<T> : Queue<T>
    {
        private readonly object _SharedObject = new();

        public new int Count 
        { 
            get 
            {
                lock (_SharedObject)
                {
                    return _count;
                }
            } 
        }

        public new bool Empty => (Count == 0);

        public SyncQueue() { }

        public new void Enqueue(T value)
        {
            lock (_SharedObject)
            {
                _Enqueue(value);
            }

        }

        public new T Dequeue()
        {
            lock (_SharedObject)
            {
                return _Dequeue();
            }

        }

        public new IEnumerator<T> GetEnumerator()
        {
            lock (_SharedObject)
            {
                return base.GetEnumerator();
            }

        }

    }

}
