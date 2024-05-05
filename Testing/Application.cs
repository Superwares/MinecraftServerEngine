using Application;
using System;
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
                    b = new(['H', 'e', 'l', 'l', 'o', ',', ' ', 'W', 'o', 'r', 'l', 'd', '!']);

                Assert.That(a.Equals(b), Is.EqualTo(true));
                Assert.That(a == b, Is.EqualTo(true));
            }

            {
                // Reference Types
                object a = "Hello, World!",
                    b = new string([ 'H','e','l','l','o',',',' ','W','o','r','l','d','!' ]);

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