using Protocol;

namespace MinecraftServerEngine
{
    public class Tests
    {
        [Test]
        public void TestConvertEntityToChunkPosition()
        {
            Entity.Vector[] positions = [
                new(10, 0, 10),
                 new(20, 0, 20),
                 new(-14, 0, -14),
                 new(-16, 0, -16),
                 new(-17, 0, -17),
                 new(-32, 0, -32),
                 new(-35, 0, 20),
                 ];
            Chunk.Vector[] expectations = [
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
                Entity.Vector p = positions[i];
                Chunk.Vector pChunkExpected = expectations[i];

                Chunk.Vector pChunk = Chunk.Vector.Convert(p);
                Assert.That(pChunkExpected, Is.EqualTo(pChunk));
            }

            Assert.Pass();
        }

        /*[Test]
        public void TestGenerateGridAroundCenter()
        {

            Chunk.Vector[] C = [
                new(0, 0),
                new(100, 100),
                new(2, 5),
                ];
            int[] D = [
                5,
                10,
                0,
                ];
            (Chunk.Vector, Chunk.Vector)[] pairsExpected = [
                (new(5, 5), new(-5, -5)),
                (new(110, 110), new(-90, -90)),
                (new(2, 5), new(2, 5)),
                ];

            int length = C.Length;
            Assert.That(length, Is.EqualTo(D.Length));
            Assert.That(length, Is.EqualTo(pairsExpected.Length));

            for (int i = 0; i < length; ++i)
            {
                Chunk.Vector c = C[i];
                int d = D[i];
                (Chunk.Vector pMaxExpected, Chunk.Vector pMinExpected) = pairsExpected[i];

                (Chunk.Vector pMax, Chunk.Vector pMin) = Chunk.Vector.Generate(c, d);

                Assert.That(pMax, Is.EqualTo(pMaxExpected));
                Assert.That(pMin, Is.EqualTo(pMinExpected));
                Assert.That(pMax.X, Is.GreaterThanOrEqualTo(pMin.X));
                Assert.That(pMax.Z, Is.GreaterThanOrEqualTo(pMin.Z));

            }

        }*/

    }

}
