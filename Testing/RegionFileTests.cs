using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftPrimitives
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
            FileInfo fileInfo = new("C:\\Users\\Peach\\Documents\\Superwares\\MinecraftServerEngine\\Testing\\r.0.0.mca");

            NBTTagCompound tag = NBTTagCompoundLoader.Load(fileInfo, 1, 1);

            if (tag != null)
            {
                Console.WriteLine(tag);
            }


        }
    }
}
