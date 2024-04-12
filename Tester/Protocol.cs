
using System;

namespace Protocol
{
    public class Tests
    {
        [Test]
        public void TestGenerateGridAroundCenter()
        {
            Chunk.Position c = new(0, 0);
            int d = 5;

            Chunk.Position[] expectedPositions = [
                new(-5, -5), new(-4, -5), new(-3, -5), new(-2, -5), new(-1, -5), new(0, -5), new(1, -5), new(2, -5),new(3, -5), new(4, -5), new(5, -5),
                new(-5, -4), new(-4, -4), new(-3, -4), new(-2, -4), new(-1, -4), new(0, -4), new(1, -4), new(2, -4),new(3, -4), new(4, -4), new(5, -4),
                new(-5, -3), new(-4, -3), new(-3, -3), new(-2, -3), new(-1, -3), new(0, -3), new(1, -3), new(2, -3),new(3, -3), new(4, -3), new(5, -3),
                new(-5, -2), new(-4, -2), new(-3, -2), new(-2, -2), new(-1, -2), new(0, -2), new(1, -2), new(2, -2),new(3, -2), new(4, -2), new(5, -2),
                new(-5, -1), new(-4, -1), new(-3, -1), new(-2, -1), new(-1, -1), new(0, -1), new(1, -1), new(2, -1),new(3, -1), new(4, -1), new(5, -1),
                new(-5,  0), new(-4,  0), new(-3,  0), new(-2,  0), new(-1,  0), new(0,  0), new(1,  0), new(2,  0),new(3,  0), new(4,  0), new(5,  0),
                new(-5,  1), new(-4,  1), new(-3,  1), new(-2,  1), new(-1,  1), new(0,  1), new(1,  1), new(2,  1),new(3,  1), new(4,  1), new(5,  1),
                new(-5,  2), new(-4,  2), new(-3,  2), new(-2,  2), new(-1,  2), new(0,  2), new(1,  2), new(2,  2),new(3,  2), new(4,  2), new(5,  2),
                new(-5,  3), new(-4,  3), new(-3,  3), new(-2,  3), new(-1,  3), new(0,  3), new(1,  3), new(2,  3),new(3,  3), new(4,  3), new(5,  3),
                new(-5,  4), new(-4,  4), new(-3,  4), new(-2,  4), new(-1,  4), new(0,  4), new(1,  4), new(2,  4),new(3,  4), new(4,  4), new(5,  4),
                new(-5,  5), new(-4,  5), new(-3,  5), new(-2,  5), new(-1,  5), new(0,  5), new(1,  5), new(2,  5),new(3,  5), new(4,  5), new(5,  5),
                ];

            Chunk.Position[] positions = Chunk.Position.GenerateGridAroundCenter(c, d);
            int length = positions.Length;
            Assert.That(length, Is.EqualTo(expectedPositions.Length));

            for (int i = 0; i < length; ++i)
            {
                Chunk.Position p = positions[i],
                    pExpected = expectedPositions[i];

                Console.WriteLine($"p: ({p.X}, {p.Z}), pExpected: ({pExpected.X}, {pExpected.Z})");
                Assert.That(p, Is.EqualTo(pExpected));
            }

        }

    }

}
