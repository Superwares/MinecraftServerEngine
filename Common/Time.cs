

namespace Common
{
    public readonly struct Time : System.IEquatable<Time>
    {
        public static Time operator +(Time t1, Time t2)
        {
            // TODO: Assertion of value range
            return new(t1.Amount + t2.Amount);
        }

        public static Time operator -(Time t1, Time t2)
        {
            // TODO: Assertion of value range
            return new(t1.Amount - t2.Amount);
        }

        public static Time operator *(Time t, int amount)
        {
            // TODO: Assertion of value range
            return new(t.Amount * amount);
        }

        public static Time operator /(Time t, int amount)
        {
            // TODO: Assertion of value range
            return new(t.Amount / amount);
        }

        public static bool operator !=(Time t1, Time t2)
        {
            return t1.Amount != t2.Amount;
        }

        public static bool operator ==(Time t1, Time t2)
        {
            return t1.Amount == t2.Amount;
        }

        public static bool operator <(Time t1, Time t2)
        {
            return t1.Amount < t2.Amount;
        }

        public static bool operator >(Time t1, Time t2)
        {
            return t1.Amount > t2.Amount;
        }

        public static bool operator <=(Time t1, Time t2)
        {
            return t1.Amount <= t2.Amount;
        }

        public static bool operator >=(Time t1, Time t2)
        {
            return t1.Amount >= t2.Amount;
        }

        public static Time Zero()
        {
            return new(0);
        }

        public static Time Now()
        {
            long usec = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMicrosecond);
            return new(usec);
        }

        public static Time FromMicroseconds(long usec)
        {
            return new(usec);
        }

        public static Time FromMilliseconds(long msec)
        {
            return new(msec * 1_000);
        }

        public static Time FromSeconds(long sec)
        {
            return new(sec * 1_000_000);
        }

        public static string Symbol => "μs";

        private readonly long Amount;

        private Time(long amount)
        {
            Amount = amount;
        }

        public readonly override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return (obj is Time t) && Equals(t);
        }

        public readonly override int GetHashCode()
        {
            // TODO: Check this conversion.
            /*return base.GetHashCode();*/
            return (int) Amount;
        }

        public readonly bool Equals(Time other)
        {
            return this == other;
        }

        public readonly override string ToString()
        {
            return $"{Amount}{Symbol}";
        }
    }
}
