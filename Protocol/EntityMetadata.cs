using Containers;
using System;
using System.Diagnostics;

namespace Protocol
{

    internal class EntityMetadata : IDisposable
    {
        private abstract class Item(byte index)
        {
            public readonly byte Index = index;

            public void Write(Buffer buffer)
            {
                buffer.WriteByte(Index);
                WriteData(buffer);
            }

            public abstract void WriteData(Buffer buffer);
        }

        private class ByteItem(byte index, byte value) : Item(index)
        {
            private readonly byte _value = value;

            public override void WriteData(Buffer buffer)
            {
                buffer.WriteInt(0, true);
                buffer.WriteByte(_value);
            }

        }

        private class IntItem(byte index, int value) : Item(index)
        {
            private readonly int _value = value;

            public override void WriteData(Buffer buffer)
            {
                buffer.WriteInt(1, true);
                buffer.WriteInt(_value, true);
            }

        }

        private class FloatItem(byte index, float value) : Item(index)
        {
            private readonly float _value = value;

            public override void WriteData(Buffer buffer)
            {
                buffer.WriteInt(2, true);
                buffer.WriteFloat(_value);
            }

        }

        private class StringItem(byte index, string value) : Item(index)
        {
            private readonly string _value = value;

            public override void WriteData(Buffer buffer)
            {
                buffer.WriteInt(3, true);
                buffer.WriteString(_value);
            }
        }

        private bool _isDisposed = false;

        private readonly Queue<Item> _items = new();

        ~EntityMetadata()
        {
            Debug.Assert(false);
        }

        public void AddByte(byte index, byte value)
        {
            _items.Enqueue(new ByteItem(index, value));
        }

        public void AddInt(byte index, int value)
        {
            _items.Enqueue(new IntItem(index, value));
        }

        public void AddFloat(byte index, float value)
        {
            _items.Enqueue(new FloatItem(index, value));
        }

        public void AddString(byte index, string value)
        {
            _items.Enqueue(new StringItem(index, value));
        }

        public byte[] WriteData()
        {
            using Buffer buffer = new();

            while (!_items.Empty)
            {
                Item item = _items.Dequeue();
                item.Write(buffer);
            }

            buffer.WriteByte(0xff);

            return buffer.ReadData();
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing == true)
            {
                // Release managed resources.
                _items.Dispose();
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
