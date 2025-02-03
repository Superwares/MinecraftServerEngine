
using Common;
using MinecraftPrimitives;

namespace TestMinecraftServerApplication
{
    public sealed class ScoreboardPlayerRow
    {
        //public const int PointsPerKill = 13;
        //public const int PointsPerDeath = -8;
        //public const int PoinsPerSurviving = 10;

        //public const int DefaultAdditionalPoints = 30;

        public readonly static int PointsPerKill;
        public readonly static int PointsPerDeath;
        public readonly static int PoinsPerSurviving;

        public readonly static int DefaultAdditionalPoints;

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

                return (Kills * PointsPerKill) + (Deaths * PointsPerDeath) + (Surviving * PoinsPerSurviving) + AdditionalPoints;
            }
        }

        static ScoreboardPlayerRow()
        {
            int PointsPerKill;
            int PointsPerDeath;
            int PoinsPerSurviving;
            int DefaultAdditionalPoints;


            IConfigGameScroeboard config = Config.Instance.Game?.Scroeboard;

            if (config == null)
            {
                MyConsole.Warn("Config.Game.Scroeboard is null");

                config = new ConfigGameScroeboard()
                {
                    PointsPerKill = 13,
                    PointsPerDeath = -8,
                    PoinsPerSurviving = 10,

                    DefaultAdditionalPoints = 30,
                };
            }

            PointsPerKill = config.PointsPerKill;
            PointsPerDeath = config.PointsPerDeath;
            PoinsPerSurviving = config.PoinsPerSurviving;
            DefaultAdditionalPoints = config.DefaultAdditionalPoints;

            ScoreboardPlayerRow.PointsPerKill = PointsPerKill;
            ScoreboardPlayerRow.PointsPerDeath = PointsPerDeath;
            ScoreboardPlayerRow.PoinsPerSurviving = PoinsPerSurviving;

            ScoreboardPlayerRow.DefaultAdditionalPoints = DefaultAdditionalPoints;
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
