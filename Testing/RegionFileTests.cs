using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftUtils
{
    internal class RegionFileTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            FileInfo fileInfo = new("C:\\Users\\welco\\Documents\\Superwares\\MinecraftServerEngine\\Testing\\r.0.0.mca");
            RegionFile file = new(fileInfo);
            Assert.IsNotNull(file);

            byte[] data = file.ReadChunk(1, 30);

            Console.WriteLine("data:");
            Console.WriteLine(data);
        }
   }
}
