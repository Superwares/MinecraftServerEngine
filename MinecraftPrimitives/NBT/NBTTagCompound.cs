
using Containers;
using MinecraftPrimitives.Protocols;

namespace MinecraftPrimitives.NBT
{
    public sealed class NBTTagCompound
        : NBTTagBase
        , IReadableNBTTag<NBTTagCompound>
    {
        private readonly struct TagListItem(string key, NBTTagBase value)
        {
            public readonly string Key = key;
            public readonly NBTTagBase Value = value;
        }

        public const int TypeId = 10;

        private bool _disposed = false;

        private readonly Map<string, NBTTagBase> _map;

        public static byte GetTypeId() => TypeId;

        private static NBTTagBase ReadNBTTagList(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            int id = DataInputStreamUtils.ReadByte(s);
            switch (id)
            {
                case NBTTagEnd.TypeId:
                    return NBTTagList<NBTTagEnd>.Read(s, depth + 1);
                case NBTTagByte.TypeId:
                    return NBTTagList<NBTTagByte>.Read(s, depth + 1);
                case NBTTagShort.TypeId:
                    return NBTTagList<NBTTagShort>.Read(s, depth + 1);
                case NBTTagInt.TypeId:
                    return NBTTagList<NBTTagInt>.Read(s, depth + 1);
                case NBTTagLong.TypeId:
                    return NBTTagList<NBTTagLong>.Read(s, depth + 1);
                case NBTTagFloat.TypeId:
                    return NBTTagList<NBTTagFloat>.Read(s, depth + 1);
                case NBTTagDouble.TypeId:
                    return NBTTagList<NBTTagDouble>.Read(s, depth + 1);
                case NBTTagByteArray.TypeId:
                    return NBTTagList<NBTTagByteArray>.Read(s, depth + 1);
                case NBTTagString.TypeId:
                    return NBTTagList<NBTTagString>.Read(s, depth + 1);
                case NBTTagListBase.TypeId:
                    throw new System.NotImplementedException();
                case TypeId:
                    return NBTTagList<NBTTagCompound>.Read(s, depth + 1);
                case NBTTagIntArray.TypeId:
                    return NBTTagList<NBTTagIntArray>.Read(s, depth + 1);
                case NBTTagLongArray.TypeId:
                    return NBTTagList<NBTTagLongArray>.Read(s, depth + 1);
                default:
                    throw new NBTTagException("Unknown type Id");

            }
        }

        public static NBTTagCompound Read(System.IO.Stream s, int depth)
        {
            System.Diagnostics.Debug.Assert(s != null);
            System.Diagnostics.Debug.Assert(depth >= 0);

            if (depth > 512)
            {
                throw new NBTTagException("Tried to read NBT tag with too high complexity, depth > 512");
            }

            Map<string, NBTTagBase> map = new();

            int id;

            while (true)
            {
                id = DataInputStreamUtils.ReadByte(s);
                if (NBTTagEnd.TypeId == id)
                {
                    break;
                }

                string key = DataInputStreamUtils.ReadModifiedUtf8String(s);
                NBTTagBase value;

                switch (id)
                {
                    default:
                        throw new NBTTagException("Unknown type Id");
                    case NBTTagEnd.TypeId:
                        throw new NBTTagException("Unexpected tag type");
                    case NBTTagByte.TypeId:
                        value = NBTTagByte.Read(s, depth + 1);
                        break;
                    case NBTTagShort.TypeId:
                        value = NBTTagShort.Read(s, depth + 1);
                        break;
                    case NBTTagInt.TypeId:
                        value = NBTTagInt.Read(s, depth + 1);
                        break;
                    case NBTTagLong.TypeId:
                        value = NBTTagLong.Read(s, depth + 1);
                        break;
                    case NBTTagFloat.TypeId:
                        value = NBTTagFloat.Read(s, depth + 1);
                        break;
                    case NBTTagDouble.TypeId:
                        value = NBTTagDouble.Read(s, depth + 1);
                        break;
                    case NBTTagByteArray.TypeId:
                        value = NBTTagByteArray.Read(s, depth + 1);
                        break;
                    case NBTTagString.TypeId:
                        value = NBTTagString.Read(s, depth + 1);
                        break;
                    case NBTTagListBase.TypeId:
                        value = ReadNBTTagList(s, depth + 1);
                        break;
                    case TypeId:
                        value = Read(s, depth + 1);
                        break;
                    case NBTTagIntArray.TypeId:
                        value = NBTTagIntArray.Read(s, depth + 1);
                        break;
                    case NBTTagLongArray.TypeId:
                        value = NBTTagLongArray.Read(s, depth + 1);
                        break;
                }

                if (map.Contains(key) == true)
                {
                    throw new NBTTagException("Duplicate item in NBT compound");
                }

                map.Insert(key, value);
            }

            return new NBTTagCompound(map);
        }

        private NBTTagCompound(Map<string, NBTTagBase> map)
        {
            _map = map;
        }

        public NBTTagCompound()
        {
            _map = new Map<string, NBTTagBase>();

        }

        ~NBTTagCompound()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public void WriteAsRoot(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            s.WriteByte(TypeId);
            s.WriteModifiedString("");

            foreach ((string key, NBTTagBase value) in _map.GetElements())
            {

                switch (value)
                {
                    default:
                        throw new NBTTagException("Unknown type");
                    case NBTTagEnd:
                        throw new NBTTagException("Unexpected tag type");
                    case NBTTagByte:
                        s.WriteByte(NBTTagByte.TypeId);
                        break;
                    case NBTTagShort:
                        s.WriteByte(NBTTagShort.TypeId);
                        break;
                    case NBTTagInt:
                        s.WriteByte(NBTTagInt.TypeId);
                        break;
                    case NBTTagLong:
                        s.WriteByte(NBTTagLong.TypeId);
                        break;
                    case NBTTagFloat:
                        s.WriteByte(NBTTagFloat.TypeId);
                        break;
                    case NBTTagDouble:
                        s.WriteByte(NBTTagDouble.TypeId);
                        break;
                    case NBTTagByteArray:
                        s.WriteByte(NBTTagByteArray.TypeId);
                        break;
                    case NBTTagString:
                        s.WriteByte(NBTTagString.TypeId);
                        break;
                    case NBTTagListBase:
                        s.WriteByte(NBTTagListBase.TypeId);
                        break;
                    case NBTTagCompound:
                        s.WriteByte(TypeId);
                        break;
                    case NBTTagIntArray:
                        s.WriteByte(NBTTagIntArray.TypeId);
                        break;
                    case NBTTagLongArray:
                        s.WriteByte(NBTTagLongArray.TypeId);
                        break;
                }


                s.WriteModifiedString(key);
                value.Write(s);
            }

            s.WriteByte(NBTTagEnd.TypeId);
        }

        public override void Write(MinecraftProtocolDataStream s)
        {
            System.Diagnostics.Debug.Assert(s != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            foreach ((string key, NBTTagBase value) in _map.GetElements())
            {

                switch (value)
                {
                    default:
                        throw new NBTTagException("Unknown type");
                    case NBTTagEnd:
                        throw new NBTTagException("Unexpected tag type");
                    case NBTTagByte:
                        s.WriteByte(NBTTagByte.TypeId);
                        break;
                    case NBTTagShort:
                        s.WriteByte(NBTTagShort.TypeId);
                        break;
                    case NBTTagInt:
                        s.WriteByte(NBTTagInt.TypeId);
                        break;
                    case NBTTagLong:
                        s.WriteByte(NBTTagLong.TypeId);
                        break;
                    case NBTTagFloat:
                        s.WriteByte(NBTTagFloat.TypeId);
                        break;
                    case NBTTagDouble:
                        s.WriteByte(NBTTagDouble.TypeId);
                        break;
                    case NBTTagByteArray:
                        s.WriteByte(NBTTagByteArray.TypeId);
                        break;
                    case NBTTagString:
                        s.WriteByte(NBTTagString.TypeId);
                        break;
                    case NBTTagListBase:
                        s.WriteByte(NBTTagListBase.TypeId);
                        break;
                    case NBTTagCompound:
                        s.WriteByte(TypeId);
                        break;
                    case NBTTagIntArray:
                        s.WriteByte(NBTTagIntArray.TypeId);
                        break;
                    case NBTTagLongArray:
                        s.WriteByte(NBTTagLongArray.TypeId);
                        break;
                }


                s.WriteModifiedString(key);
                value.Write(s);
            }

            s.WriteByte(NBTTagEnd.TypeId);
        }

        public void Add(string key, NBTTagBase value)
        {
            if (key == null || string.IsNullOrEmpty(key) == true)
            {
                throw new System.ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new System.ArgumentNullException(nameof(value));
            }

            _map.Insert(key, value);
        }

        public T GetNBTTag<T>(string key) where T : NBTTagBase
        {
            try
            {
                NBTTagBase nbtBase = _map.Lookup(key);

                return (T)nbtBase;
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
            catch (System.InvalidCastException)
            {
                return null;
            }

        }

        public override string ToString()
        {
            string tab = "    ";

            string str = "{\n";

            foreach ((string name, NBTTagBase tag) in _map.GetElements())
            {
                string _str = tag.ToString();

                string indentedStr = string.Join($"\n{tab}",
                    _str.Split('\n', System.StringSplitOptions.None));

                //Console.WriteLine($"{tag.Key} : {tag}");

                str += $"{tab}{name}: {indentedStr}";

                //if (name != System.Linq.Enumerable.Last(_map.GetElements()).Item1)
                //{
                //    str += $", ";
                //}

                str += $", ";
                str += "\n";
            }

            str += "}";

            return str;
        }

        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    foreach (NBTTagBase tag in _map.GetValues())
                    {
                        tag.Dispose();
                    }
                    _map.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }
}
