
using System;
using System.Diagnostics;

namespace Protocol
{
    public abstract class Item : IEquatable<Item>
    {
        public int MinCount => 1;
        public abstract int MaxCount { get; }

        private readonly int _id;
        public int Id => _id;

        private int _count;
        public int Count
        {
            get
            {
                Debug.Assert(_count <= MaxCount);
                Debug.Assert(_count >= MinCount);
                return _count;
            }
        }

        public Item(int id, int count)
        {
            Debug.Assert(MaxCount >= MinCount);
            Debug.Assert(count <= MaxCount);
            Debug.Assert(count >= MinCount);
            _id = id;
            _count = count;
        }

        public int Stack(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);
            System.Diagnostics.Debug.Assert(count >= MinCount);
            System.Diagnostics.Debug.Assert(count <= MaxCount);

            int rest;
            _count += count;

            if (_count > MaxCount)
            {
                rest = _count - MaxCount;
                _count = MaxCount;
            }
            else
            {
                rest = 0;
            }

            return count - rest;
        }

        public void Waste(int count)
        {
            System.Diagnostics.Debug.Assert(_count >= MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            System.Diagnostics.Debug.Assert(_count > count);
            _count -= count;
        }

        private Item Clone(int count)
        {
            System.Diagnostics.Debug.Assert(_count > MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            switch (_id)
            {
                default:
                    throw new NotImplementedException();
                case 280:
                    return new Stick(count);
            }
        }

        public Item DivideHalf()
        {
            System.Diagnostics.Debug.Assert(_count > MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            Debug.Assert((_count % 2) <= 1);
            int count = (_count / 2) + (_count % 2);
            _count /= 2;

            return Clone(count);

        }

        public Item DivideOne()
        {
            System.Diagnostics.Debug.Assert(_count > MinCount);
            System.Diagnostics.Debug.Assert(_count <= MaxCount);

            _count--;
            return Clone(1);
        }

        public bool Equals(Item other)
        {
            if (Id != other.Id) return false;
            if (Count != other.Count) return false;

            return true;
        }
    }

    public abstract class SingleItem : Item
    {
        public override int MaxCount => 1;

        public SingleItem(int id, int count) : base(id, count) { }
    }

    public abstract class BundleItem : Item
    {
        public override int MaxCount => 64;

        public BundleItem(int id, int count) : base(id, count) { }
    }

    public sealed class Stick : BundleItem
    {
        public Stick(int count) : base(280, count) { }
    }

}
