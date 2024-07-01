
using Containers;

namespace MinecraftServerEngine
{
    internal sealed class EntityMetadata : System.IDisposable
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
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(0, true);
                buffer.WriteByte(_value);
            }

        }

        private class IntItem(byte index, int value) : Item(index)
        {
            private readonly int _value = value;

            public override void WriteData(Buffer buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(1, true);
                buffer.WriteInt(_value, true);
            }

        }

        private class FloatItem(byte index, float value) : Item(index)
        {
            private readonly float _value = value;

            public override void WriteData(Buffer buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(2, true);
                buffer.WriteFloat(_value);
            }

        }

        private class StringItem(byte index, string value) : Item(index)
        {
            private readonly string _value = value;

            public override void WriteData(Buffer buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(3, true);
                buffer.WriteString(_value);
            }
        }

        /*private class SlotDataItem(byte index, SlotData value) : Item(index)
        {
            private readonly SlotData _VALUE = value;

            public override void WriteData(Buffer buffer)
            {
        System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(5, true);
                buffer.WriteData(_VALUE.WriteData());
            }
        }*/

        private class BoolItem(byte index, bool value) : Item(index)
        {
            private readonly bool _VALUE = value;

            public override void WriteData(Buffer buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(6, true);
                buffer.WriteBool(_VALUE);
            }
        }

        private bool _disposed = false;

        private readonly Queue<Item> Items = new();

        ~EntityMetadata() => System.Diagnostics.Debug.Assert(false);

        public void AddByte(byte index, byte value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new ByteItem(index, value));
        }

        public void AddInt(byte index, int value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new IntItem(index, value));
        }

        public void AddFloat(byte index, float value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new FloatItem(index, value));
        }

        public void AddString(byte index, string value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new StringItem(index, value));
        }

        /*public void AddSlotData(byte index, SlotData value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

        System.Diagnostics.Debug.Assert(index >= 0);

            _ITEMS.Enqueue(new SlotDataItem(index, value));
        }*/

        public void AddBool(byte index, bool value)
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new BoolItem(index, value));
        }

        public byte[] WriteData()
        {
            System.Diagnostics.Debug.Assert(!_disposed);

            using Buffer buffer = new();

            while (!Items.Empty)
            {
                Item item = Items.Dequeue();
                item.Write(buffer);
            }

            buffer.WriteByte(0xff);

            return buffer.ReadData();
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(!_disposed);

            // Release resources.
            Items.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }
}
