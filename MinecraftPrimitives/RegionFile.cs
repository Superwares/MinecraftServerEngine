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

        public RegionFile(FileInfo regionFile)
        {
            this.regionFile = regionFile;
            fileSize = 0;

            try
            {
                if (regionFile.Exists == true)
                {
                    lastModified = regionFile.LastWriteTimeUtc.Ticks;
                }

                fileStream = new FileStream(
                    regionFile.FullName,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite
                    );

                if (fileStream.Length < 4096L)
                {
                    fileStream.Write(EMPTY_CHUNK, 0, EMPTY_CHUNK.Length);
                    fileStream.Write(EMPTY_CHUNK, 0, EMPTY_CHUNK.Length);
                    fileSize += 8192;
                }

                if ((fileStream.Length & 4095L) != 0L)
                {
                    for (int i = 0; i < (fileStream.Length & 4095L); ++i)
                    {
                        fileStream.WriteByte(0);
                    }
                }

                int sectorCount = (int)fileStream.Length / 4096;
                freeSectors = new List<bool>(new bool[sectorCount]);

                for (int i = 0; i < sectorCount; ++i)
                {
                    freeSectors[i] = true;
                }

                freeSectors[0] = false;
                freeSectors[1] = false;
                fileStream.Seek(0, SeekOrigin.Begin);

                for (int i = 0; i < 1024; ++i)
                {
                    int value = DataInputStreamUtils.ReadInt(fileStream);
                    chunkLocations[i] = value;
                    if (value != 0 && (value >> 8) + (value & 255) <= freeSectors.Count)
                    {
                        for (int j = 0; j < (value & 255); ++j)
                        {
                            freeSectors[(value >> 8) + j] = false;
                        }
                    }
                }

                for (int i = 0; i < 1024; ++i)
                {
                    int value = DataInputStreamUtils.ReadInt(fileStream);
                    chunkTimestamps[i] = value;
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

                if (sectorStart + sectorCount > freeSectors.Count)
                {
                    return null;
                }

                fileStream.Seek(sectorStart * 4096, SeekOrigin.Begin);

                int length = DataInputStreamUtils.ReadInt(fileStream);
                if (length > 4096 * sectorCount || length <= 0)
                {
                    return null;
                }

                int compressionType = DataInputStreamUtils.ReadByte(fileStream);
                byte[] data = new byte[length - 1];

                DataInputStreamUtils.Read(fileStream, data);

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

        private bool IsOutOfBounds(int x, int z)
        {
            return x < 0 || x >= 32 || z < 0 || z >= 32;
        }

        private int GetOffset(int x, int z)
        {
            return chunkLocations[x + (z * 32)];
        }

        public void Close()
        {
            fileStream?.Close();
        }

        private void Dispose(bool disposing)
        {
            Close();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}


