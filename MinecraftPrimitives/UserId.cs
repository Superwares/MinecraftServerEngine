namespace MinecraftPrimitives
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

        public readonly System.Guid Value;

        public UserId(System.Guid guid)
        {
            Value = guid;
        }

        public readonly bool Equals(UserId other)
        {
            return Value.Equals(other.Value);
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
            return Value.ToString();
        }
    }
}
