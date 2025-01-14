using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace MinecraftUtils
{
    public class RegionFile
    {
        private static readonly byte[] a = new byte[4096];
        private readonly FileInfo b;
        private FileStream c;
        private readonly int[] d = new int[1024];
        private readonly int[] e = new int[1024];
        private List<bool> f;
        private int g;
        private long h;

        private static byte ReadByte(FileStream s)
        {
            int ch = s.ReadByte();
            if (ch < 0)
            {
                throw new Exception("EOF");
            }

            return (byte)(ch);
        }

        private static void ReadBytes(FileStream s, byte[] bytes)
        {
            s.Read(bytes, 0, bytes.Length);
        }

        private static int ReadInt32(FileStream s)
        {
            int ch1 = s.ReadByte();
            int ch2 = s.ReadByte();
            int ch3 = s.ReadByte();
            int ch4 = s.ReadByte();
            if ((ch1 | ch2 | ch3 | ch4) < 0)
            {
                throw new Exception("EOF");
            }
            return ((ch1 << 24) + (ch2 << 16) + (ch3 << 8) + (ch4 << 0));
        }

        public RegionFile(FileInfo file)
        {
            this.b = file;
            this.g = 0;

            try
            {
                if (file.Exists == true)
                {
                    this.h = file.LastWriteTimeUtc.Ticks;
                }

                this.c = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                if (this.c.Length < 4096L)
                {
                    this.c.Write(a, 0, a.Length);
                    this.c.Write(a, 0, a.Length);
                    this.g += 8192;
                }

                if ((this.c.Length & 4095L) != 0L)
                {
                    for (int i = 0; i < (this.c.Length & 4095L); ++i)
                    {
                        this.c.WriteByte(0);
                    }
                }

                int sectorCount = (int)this.c.Length / 4096;
                this.f = new List<bool>(new bool[sectorCount]);

                for (int i = 0; i < sectorCount; ++i)
                {
                    this.f[i] = true;
                }

                this.f[0] = false;
                this.f[1] = false;
                this.c.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < 1024; ++i)
                {
                    int value = ReadInt32(this.c);
                    this.d[i] = value;
                    if (value != 0 && (value >> 8) + (value & 255) <= this.f.Count)
                    {
                        for (int j = 0; j < (value & 255); ++j)
                        {
                            this.f[(value >> 8) + j] = false;
                        }
                    }
                }

                for (int i = 0; i < 1024; ++i)
                {
                    int value = ReadInt32(this.c);
                    this.e[i] = value;
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }

        public byte[] ReadChunk(int x, int z)
        {
            if (IsOutOfBounds(x, z))
            {
                return null;
            }

            try
            {
                int offset = GetOffset(x, z);
                if (offset == 0)
                {
                    return null;
                }

                int sectorStart = offset >> 8;
                int sectorCount = offset & 255;

                if (sectorStart + sectorCount > this.f.Count)
                {
                    return null;
                }

                this.c.Seek(sectorStart * 4096, SeekOrigin.Begin);

                int length = ReadInt32(this.c);
                if (length > 4096 * sectorCount || length <= 0)
                {
                    return null;
                }

                byte compressionType = ReadByte(this.c);
                byte[] data = new byte[length - 1];

                ReadBytes(this.c, data);

                if (compressionType == 1)
                {
                    using (var gzipStream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                    using (var memoryStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
                else if (compressionType == 2)
                {
                    using (var deflateStream = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress))
                    using (var memoryStream = new MemoryStream())
                    {
                        deflateStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }
                }
                else
                {
                    return null;
                }

            }
            catch (IOException ex)
            {
                throw ex;
            }
        }


        //public DataInputStream ReadChunk(int x, int z)
        //{
        //    if (IsOutOfBounds(x, z))
        //    {
        //        return null;
        //    }

        //    try
        //    {
        //        int offset = GetOffset(x, z);
        //        if (offset == 0)
        //        {
        //            return null;
        //        }

        //        int sectorStart = offset >> 8;
        //        int sectorCount = offset & 255;

        //        if (sectorStart + sectorCount > this.f.Count)
        //        {
        //            return null;
        //        }

        //        this.c.Seek(sectorStart * 4096, SeekOrigin.Begin);
        //        using (BinaryReader reader = new BinaryReader(this.c, System.Text.Encoding.Default, leaveOpen: true))
        //        {
        //            int length = reader.ReadInt32();
        //            if (length > 4096 * sectorCount || length <= 0)
        //            {
        //                return null;
        //            }

        //            byte compressionType = reader.ReadByte();
        //            byte[] data = reader.ReadBytes(length - 1);

        //            if (compressionType == 1)
        //            {
        //                return new DataInputStream(new GZipStream(new MemoryStream(data), CompressionMode.Decompress));
        //            }
        //            else if (compressionType == 2)
        //            {
        //                return new DataInputStream(new DeflateStream(new MemoryStream(data), CompressionMode.Decompress));
        //            }
        //            else
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    catch (IOException)
        //    {
        //        return null;
        //    }
        //}

        //public DataOutputStream WriteChunk(int x, int z)
        //{
        //    return IsOutOfBounds(x, z) ? null : new DataOutputStream(new DeflateStream(new ChunkBuffer(x, z), CompressionMode.Compress));
        //}

        private bool IsOutOfBounds(int x, int z)
        {
            return x < 0 || x >= 32 || z < 0 || z >= 32;
        }

        private int GetOffset(int x, int z)
        {
            return this.d[x + (z * 32)];
        }

        private void SetOffset(int x, int z, int offset)
        {
            this.d[x + z * 32] = offset;
            this.c.Seek((x + z * 32) * 4, SeekOrigin.Begin);
            using (BinaryWriter writer = new BinaryWriter(this.c, System.Text.Encoding.Default, leaveOpen: true))
            {
                writer.Write(offset);
            }
        }

        public void Close()
        {
            this.c?.Close();
        }

        private class ChunkBuffer : MemoryStream
        {
            private readonly int x;
            private readonly int z;

            public ChunkBuffer(int x, int z)
            {
                this.x = x;
                this.z = z;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Write chunk data here
                }
                base.Dispose(disposing);
            }
        }
    }
}


