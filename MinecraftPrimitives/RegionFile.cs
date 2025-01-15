using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace MinecraftPrimitives
{
    public sealed class RegionFile : IDisposable
    {
        private static readonly byte[] EMPTY_CHUNK = new byte[4096];
        private readonly FileInfo regionFile;
        private FileStream fileStream;

        // Location information of each chunk
        private readonly int[] chunkLocations = new int[1024];

        // Last modification time of each chunk
        private readonly int[] chunkTimestamps = new int[1024];

        // Indicates the status of sectors available for use within the file. (true: available, false: in use.)
        private List<bool> freeSectors;

        // Indicates the file size in bytes. Used to calculate the size of sectors in use.
        private int fileSize;

        // Last modification time of the file
        private long lastModified;

        private static int ReadByte(FileStream s)
        {
            int value = s.ReadByte();
            if (value < 0)
            {
                throw new Exception("EOF");
            }

            return value;
        }

        private static void ReadBytes(FileStream s, byte[] bytes)
        {
            s.Read(bytes, 0, bytes.Length);
        }

        private static int ReadInt32(FileStream s)
        {
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            if ((b0 | b1 | b2 | b3) < 0)
            {
                throw new Exception("EOF");
            }
            return ((b0 << 24) + (b1 << 16) + (b2 << 8) + (b3 << 0));
        }

        public RegionFile(FileInfo regionFile)
        {
            this.regionFile = regionFile;
            this.fileSize = 0;

            try
            {
                if (regionFile.Exists == true)
                {
                    this.lastModified = regionFile.LastWriteTimeUtc.Ticks;
                }

                this.fileStream = new FileStream(regionFile.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

                if (this.fileStream.Length < 4096L)
                {
                    this.fileStream.Write(EMPTY_CHUNK, 0, EMPTY_CHUNK.Length);
                    this.fileStream.Write(EMPTY_CHUNK, 0, EMPTY_CHUNK.Length);
                    this.fileSize += 8192;
                }

                if ((this.fileStream.Length & 4095L) != 0L)
                {
                    for (int i = 0; i < (this.fileStream.Length & 4095L); ++i)
                    {
                        this.fileStream.WriteByte(0);
                    }
                }

                int sectorCount = (int)this.fileStream.Length / 4096;
                this.freeSectors = new List<bool>(new bool[sectorCount]);

                for (int i = 0; i < sectorCount; ++i)
                {
                    this.freeSectors[i] = true;
                }

                this.freeSectors[0] = false;
                this.freeSectors[1] = false;
                this.fileStream.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < 1024; ++i)
                {
                    int value = ReadInt32(this.fileStream);
                    this.chunkLocations[i] = value;
                    if (value != 0 && (value >> 8) + (value & 255) <= this.freeSectors.Count)
                    {
                        for (int j = 0; j < (value & 255); ++j)
                        {
                            this.freeSectors[(value >> 8) + j] = false;
                        }
                    }
                }

                for (int i = 0; i < 1024; ++i)
                {
                    int value = ReadInt32(this.fileStream);
                    this.chunkTimestamps[i] = value;
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }

        public Stream ReadChunk(int x, int z)
        {
            if (IsOutOfBounds(x, z) == true)
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

                if (sectorStart + sectorCount > this.freeSectors.Count)
                {
                    return null;
                }

                this.fileStream.Seek(sectorStart * 4096, SeekOrigin.Begin);

                int length = ReadInt32(this.fileStream);
                if (length > 4096 * sectorCount || length <= 0)
                {
                    return null;
                }

                int compressionType = ReadByte(this.fileStream);
                byte[] data = new byte[length - 1];

                ReadBytes(this.fileStream, data);

                if (compressionType == 1)
                {
                    GZipStream _s = new(new MemoryStream(data), CompressionMode.Decompress);
                    MemoryStream s = new();
                    _s.CopyTo(s);
                    return s;
                }
                else if (compressionType == 2)
                {
                    using (var compressedDataStream = new MemoryStream(data))
                    {
                        // ZLIB 스트림을 처리하기 위해 DeflateStream을 사용할 때 ZLIB 헤더를 처리해야 합니다.
                        // ZLIB 헤더(2 바이트)를 스킵합니다.
                        compressedDataStream.ReadByte(); // 첫 번째 바이트
                        compressedDataStream.ReadByte(); // 두 번째 바이트

                        using (var deflateStream = new DeflateStream(compressedDataStream, CompressionMode.Decompress))
                        {
                            var outputStream = new MemoryStream();
                            deflateStream.CopyTo(outputStream);
                            outputStream.Seek(0, SeekOrigin.Begin); // 스트림의 시작으로 되돌립니다.
                            return outputStream;
                        }
                    }


                }
                else
                {
                    return null;
                }

            }
            catch (IOException ex)
            {
                // TODO: Print warning message and returns null
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
            return this.chunkLocations[x + (z * 32)];
        }

        private void SetOffset(int x, int z, int offset)
        {
            this.chunkLocations[x + z * 32] = offset;
            this.fileStream.Seek((x + z * 32) * 4, SeekOrigin.Begin);
            using (BinaryWriter writer = new BinaryWriter(this.fileStream, System.Text.Encoding.Default, leaveOpen: true))
            {
                writer.Write(offset);
            }
        }

        public void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            this.fileStream?.Close();
        }

        public void Dispose() => Close();

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


