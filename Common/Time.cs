

namespace Common
{
    public readonly struct Time : System.IEquatable<Time>
    {
        public static Time operator +(Time t1, Time t2)
        {
            // TODO: Assertion of value range
            return new(t1._AMOUNT + t2._AMOUNT);
        }

        public static Time operator -(Time t1, Time t2)
        {
            // TODO: Assertion of value range
            return new(t1._AMOUNT - t2._AMOUNT);
        }

        public static bool operator !=(Time t1, Time t2)
        {
            return t1._AMOUNT != t2._AMOUNT;
        }

        public static bool operator ==(Time t1, Time t2)
        {
            return t1._AMOUNT == t2._AMOUNT;
        }

        public static bool operator <(Time t1, Time t2)
        {
            return t1._AMOUNT < t2._AMOUNT;
        }

        public static bool operator >(Time t1, Time t2)
        {
            return t1._AMOUNT > t2._AMOUNT;
        }

        public static bool operator <=(Time t1, Time t2)
        {
            return t1._AMOUNT <= t2._AMOUNT;
        }

        public static bool operator >=(Time t1, Time t2)
        {
            return t1._AMOUNT >= t2._AMOUNT;
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

        private readonly long _AMOUNT;

        private Time(long amount)
        {
            _AMOUNT = amount;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return (obj is Time t) && Equals(t);
        }

        public override int GetHashCode()
        {
            // TODO: Check this conversion.
            /*return base.GetHashCode();*/
            return (int) _AMOUNT;
        }

        public bool Equals(Time other)
        {
            return _AMOUNT == other._AMOUNT;
        }
    }
}
