
namespace MinecraftServerEngine
{

    internal class EntityIdAllocatorTests
    {
        [Test]
        public void Test1()
        {
            const int N = 10_000;

            for (int i = 0; i < N; ++i)
            {
                int num = EntityIdAllocator.Alloc();
                Assert.That(num, Is.EqualTo(i));
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                EntityIdAllocator.Dealloc(j);
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = (i * 2) + 1;
                EntityIdAllocator.Dealloc(j);
            }

        }

        [Test]
        public void Test2()
        {
            const int N = 10_000;

            for (int i = 0; i < N; ++i)
            {
                int num = EntityIdAllocator.Alloc();
                Assert.That(num, Is.EqualTo(i));
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                EntityIdAllocator.Dealloc(j);
            }

            for (int i = 0; i < N / 2; ++i)
            {
                int j = i * 2;
                int num = EntityIdAllocator.Alloc();
                Assert.That(num, Is.EqualTo(j));
            }

            for (int i = 0; i < N; ++i)
            {
                EntityIdAllocator.Dealloc(i);
            }

        }

    }
}
