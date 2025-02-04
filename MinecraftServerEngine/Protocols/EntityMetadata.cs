
using Containers;

using MinecraftServerEngine.Items;

namespace MinecraftServerEngine.Protocols
{

    internal sealed class EntityMetadata : System.IDisposable
    {
        private abstract class Item(byte index)
        {
            public readonly byte Index = index;

            internal void Write(MinecraftProtocolDataStream buffer)
            {
                buffer.WriteByte(Index);
                WriteData(buffer);
            }

            internal abstract void WriteData(MinecraftProtocolDataStream buffer);
        }

        private class ByteItem(byte index, byte value) : Item(index)
        {
            private readonly byte Value = value;

            internal override void WriteData(MinecraftProtocolDataStream buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(0, true);
                buffer.WriteByte(Value);
            }

        }

        private class IntItem(byte index, int value) : Item(index)
        {
            private readonly int Value = value;

            internal override void WriteData(MinecraftProtocolDataStream buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(1, true);
                buffer.WriteInt(Value, true);
            }

        }

        private class FloatItem(byte index, float value) : Item(index)
        {
            private readonly float Value = value;

            internal override void WriteData(MinecraftProtocolDataStream buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(2, true);
                buffer.WriteFloat(Value);
            }

        }

        private class StringItem(byte index, string value) : Item(index)
        {
            private readonly string Value = value;

            internal override void WriteData(MinecraftProtocolDataStream buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(3, true);
                buffer.WriteString(Value);
            }
        }

        private class ItemStackItem(byte index, ItemStack value) : Item(index)
        {
            private readonly ItemStack Value = value;

            internal override void WriteData(MinecraftProtocolDataStream buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(5, true);
                buffer.WriteData(Value.WriteData());
            }
        }

        private class BoolItem(byte index, bool value) : Item(index)
        {
            private readonly bool Value = value;

            internal override void WriteData(MinecraftProtocolDataStream buffer)
            {
                System.Diagnostics.Debug.Assert(buffer != null);

                buffer.WriteInt(6, true);
                buffer.WriteBool(Value);
            }
        }

        private bool _disposed = false;

        private readonly Queue<Item> Items = new();

        ~EntityMetadata() => System.Diagnostics.Debug.Assert(false);

        internal void AddByte(byte index, byte value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new ByteItem(index, value));
        }

        internal void AddInt(byte index, int value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new IntItem(index, value));
        }

        internal void AddFloat(byte index, float value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new FloatItem(index, value));
        }

        internal void AddString(byte index, string value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new StringItem(index, value));
        }

        internal void AddItemStack(byte index, ItemStack value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new ItemStackItem(index, value));
        }

        internal void AddBool(byte index, bool value)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(index >= 0);

            Items.Enqueue(new BoolItem(index, value));
        }

        internal void WriteData(MinecraftProtocolDataStream buffer)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            while (Items.Empty == false)
            {
                Item item = Items.Dequeue();
                item.Write(buffer);
            }

            buffer.WriteByte(0xff);
        }

        public void Dispose()
        {
            // Assertions.
            System.Diagnostics.Debug.Assert(_disposed == false);

            // Release resources.
            Items.Dispose();

            // Finish.
            System.GC.SuppressFinalize(this);
            _disposed = true;
        }


    }
}
