using Containers;

namespace Containers
{
    internal class NumListTests
    {
        [Test]
        public void Test1()
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
                int j = (i * 2) + 1;
                numList.Dealloc(j);
            }

            Assert.Pass();
        }

        [Test]
        public void Test2()
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

    }

    internal class QueueTests
    {
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

    }

    internal class TableTests
    {
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
    }

}
