


namespace MinecraftServerEngine
{
    public class Item : IReadOnlyItem
    {
        public ItemType Type { get; }
        public string Name { get; }
        public string[] Lore { get; }

        public int MaxCount => Type.GetMaxStackCount();
        public int MinCount => Type.GetMinStackCount();


        public int MaxDurability { get; }
        public int _currentDurability;
        public int CurrentDurability => _currentDurability;
        public bool IsBreakable => MaxDurability > 0;
        public bool IsBreaked
        {
            get
            {
                System.Diagnostics.Debug.Assert(MaxDurability >= 0);
                System.Diagnostics.Debug.Assert(CurrentDurability >= 0);
                System.Diagnostics.Debug.Assert(MaxDurability >= CurrentDurability);

                return MaxDurability > 0 && CurrentDurability == 0;
            }
        }


        internal byte[] _hash;
        public byte[] Hash => _hash;


        // TODO: Check  additional validation or safeguards should be implemented.
        internal static byte[] GenerateHash(string input)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                // Convert byte array to a hexadecimal string
                //StringBuilder sb = new StringBuilder();
                //foreach (byte b in hashBytes)
                //{
                //    sb.Append(b.ToString("x2"));
                //}
                //return sb.ToString();

                return hashBytes;
            }
        }

        internal byte[] GenerateItemHash(
            ItemType type, string name,
            int maxDurability, int currentDurability,
            string[] lore)
        {

            string hashString = $"{type.ToString()}_{name}_";


            System.Diagnostics.Debug.Assert(maxDurability >= 0);
            System.Diagnostics.Debug.Assert(maxDurability >= currentDurability);
            if (maxDurability > 0)
            {
                hashString += $"Breakable_{maxDurability.ToString()}_{currentDurability.ToString()}";
            }

            hashString += "@";

            foreach (string line in lore)
            {
                hashString += line + "\n";
            }

            return GenerateHash(hashString);
        }

        internal static bool AreByteArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1 == null && array2 == null)
            {
                return true;
            }

            // Check if the references are the same (or both are null)
            if (ReferenceEquals(array1, array2))
            {
                return true;
            }

            // If either is null or lengths are different, return false
            if (array1 == null || array2 == null || array1.Length != array2.Length)
            {
                return false;
            }

            // Compare each element
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            //array1.SequenceEqual(array2)

            return true;
        }

        public Item(
            ItemType type, string name,
            int maxDurability, int currentDurability,
            params string[] lore)
        {
            System.Diagnostics.Debug.Assert(type.GetMaxStackCount() >= type.GetMinStackCount());

            if (maxDurability < 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(maxDurability),
                    "Max durability must be greater than or equal to 0.");
            }

            if (currentDurability < 0 || maxDurability < currentDurability)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(currentDurability),
                    "Current durability must be greater than 0 and less than or equal to max durability.");
            }

            if (name == null)
            {
                name = "";
            }

            Type = type;

            Name = name;

            MaxDurability = maxDurability;
            _currentDurability = currentDurability;

            Lore = lore;

            _hash = GenerateItemHash(type, name, maxDurability, currentDurability, lore);
        }

        public Item(
            ItemType type, string name,
            int maxDurability,
            params string[] lore)
            : this(type, name, maxDurability, maxDurability, lore)
        {

        }

        public Item(
            ItemType type, string name,
            params string[] lore)
            : this(type, name, 0, 0, lore)
        {

        }

        //public static Item Create(
        //    IReadOnlyItem item,
        //    params string[] additionalLore)
        //{
        //    string[] lore = System.Linq.Enumerable.ToArray(
        //        System.Linq.Enumerable.Concat(item.Lore, additionalLore)
        //        );

        //    return new Item(
        //        item.Type, item.Name,
        //        item.MaxDurability, item.CurrentDurability,
        //        lore);
        //}

        internal bool CheckHash(byte[] hash)
        {
            if (hash == null)
            {
                System.Diagnostics.Debug.Assert(Hash != null);
                return false;
            }

            return AreByteArraysEqual(Hash, hash);
        }

        public void Damage(int amount)
        {
            if (_currentDurability - amount <= 0)
            {
                _currentDurability = 0;
            }
            else
            {
                _currentDurability -= amount;
            }

            System.Diagnostics.Debug.Assert(_currentDurability >= 0);

            _hash = GenerateItemHash(Type, Name, MaxDurability, CurrentDurability, Lore);
        }

        public bool Equals(IReadOnlyItem other)
        {
            if (other == null)
            {
                return false;
            }

            return AreByteArraysEqual(other.Hash, Hash);
        }

        public override bool Equals(object obj)
        {
            if (obj is IReadOnlyItem other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            System.Diagnostics.Debug.Assert(Hash != null);
            if (Hash.Length == 0)
            {
                return 0;
            }

            unchecked
            {
                int hash = 17;
                foreach (byte b in Hash)
                {
                    hash = hash * 31 + b;
                }
                return hash;
            }
        }

    }
}
