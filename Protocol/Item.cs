
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

        private readonly int _count;
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
