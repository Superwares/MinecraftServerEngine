using Application;
using System.Diagnostics;
using System.Numerics;


namespace Application
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestNumList1()
        {
            int N = 10_000;
            using NumList numList = new();

            for (int i = 0; i < N; ++i)
            {
                int num = numList.Alloc();
                Assert.That(num, Is.EqualTo(i));
            }

            for (int i = 0; i < N/2; ++i)
            {
                int j = i * 2;
                numList.Dealloc(j);
            }

            for (int i = 0; i < N/2; ++i)
            {
                int j = (i * 2) + 1;
                numList.Dealloc(j);
            }

            Assert.Pass();
        }

        [Test]
        public void TestNumList2()
        {
            int N = 10_000;
            using NumList numList = new();

            for (int i = 0; i < N; ++i)
            {
                int num = numList.Alloc();
                Assert.That(num, Is.EqualTo(i));
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                numList.Dealloc(j);
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                int num = numList.Alloc();
                Assert.That(num, Is.EqualTo(j));
            }

            for (int i = 0; i < N; ++i)
            {
                numList.Dealloc(i);
            }

            Assert.Pass();
        }

        [Test]
        public void TestConvertEntityToChunkPosition()
        {
            EntityPosition[] positions = [
                new(10, 0, 10), 
                new(20, 0, 20), 
                new(-14, 0, -14), 
                new(-16, 0, -16), 
                new(-17, 0, -17), 
                new(-32, 0, -32),
                new(-35, 0, 20),
                ];
            ChunkPosition[] expectations = [
                new(0, 0),
                new(1, 1),
                new(-1, -1),
                new(-1, -1),
                new(-2, -2),
                new(-2, -2),
                new(-3, 1),
                ];
            Assert.That(positions, Has.Length.EqualTo(expectations.Length));

            for (int i = 0; i < positions.Length; ++i)
            {
                EntityPosition p = positions[i];
                ChunkPosition pChunkExpected = expectations[i];

                ChunkPosition pChunk = Chunk.Convert(p);
                Assert.That(pChunkExpected, Is.EqualTo(pChunk));
            }

            Assert.Pass();
        }

        [Test]
        public void TestBasicOperations()
        {
            {
                int N = 1_000_000;
                bool[] boolArr = new bool[N];
                bool defaultBoolValue = false;

                for (int i = 0; i < N; ++i)
                {
                    Assert.That(boolArr[i], Is.EqualTo(defaultBoolValue));
                }
            }

            {
                // Reference Types
                string a = "Hello, World!", b = "Hello, World!";

                Assert.That(a.Equals(b), Is.EqualTo(true));
                Assert.That(a == b, Is.EqualTo(false));
            }

            {
                // Value Types
                int a = 999, b = 999;

                Assert.That(a.Equals(b), Is.EqualTo(true));
                Assert.That(a == b, Is.EqualTo(true));
            }

        }


    }
}