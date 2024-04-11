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
        public void TestQueue()
        {
            int N = 1_000_000;

            using Queue<int> queue = new();

            for (int i = 0; i < N; ++i)
            {
                queue.Enqueue(i);
            }

            for (int i = 0; i < N; ++i)
            {
                int v = queue.Dequeue();
                Assert.That(i, Is.EqualTo(v));
            }

        }

        [Test]
        public void TestTable()
        {
            int N = 1_000_000;

            using Table<int, object> table = new();
            object[] objects = new object[N];

            for (int i = 0; i < N; ++i)
            {
                objects[i] = new();
            }

            for (int i = 0; i < N; ++i)
            {
                table.Insert(i, objects[i]);
            }

            for (int i = 0; i < N; ++i)
            {
                bool objectExists = table.Contains(i);
                Assert.That(objectExists, Is.EqualTo(true));
            }

            for (int i = 0; i < N; ++i)
            {
                object expected = objects[i];
                object value = table.Lookup(i);
                Assert.That(
                    ReferenceEquals(expected, value),
                    Is.EqualTo(true));
            }

            for (int i = 0; i < N; ++i)
            {
                object expected = objects[i];
                object value = table.Extract(i);
                Assert.That(
                    ReferenceEquals(expected, value),
                    Is.EqualTo(true));
            }

        }

        [Test]
        public void TestConvertEntityToChunkPosition()
        {
            Entity.Position[] positions = [
                new(10, 0, 10), 
                new(20, 0, 20), 
                new(-14, 0, -14), 
                new(-16, 0, -16), 
                new(-17, 0, -17), 
                new(-32, 0, -32),
                new(-35, 0, 20),
                ];
            Chunk.Position[] expectations = [
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
                Entity.Position p = positions[i];
                Chunk.Position pChunkExpected = expectations[i];

                Chunk.Position pChunk = Chunk.Position.Convert(p);
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

                // a == b is false.
                // Because "Hello, World!" is located at text region of memory layout, 
                // so a and b have same reference(address).
                Assert.That(a == b, Is.EqualTo(true));
            }

            {
                // Reference Types
                string a = "Hello, World!",
                    b = new String(['H', 'e', 'l', 'l', 'o', ',', ' ', 'W', 'o', 'r', 'l', 'd', '!']);

                Assert.That(a.Equals(b), Is.EqualTo(true));
                Assert.That(a == b, Is.EqualTo(true));
            }

            {
                // Reference Types
                object a = "Hello, World!",
                    b = new String([ 'H','e','l','l','o',',',' ','W','o','r','l','d','!' ]);

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