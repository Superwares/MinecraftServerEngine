
using System;
using System.Diagnostics;

namespace Protocol
{
    public class Tests
    {
        [Test]
        public void TestGenerateGridAroundCenter()
        {

            Chunk.Position[] C = [
                new(0, 0),
                new(100, 100),
                new(2, 5),
                ];
            int[] D = [
                5,
                10,
                0,
                ];
            (Chunk.Position, Chunk.Position)[] pairsExpected = [
                (new(5, 5), new(-5, -5)),
                (new(110, 110), new(-90, -90)),
                (new(2, 5), new(2, 5)),
                ];

            int length = C.Length;
            Assert.That(length, Is.EqualTo(D.Length));
            Assert.That(length, Is.EqualTo(pairsExpected.Length));

            for (int i = 0; i < length; ++i)
            {
                Chunk.Position c = C[i];
                int d = D[i];
                (Chunk.Position pMaxExpected, Chunk.Position pMinExpected) = pairsExpected[i];

                (Chunk.Position pMax, Chunk.Position pMin) = Chunk.Position.GenerateGridAround(c, d);

                Assert.That(pMax, Is.EqualTo(pMaxExpected));
                Assert.That(pMin, Is.EqualTo(pMinExpected));
                Assert.That(pMax.x, Is.GreaterThanOrEqualTo(pMin.x));
                Assert.That(pMax.z, Is.GreaterThanOrEqualTo(pMin.z));

            }

        }

    }

}
