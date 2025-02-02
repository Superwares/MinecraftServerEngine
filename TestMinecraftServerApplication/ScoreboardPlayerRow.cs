
using MinecraftPrimitives;

namespace TestMinecraftServerApplication
{
    public sealed class ScoreboardPlayerRow
    {
        public const int PointsPerKill = 13;
        public const int PointsPerDeath = -8;
        public const int PoinsPerSurviving = 10;

        public const int DefaultAdditionalPoints = 30;

        public readonly UserId UserId;
        public readonly string Username;
        public int Kills, Deaths, Surviving;
        public int AdditionalPoints = DefaultAdditionalPoints;

        public int TotalPoints
        {
            get
            {
                System.Diagnostics.Debug.Assert(Kills >= 0);
                System.Diagnostics.Debug.Assert(Deaths >= 0);
                System.Diagnostics.Debug.Assert(AdditionalPoints >= 0);

                return (Kills * PointsPerKill) + (Deaths * PointsPerDeath) + AdditionalPoints;
            }
        }

        public ScoreboardPlayerRow(UserId id, string username)
        {
            UserId = id;
            Username = username;
            Kills = 0; Deaths = 0; Surviving = 0;
            AdditionalPoints = DefaultAdditionalPoints;
        }
    }

}
