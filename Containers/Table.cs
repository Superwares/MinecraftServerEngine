using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Containers
{
    public class Table<K, V>
    {
        private static readonly int _InitSize;

        private bool[] _flags;
        private K?[] _keys;
        private V?[] _values;
        private int _length;
        private int _count;

        public Table()
        {

        }
    }
}
