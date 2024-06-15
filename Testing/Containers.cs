namespace Containers
{
    internal class NumListTests
    {
        private const int N = 10_000;

        [Test]
        public void Test1()
        {
            using Numlist numlist = new();

            for (int i = 0; i < N; ++i)
            {
                int num = numlist.Alloc();
                Assert.That(num, Is.EqualTo(i));
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                numlist.Dealloc(j);
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = (i * 2) + 1;
                numlist.Dealloc(j);
            }

        }

        [Test]
        public void Test2()
        {
            using Numlist numlist = new();

            for (int i = 0; i < N; ++i)
            {
                int num = numlist.Alloc();
                Assert.That(num, Is.EqualTo(i));
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                numlist.Dealloc(j);
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                int num = numlist.Alloc();
                Assert.That(num, Is.EqualTo(j));
            }

            for (int i = 0; i < N; ++i)
            {
                numlist.Dealloc(i);
            }

        }

    }

    internal class QueueTests
    {
        private const int N = 1_000_000;

        [Test]
        public void Test1()
        {
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

    internal class SetTests
    {
        private const int N = 1_000_000;
        private readonly object[] _EXPECTED_OBJECTS = new object[N];

        [SetUp]
        public void Init()
        {
            for (int i = 0; i < N; ++i)
            {
                _EXPECTED_OBJECTS[i] = new();
            }
        }

        [Test]
        public void TestValueType()
        {
            using Set<int> values = new();

            for (int i = 0; i < N; ++i)
            {
                values.Insert(i);
            }

            for (int i = 0; i < N; ++i)
            {
                bool exists = values.Contains(i);
                Assert.That(exists);
            }

            for (int i = 0; i < N; ++i)
            {
                values.Extract(i);
            }

        }

        [Test]
        public void TestReferenceType()
        {
            using Set<object> objects = new();

            for (int i = 0; i < N; ++i)
            {
                object obj = _EXPECTED_OBJECTS[i];
                objects.Insert(obj);
            }

            for (int i = 0; i < N; ++i)
            {
                object obj = _EXPECTED_OBJECTS[i];
                bool exists = objects.Contains(obj);
                Assert.That(exists);
            }

            for (int i = 0; i < N; ++i)
            {
                object obj = _EXPECTED_OBJECTS[i];
                objects.Extract(obj);
            }

        }

    }

    internal class TreeTests
    {
        private const int N = 1_000_000;
        private readonly object[] _EXPECTED_OBJECTS = new object[N];

        [SetUp]
        public void Init()
        {
            for (int i = 0; i < N; ++i)
            {
                _EXPECTED_OBJECTS[i] = new();
            }
        }

        [Test]
        public void TestValueType()
        {
            using Tree<int> values = new();

            for (int i = 0; i < N; ++i)
            {
                values.Insert(i);
            }

            for (int i = 0; i < N; ++i)
            {
                bool exists = values.Contains(i);
                Assert.That(exists);
            }

            for (int i = 0; i < N; ++i)
            {
                values.Extract(i);
            }

        }

        [Test]
        public void TestReferenceType()
        {
            using Tree<object> objects = new();

            for (int i = 0; i < N; ++i)
            {
                _EXPECTED_OBJECTS[i] = new();
            }

            for (int i = 0; i < N; ++i)
            {
                object obj = _EXPECTED_OBJECTS[i];
                objects.Insert(obj);
            }

            for (int i = 0; i < N; ++i)
            {
                object obj = _EXPECTED_OBJECTS[i];
                bool exists = objects.Contains(obj);
                Assert.That(exists);
            }

            for (int i = 0; i < N; ++i)
            {
                object obj = _EXPECTED_OBJECTS[i];
                objects.Extract(obj);
            }

        }

    }

    internal class TableTests
    {
        private const int N = 1_000_000;
        private readonly object[] _EXPECTED_OBJECTS = new object[N];

        [SetUp]
        public void Init()
        {
            for (int i = 0; i < N; ++i)
            {
                _EXPECTED_OBJECTS[i] = new();
            }
        }

        [Test]
        public void Test1()
        {
            using Table<int, object> objects = new();

            for (int i = 0; i < N; ++i)
            {
                objects.Insert(i, _EXPECTED_OBJECTS[i]);
            }

            for (int i = 0; i < N; ++i)
            {
                bool exists = objects.Contains(i);
                Assert.That(exists);
            }

            for (int i = 0; i < N; ++i)
            {
                object expected = _EXPECTED_OBJECTS[i];
                object value = objects.Lookup(i);
                Assert.That(ReferenceEquals(expected, value));
            }

            for (int i = 0; i < N; ++i)
            {
                object expected = _EXPECTED_OBJECTS[i];
                object value = objects.Extract(i);
                Assert.That(ReferenceEquals(expected, value));
            }

        }
    }

    internal class MapTests
    {
        private const int N = 1_000_000;
        private readonly object[] _EXPECTED_OBJECTS = new object[N];

        [SetUp]
        public void Init()
        {
            for (int i = 0; i < N; ++i)
            {
                _EXPECTED_OBJECTS[i] = new();
            }
        }

        [Test]
        public void Test1()
        {
            using Map<int, object> objects = new();

            for (int i = 0; i < N; ++i)
            {
                objects.Insert(i, _EXPECTED_OBJECTS[i]);
            }

            for (int i = 0; i < N; ++i)
            {
                bool exists = objects.Contains(i);
                Assert.That(exists);
            }

            for (int i = 0; i < N; ++i)
            {
                object expected = _EXPECTED_OBJECTS[i];
                object value = objects.Lookup(i);
                Assert.That(ReferenceEquals(expected, value));
            }

            for (int i = 0; i < N; ++i)
            {
                object expected = _EXPECTED_OBJECTS[i];
                object value = objects.Extract(i);
                Assert.That(ReferenceEquals(expected, value));
            }

        }

    }
}
