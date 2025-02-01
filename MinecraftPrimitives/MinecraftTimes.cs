using Common;

namespace MinecraftPrimitives
{
    public static class MinecraftTimes
    {
        public readonly static Time TimePerTick = Time.FromMilliseconds(50);

        //public readonly static Time OneSecond = Time.FromMicroseconds(13_888);   // Real time: 0.013888888... seconds
        //public readonly static Time OneMinute = Time.FromMicroseconds(833_333);  // Real time: 0.833333333... seconds
        public readonly static Time OneHour = Time.FromSeconds(50);             // Real time: 50 seconds
        public readonly static Time OneDay = Time.FromMinutes(20);              // Real time: 20 minutes

        public readonly static Time DaytimeStart = Time.FromMicroseconds(0);          // 0 ticks (06:00:00.0)
        public readonly static Time DaytimeMid = Time.FromMicroseconds(300_000_000);  // 6000 ticks (12:00:00.0)
        public readonly static Time DaytimeEnd = Time.FromMicroseconds(600_000_000);  // 12000 ticks (18:00:00.0)

        public readonly static Time SunsetStart = Time.FromMicroseconds(600_000_000);  // 12000 ticks (18:00:00.0)
        public readonly static Time SunsetEnd = Time.FromMicroseconds(650_000_000);    // 13000 ticks (19:00:00.0)

        public readonly static Time NighttimeStart = Time.FromMicroseconds(650_000_000);  // 13000 ticks (19:00:00.0)
        public readonly static Time NighttimeMid = Time.FromMicroseconds(900_000_000);    // 18000 ticks (00:00:00.0)
        public readonly static Time NighttimeEnd = Time.FromMicroseconds(1_150_000_000);  // 23000 ticks (05:00:00.0)

        public readonly static Time SunriseStart = Time.FromMicroseconds(1_150_000_000);  // 23000 ticks (05:00:00.0)
        public readonly static Time SunriseEnd = Time.FromMicroseconds(1_200_000_000);    // 24000 (0) ticks (06:00:00.0)


        static MinecraftTimes()
        {
            System.Diagnostics.Debug.Assert(DaytimeStart == Time.Zero);
            System.Diagnostics.Debug.Assert(DaytimeStart < DaytimeMid);
            System.Diagnostics.Debug.Assert(DaytimeMid < DaytimeEnd);
            System.Diagnostics.Debug.Assert(DaytimeEnd <= SunsetStart);

            System.Diagnostics.Debug.Assert(SunsetStart < SunsetEnd);
            System.Diagnostics.Debug.Assert(SunsetEnd <= NighttimeStart);

            System.Diagnostics.Debug.Assert(NighttimeStart < NighttimeMid);
            System.Diagnostics.Debug.Assert(NighttimeMid < NighttimeEnd);
            System.Diagnostics.Debug.Assert(NighttimeEnd <= SunriseStart);

            System.Diagnostics.Debug.Assert(SunriseStart < SunriseEnd);
            System.Diagnostics.Debug.Assert(SunriseEnd == OneDay);
        }

        public static long ToTicks(Time time)
        {
            return time.Amount / TimePerTick.Amount;
        }
    }
}
