namespace MinecraftServerEngine
{
    public readonly struct UserId : System.IEquatable<UserId>
    {
        public static bool operator ==(UserId id1, UserId id2)
        {
            return id1.Equals(id2);
        }

        public static bool operator !=(UserId id1, UserId id2)
        {
            return !id1.Equals(id2);
        }

        public static readonly UserId Null = new(System.Guid.Empty);

        public static UserId Random()
        {
            return new(System.Guid.NewGuid());
        }

        internal readonly System.Guid Data;

        public UserId(System.Guid guid)
        {
            Data = guid;
        }

        public readonly bool Equals(UserId other)
        {
            return Data.Equals(other.Data);
        }

        public readonly override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj is UserId idUser && Equals(idUser);
        }

        public readonly override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public readonly override string ToString()
        {
            return Data.ToString();
        }
    }
}
