using Common;

namespace Sync
{
    internal class BarrierTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            const int N = 10_000;

            const int ThreadCount = 4;
            Thread thread;

            using Barrier barrier = new(ThreadCount);

            var threads = new Thread[ThreadCount];
            for (int i = 0; i < ThreadCount; ++i)
            {
                thread = Thread.New(() =>
                {
                    for (int j = 0; j < N; ++j)
                    {
                        barrier.SignalAndWait();
                    }
                });
                threads[i] = thread;
            }
            

            for (int i = 0; i < ThreadCount; ++i)
            {
                thread = threads[i];
                thread.Join();
            }
        }
    }

    internal class ReadLockerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            const int N = 1_000_000;

            const int ThreadCount = 4;
            Thread thread;

            using ReadLocker Locker = new();
            
            int count = 0;

            var threads = new Thread[ThreadCount];
            {
                Locker.Read();

                for (int i = 0; i < ThreadCount; ++i)
                {
                    thread = Thread.New(() =>
                    {
                        for (int j = 0; j < N; ++j)
                        {
                            Locker.Hold();
                            ++count;
                            Locker.Release();
                        }
                    });
                    threads[i] = thread;
                }

                Assert.That(count, Is.EqualTo(0));
                Locker.Release();
            }

            for (int i = 0; i < ThreadCount; ++i)
            {
                thread = threads[i];
                thread.Join();
            }

            int ExpectedCount = N * ThreadCount;
            Assert.That(count, Is.EqualTo(ExpectedCount));
        }

        [Test]
        public void Test2()
        {
            const int N = 1_000_000;

            using ReadLocker Locker = new();

            for (int i = 0; i < N; ++i)
            {
                Locker.Read();
            }

            for (int i = 0; i < N; ++i)
            {
                Locker.Release();
            }
        }

    }
}
