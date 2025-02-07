
using Common;

using MinecraftServerEngine.Protocols;

namespace TestMinecraftServerApplication
{
    using Configs;

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
            //int PointsPerKill;
            //int PointsPerDeath;
            //int PoinsPerSurviving;
            //int DefaultAdditionalPoints;


            //IConfigGameScoreboard config = ConfigXml.GetConfig().Game?.Scoreboard;

            //if (config == null)
            //{
            //    MyConsole.Warn("Config.Game.Scroeboard is null");

            //    config = new ConfigGameScoreboard()
            //    {
            //        PointsPerKill = 13,
            //        PointsPerDeath = -8,
            //        PoinsPerSurviving = 10,

            //        DefaultAdditionalPoints = 30,
            //    };
            //}

            //PointsPerKill = config.PointsPerKill;
            //PointsPerDeath = config.PointsPerDeath;
            //PoinsPerSurviving = config.PoinsPerSurviving;
            //DefaultAdditionalPoints = config.DefaultAdditionalPoints;

            //ScoreboardPlayerRow.PointsPerKill = PointsPerKill;
            //ScoreboardPlayerRow.PointsPerDeath = PointsPerDeath;
            //ScoreboardPlayerRow.PoinsPerSurviving = PoinsPerSurviving;

            //ScoreboardPlayerRow.DefaultAdditionalPoints = DefaultAdditionalPoints;

            ConfigGameScoreboard config = ConfigXml.GetConfig().Game.Scoreboard;

            PointsPerKill = config.PointsPerKill;
            PointsPerDeath = config.PointsPerDeath;
            PoinsPerSurviving = config.PoinsPerSurviving;

            DefaultAdditionalPoints = config.DefaultAdditionalPoints;
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
