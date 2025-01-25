

using System;

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

        public static Time operator /(Time t1, Time t2)
        {
            // TODO: Assertion of value range
            return new(t1.Amount / t2.Amount);
        }

        public static Time operator %(Time t1, Time t2)
        {
            // TODO: Assertion of value range
            return new(t1.Amount % t2.Amount);
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

        public static Time Zero => new(0);

        public readonly long Amount;

        private Time(long amount)
        {
            Amount = amount;
        }

        public readonly Time Clamp(Time min, Time max)
        {
            long amount = System.Math.Clamp(Amount, min.Amount, max.Amount);
            return new Time(amount);
        }

        public readonly string FormatToISO8601()
        {
            // Convert microseconds to TimeSpan
            TimeSpan timeSpan = TimeSpan.FromTicks(Amount * 10); // 1 tick = 100 nanoseconds

            // Assuming the microseconds represent the time elapsed since 0001-01-01 00:00:00 (Gregorian Calendar)
            DateTimeOffset dateTimeOffset = new DateTimeOffset(0001, 1, 1, 0, 0, 0, TimeSpan.Zero).Add(timeSpan);

            // Convert to ISO8601 format string
            return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fff");
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
            return (int)Amount;
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
