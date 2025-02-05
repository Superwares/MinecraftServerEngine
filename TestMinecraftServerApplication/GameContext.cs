
using Common;
using Containers;
using Sync;

using MinecraftServerEngine;
using MinecraftServerEngine.Entities;
using MinecraftServerEngine.Text;
using MinecraftServerEngine.Protocols;

namespace TestMinecraftServerApplication
{
    using Items;
    using Configs;

    public sealed class GameContext : System.IDisposable
    {
        public const int DefaultCoinAmount = 10;

        public const double DefaultPlayerAdditionalHealth = 0.0;
        public const double DefaultPlayerMaxHealth = 20.0;

        public const double DefaultSeekerAdditionalHealth = 100.0;
        public const double DefaultSeekerMaxHealth = 20.0;

        public const double BurnedSeekerMaxHealth = 80.0;
        public const double BurnedSeekerMovementSpeedIncrease = 0.1;

        public const int DefaultSeekerHints = 3;

        public const int MinPlayers = 2;
        public const int MaxPlayers = 18;
        public const int MaxRounds = MaxPlayers;


        // The number of coins awarded for a kill in the game.
        //public const int KillCoins = 59;
        public readonly static int KillCoins;

        // The default number of coins a player starts with in the game.
        //public const int DefaultCoins = 110;
        public readonly static int DefaultCoins;

        // The number of coins awarded for surviving the round in the game
        //public const int SurvivingRoundCoins = 31;
        public readonly static int RoundSurvivingCoins;

        // The additional points awarded to the seeker for winning a round in the game
        //public const int RoundSeekerWinAdditionalPoints = 55;
        public readonly static int RoundSeekerWinAdditionalPoints;
        // The number of coins awarded to the seeker for winning a round in the game
        //public const int RoundSeekerWinCoins = 210;
        public readonly static int RoundSeekerWinCoins;
        // The additional points awarded to a hider for winning a round in the game
        //public const int RoundHiderWinAdditionalPoints = 11;
        public readonly static int RoundHiderWinAdditionalPoints;
        // The number of coins awarded to a hider for winning a round in the game
        //public const int RoundHiderWinCoins = 22;
        public readonly static int RoundHiderWinCoins;


        public readonly static GameContextInventory Inventory = new(MinPlayers, MaxPlayers, MaxRounds);


        static GameContext()
        {
            int KillCoins;
            int DefaultCoins;

            int SurvivingRoundCoins;

            int RoundSeekerWinAdditionalPoints;
            int RoundSeekerWinCoins;
            int RoundHiderWinAdditionalPoints;
            int RoundHiderWinCoins;


            IConfigGame config = Config.Instance.Game;

            if (config == null)
            {
                MyConsole.Warn("Config.Game is null");

                config = new ConfigGame()
                {
                    KillCoins = 59,
                    DefaultCoins = 110,

                    Round = new ConfigGameRound()
                    {
                        SurvivingCoins = 31,

                        SeekerWinAdditionalPoints = 55,
                        SeekerWinCoins = 210,
                        HiderWinAdditionalPoints = 11,
                        HiderWinCoins = 22,
                    },

                };
            }

            KillCoins = config.KillCoins;
            DefaultCoins = config.DefaultCoins;

            SurvivingRoundCoins = config.Round.SurvivingCoins;

            RoundSeekerWinAdditionalPoints = config.Round.SeekerWinAdditionalPoints;
            RoundSeekerWinCoins = config.Round.SeekerWinCoins;
            RoundHiderWinAdditionalPoints = config.Round.HiderWinAdditionalPoints;
            RoundHiderWinCoins = config.Round.HiderWinCoins;

            if (config.KillCoins < 0)
            {
                MyConsole.Warn($"Config.Game.KillCoins value is negative: {config.KillCoins}");

                KillCoins = 59;
            }

            if (config.DefaultCoins < 0)
            {
                MyConsole.Warn($"Config.Game.DefaultCoins value is negative: {config.DefaultCoins}");

                DefaultCoins = 110;
            }

            if (config.Round.SurvivingCoins < 0)
            {
                MyConsole.Warn($"Config.Game.Round.SurvivingCoins value is negative: {config.Round.SurvivingCoins}");

                SurvivingRoundCoins = 31;
            }

            if (config.Round.SeekerWinCoins < 0)
            {
                MyConsole.Warn($"Config.Game.Round.SeekerWinCoins value is negative: {config.Round.SeekerWinCoins}");

                RoundSeekerWinCoins = 210;
            }

            if (config.Round.HiderWinCoins < 0)
            {
                MyConsole.Warn($"Config.Game.Round.HiderWinCoins value is negative: {config.Round.HiderWinCoins}");

                RoundHiderWinCoins = 22;
            }

            GameContext.KillCoins = KillCoins;
            GameContext.DefaultCoins = DefaultCoins;

            GameContext.RoundSurvivingCoins = SurvivingRoundCoins;

            GameContext.RoundSeekerWinAdditionalPoints = RoundSeekerWinAdditionalPoints;
            GameContext.RoundSeekerWinCoins = RoundSeekerWinCoins;
            GameContext.RoundHiderWinAdditionalPoints = RoundHiderWinAdditionalPoints;
            GameContext.RoundHiderWinCoins = RoundHiderWinCoins;

        }


        private bool _disposed = false;



        private readonly Locker _LockerPlayers = new();

        private readonly List<SuperPlayer> _players = new();



        private bool _ready = false;
        public bool IsReady => _ready;

        private bool _started = false;
        public bool IsStarted => _started;

        private bool _inRound = false;
        private bool _hiderWin = false, _seekerWin = false;
        public bool IsHiderWin
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_ready == true);
                System.Diagnostics.Debug.Assert(_started == true);

                return _hiderWin;
            }
        }
        public bool IsSeekerWin
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_ready == true);
                System.Diagnostics.Debug.Assert(_started == true);

                return _seekerWin;
            }
        }

        public IReadOnlyList<SuperPlayer> Players
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                return _players;
            }
        }

        public int TotalRounds
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                return _players.Length;
            }
        }


        private List<SuperPlayer> _prevSeekers = new();
        private SuperPlayer _currentSeeker = null;
        private int _currentRoundIndex = -1;
        public bool IsBeforeFirstRound
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_ready == true);

                return _currentRoundIndex <= 0;
            }
        }
        public bool IsFinalRound
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_started == true);

                return _currentRoundIndex == TotalRounds - 1;
            }
        }

        public SuperPlayer CurrentSeeker
        {
            get
            {
                System.Diagnostics.Debug.Assert(_disposed == false);

                System.Diagnostics.Debug.Assert(_started == true);

                System.Diagnostics.Debug.Assert(_currentSeeker != null);
                return _currentSeeker;
            }
        }

        private readonly Locker _LockerScoreboard = new();
        private readonly Map<UserId, ScoreboardPlayerRow> _ScoreboardByUserId = new();
        private readonly Map<string, ScoreboardPlayerRow> _ScoreboardByUsername = new();

        public GameContext()
        {
        }

        ~GameContext()
        {
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public bool AddPlayer(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();

            try
            {
                if (_ready == true || _started == true)
                {
                    return false;
                }

                if (_players.Length > MaxPlayers)
                {
                    return false;
                }

                bool exists = _players.Find(_player => _player.UserId == player.UserId, null) != null;

                if (exists == false)
                {
                    System.Diagnostics.Debug.Assert(_players != null);
                    _players.Append(player);

                    Inventory.ResetPlayerSeatsBeforeGame(_players, MinPlayers, MaxPlayers);
                }

                return exists == false;
            }
            catch (DuplicateKeyException)
            {
                return false;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

        }

        public bool RemovePlayer(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();

            try
            {
                if (_ready == true || _started == true)
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(_players != null);
                _players.Extract(player => player.UserId == userId, null);

                Inventory.ResetPlayerSeatsBeforeGame(_players, MinPlayers, MaxPlayers);
            }
            catch (KeyNotFoundException)
            {
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

            return true;
        }

        public void TeleportTo(AbstractPlayer player, int playerSeat)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(playerSeat >= 0);
            System.Diagnostics.Debug.Assert(playerSeat < GameContext.MaxPlayers);

            if (player.Gamemode != Gamemode.Spectator)
            {
                return;
            }

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();

            try
            {
                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_players.Length >= playerSeat);

                SuperPlayer targetPlayer = _players[playerSeat];

                if (object.ReferenceEquals(player, targetPlayer) == false)
                {
                    System.Diagnostics.Debug.Assert(targetPlayer != null);
                    player.Teleport(targetPlayer.Position, targetPlayer.Look);
                }
                
                // TODO: Close any opened inventory...
            }
            catch (KeyNotFoundException)
            {
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

            return ;
        }

        public bool Ready()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();
            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                if (_ready == true || _started == true)
                {
                    return false;
                }

                if (_players.Length < MinPlayers)
                {
                    return false;
                }

                System.Diagnostics.Debug.Assert(_players.Length >= MinPlayers);
                System.Diagnostics.Debug.Assert(_players.Length <= MaxPlayers);

                System.Diagnostics.Debug.Assert(_ready == false);
                _ready = true;


                return true;
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
            }

        }

        public void PlaySound(string name, int category, double volume, double pitch)
        {
            System.Diagnostics.Debug.Assert(_players != null);
            foreach (SuperPlayer player in _players)
            {
                System.Diagnostics.Debug.Assert(player != null);
                player.PlaySound(name, category, volume, pitch);
            }
        }

        public void Start()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == false);

            System.Diagnostics.Debug.Assert(_started == false);
            _started = true;

            System.Diagnostics.Debug.Assert(_prevSeekers != null);
            System.Diagnostics.Debug.Assert(_prevSeekers.Length == 0);
            System.Diagnostics.Debug.Assert(_currentSeeker == null);
            System.Diagnostics.Debug.Assert(_currentRoundIndex < 0);

            System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
            System.Diagnostics.Debug.Assert(_ScoreboardByUserId.Count == 0);
            System.Diagnostics.Debug.Assert(_ScoreboardByUsername != null);
            System.Diagnostics.Debug.Assert(_ScoreboardByUsername.Count == 0);

            System.Diagnostics.Debug.Assert(_players != null);
            foreach (SuperPlayer player in _players)
            {
                System.Diagnostics.Debug.Assert(player != null);
                ScoreboardPlayerRow row = new(player.UserId, player.Username);

                _ScoreboardByUserId.Insert(row.UserId, row);
                _ScoreboardByUsername.Insert(row.Username, row);

                System.Diagnostics.Debug.Assert(player != null);
                player.Reset();

                System.Diagnostics.Debug.Assert(player != null);
                System.Diagnostics.Debug.Assert(DefaultCoins >= 0);
                player.GiveItemStacks(Coin.Item, DefaultCoins);
            }

            System.Diagnostics.Debug.Assert(_players != null);
            Inventory.StartGame(_players, TotalRounds, _ScoreboardByUserId);

            PlaySound("entity.player.levelup", 0, 1.0, 1.5);
        }

        public void StartRound()
        {

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_prevSeekers != null);
            System.Diagnostics.Debug.Assert(_currentSeeker == null);
            ++_currentRoundIndex;

            System.Diagnostics.Debug.Assert(_inRound == false);
            _inRound = true;

            _hiderWin = false;
            _seekerWin = false;

            System.Diagnostics.Debug.Assert(_players != null);
            foreach (SuperPlayer player in _players)
            {
                player.SwitchGamemode(Gamemode.Adventure);

                player.SetAdditionalHealth(DefaultPlayerAdditionalHealth);
                player.SetMaxHealth(DefaultPlayerMaxHealth);
                player.ResetMovementSpeed();

                player.HealFully();
            }

            System.Diagnostics.Debug.Assert(Inventory != null);
            System.Diagnostics.Debug.Assert(_players != null);
            Inventory.StartRound(_players, TotalRounds, _currentRoundIndex);

        }

        public int MakeNonSeekerIndexList(List<int> indices)
        {
            System.Diagnostics.Debug.Assert(indices != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            bool prevSeeker = false;
            for (int i = 0; i < _players.Length; ++i)
            {
                for (int j = 0; j < _prevSeekers.Length; ++j)
                {
                    if (object.ReferenceEquals(_prevSeekers[j], _players[i]) == true)
                    {
                        prevSeeker = true;
                        break;
                    }


                }

                if (prevSeeker == true)
                {
                    prevSeeker = false;
                    continue;
                }

                indices.Append(i);
            }

            return _players.Length - _prevSeekers.Length;
        }

        public void SeletSeeker(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker == null);
            _currentSeeker = player;

            _currentSeeker.SetAdditionalHealth(DefaultSeekerAdditionalHealth);
            _currentSeeker.SetMaxHealth(DefaultSeekerMaxHealth);
            player.ResetMovementSpeed();

            _currentSeeker.HealFully();

            System.Diagnostics.Debug.Assert(DefaultSeekerHints >= 0);
            _currentSeeker.GiveItemStacks(Hint.Item, DefaultSeekerHints);
        }

        public void StartSeekerCount()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker != null);
            _currentSeeker.ApplyBilndness(true);

        }

        public void EndSeekerCount()
        {

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker != null);
            _currentSeeker.ApplyBilndness(false);


        }

        public void HandleKillEvent(SuperPlayer player)
        {
            System.Diagnostics.Debug.Assert(player != null);

            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                if (_inRound == false)
                {
                    return;
                }

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                ScoreboardPlayerRow row = _ScoreboardByUserId.Lookup(player.UserId);

                ++row.Kills;

                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                Inventory.UpdatePlayerScores(_players, _ScoreboardByUserId);

                System.Diagnostics.Debug.Assert(player != null);
                System.Diagnostics.Debug.Assert(KillCoins >= 0);
                player.GiveItemStacks(Coin.Item, KillCoins);

                player.WriteMessageInChatBox([
                    new TextComponent($"킬! (+{GameContext.KillCoins}코인)", TextColor.DarkGreen),
                    ]);

            }
            catch (KeyNotFoundException)
            {

            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }

        }

        public void HandleKillEventForSeeker()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                if (_inRound == false)
                {
                    return;
                }

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                System.Diagnostics.Debug.Assert(_currentSeeker != null);
                ScoreboardPlayerRow row = _ScoreboardByUserId.Lookup(_currentSeeker.UserId);

                ++row.Kills;

                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                Inventory.UpdatePlayerScores(_players, _ScoreboardByUserId);

                System.Diagnostics.Debug.Assert(_currentSeeker != null);
                System.Diagnostics.Debug.Assert(KillCoins >= 0);
                _currentSeeker.GiveItemStacks(Coin.Item, KillCoins);

                _currentSeeker.WriteMessageInChatBox([
                    new TextComponent($"킬! (+{GameContext.KillCoins}코인)", TextColor.DarkGreen),
                    ]);

            }
            catch (KeyNotFoundException)
            {

            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }

        }

        public void HandleDeathEvent(UserId userId)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                if (_inRound == false)
                {
                    return;
                }

                if (_hiderWin == true || _seekerWin == true)
                {
                    return;
                }

                System.Diagnostics.Debug.Assert(_currentSeeker != null);

                if (_currentSeeker.UserId == userId)
                {

                    foreach (SuperPlayer player in _players)
                    {
                        System.Diagnostics.Debug.Assert(player != null);
                        System.Diagnostics.Debug.Assert(_currentSeeker != null);
                        if (
                            object.ReferenceEquals(player, _currentSeeker) == true ||
                            player.Gamemode != Gamemode.Adventure
                            )
                        {
                            continue;
                        }

                        System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                        ScoreboardPlayerRow hiderPlayerScoreRow = _ScoreboardByUserId.Lookup(player.UserId);

                        System.Diagnostics.Debug.Assert(hiderPlayerScoreRow != null);
                        System.Diagnostics.Debug.Assert(RoundHiderWinAdditionalPoints >= 0);
                        hiderPlayerScoreRow.AdditionalPoints += RoundHiderWinAdditionalPoints;

                        System.Diagnostics.Debug.Assert(_currentSeeker != null);
                        System.Diagnostics.Debug.Assert(RoundHiderWinCoins >= 0);
                        _currentSeeker.GiveItemStacks(Coin.Item, RoundHiderWinCoins);

                        player.WriteMessageInChatBox([
                            new TextComponent($"생존승리! (+{GameContext.RoundHiderWinAdditionalPoints}포인트, +{GameContext.RoundHiderWinCoins}코인)", TextColor.DarkGreen),
                            ]);
                    }

                    _hiderWin = true;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(_seekerWin != true);
                    _seekerWin = true;

                    System.Diagnostics.Debug.Assert(_players != null);
                    foreach (SuperPlayer player in _players)
                    {
                        System.Diagnostics.Debug.Assert(player != null);
                        System.Diagnostics.Debug.Assert(_currentSeeker != null);
                        if (object.ReferenceEquals(player, _currentSeeker) == true)
                        {
                            continue;
                        }

                        if (player.Gamemode == Gamemode.Adventure)
                        {
                            _seekerWin = false;
                            break;
                        }
                    }

                    if (_seekerWin == true)
                    {
                        System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                        ScoreboardPlayerRow seekerPlayerScoreRow = _ScoreboardByUserId.Lookup(_currentSeeker.UserId);

                        System.Diagnostics.Debug.Assert(seekerPlayerScoreRow != null);
                        System.Diagnostics.Debug.Assert(RoundSeekerWinAdditionalPoints >= 0);
                        seekerPlayerScoreRow.AdditionalPoints += RoundSeekerWinAdditionalPoints;

                        System.Diagnostics.Debug.Assert(_currentSeeker != null);
                        System.Diagnostics.Debug.Assert(RoundSeekerWinCoins >= 0);
                        _currentSeeker.GiveItemStacks(Coin.Item, RoundSeekerWinCoins);

                        _currentSeeker.WriteMessageInChatBox([
                            new TextComponent($"술래승리! (+{GameContext.RoundSeekerWinAdditionalPoints}포인트, +{GameContext.RoundSeekerWinCoins}코인)", TextColor.DarkGreen),
                            ]);

                    }

                }

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                ScoreboardPlayerRow deathPlayerRow = _ScoreboardByUserId.Lookup(userId);

                ++deathPlayerRow.Deaths;

                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                Inventory.UpdatePlayerScores(_players, _ScoreboardByUserId);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }
        }

        public void StartBuringTime()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_currentSeeker != null);
            _currentSeeker.SetMaxHealth(BurnedSeekerMaxHealth);
            _currentSeeker.AddMovementSpeed(BurnedSeekerMovementSpeedIncrease);
            _currentSeeker.HealFully();
        }

        public void EndRound()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {

                System.Diagnostics.Debug.Assert(_players != null);
                foreach (SuperPlayer player in _players)
                {
                    System.Diagnostics.Debug.Assert(player != null);
                    System.Diagnostics.Debug.Assert(_currentSeeker != null);
                    if (object.ReferenceEquals(player, _currentSeeker) == true)
                    {
                        continue;
                    }

                    if (player.Gamemode != Gamemode.Adventure)
                    {
                        continue;
                    }

                    System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                    ScoreboardPlayerRow survivorPlayerScoreRow = _ScoreboardByUserId.Lookup(player.UserId);

                    System.Diagnostics.Debug.Assert(survivorPlayerScoreRow != null);
                    ++survivorPlayerScoreRow.Surviving;

                    System.Diagnostics.Debug.Assert(_currentSeeker != null);
                    System.Diagnostics.Debug.Assert(RoundSurvivingCoins >= 0);
                    _currentSeeker.GiveItemStacks(Coin.Item, RoundSurvivingCoins);

                    player.WriteMessageInChatBox([
                        new TextComponent($"생존! (+{GameContext.RoundSurvivingCoins}코인)", TextColor.DarkGreen),
                        ]);
                }

                System.Diagnostics.Debug.Assert(_prevSeekers != null);
                System.Diagnostics.Debug.Assert(_currentSeeker != null);
                _prevSeekers.Append(_currentSeeker);

                _currentSeeker = null;
                _hiderWin = false;
                _seekerWin = false;

                System.Diagnostics.Debug.Assert(_currentRoundIndex >= 0);

                System.Diagnostics.Debug.Assert(Inventory != null);
                Inventory.EndRound(_players, TotalRounds, _currentRoundIndex);

                System.Diagnostics.Debug.Assert(_inRound == true);
                _inRound = false;

                System.Diagnostics.Debug.Assert(_players != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                Inventory.UpdatePlayerScores(_players, _ScoreboardByUserId);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }
        }

        public void DetermineWinners(List<SuperPlayer> players)
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_ready == true);
            System.Diagnostics.Debug.Assert(_started == true);

            System.Diagnostics.Debug.Assert(IsFinalRound == true);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();
            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {
                int totalPoints = int.MinValue;

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                foreach ((UserId _, ScoreboardPlayerRow row) in _ScoreboardByUserId.GetElements())
                {
                    if (totalPoints < row.TotalPoints)
                    {
                        totalPoints = row.TotalPoints;
                    }
                }

                foreach (SuperPlayer player in _players)
                {
                    System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                    ScoreboardPlayerRow row = _ScoreboardByUserId.Lookup(player.UserId);

                    if (totalPoints == row.TotalPoints)
                    {
                        players.Append(player);
                    }
                }

                System.Diagnostics.Debug.Assert(players.Length > 0);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }
        }

        public void End()
        {
            System.Diagnostics.Debug.Assert(_disposed == false);

            System.Diagnostics.Debug.Assert(_LockerPlayers != null);
            _LockerPlayers.Hold();
            System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
            _LockerScoreboard.Hold();

            try
            {

                System.Diagnostics.Debug.Assert(_players != null);
                foreach (SuperPlayer player in _players)
                {
                    System.Diagnostics.Debug.Assert(player != null);
                    player.Reset();
                }

                System.Diagnostics.Debug.Assert(_ready == true);
                System.Diagnostics.Debug.Assert(_started == true);
                _started = false;
                _ready = false;

                _hiderWin = false;
                _seekerWin = false;

                _prevSeekers.Flush();
                _currentSeeker = null;
                _currentRoundIndex = -1;

                System.Diagnostics.Debug.Assert(_ScoreboardByUserId != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId.Count >= MinPlayers);
                System.Diagnostics.Debug.Assert(_ScoreboardByUserId.Count <= MaxPlayers);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername != null);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername.Count >= MinPlayers);
                System.Diagnostics.Debug.Assert(_ScoreboardByUsername.Count <= MaxPlayers);
                _ScoreboardByUserId.Flush();
                _ScoreboardByUsername.Flush();

                System.Diagnostics.Debug.Assert(Inventory != null);
                Inventory.Reset(_players, MinPlayers, MaxPlayers, MaxRounds);
            }
            finally
            {
                System.Diagnostics.Debug.Assert(_LockerPlayers != null);
                _LockerPlayers.Release();
                System.Diagnostics.Debug.Assert(_LockerScoreboard != null);
                _LockerScoreboard.Release();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed == false)
            {
                System.Diagnostics.Debug.Assert(_started == false);

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing == true)
                {
                    // Dispose managed resources.
                    _LockerPlayers.Dispose();
                    _players.Dispose();

                    _prevSeekers.Dispose();

                    _LockerScoreboard.Dispose();
                    _ScoreboardByUserId.Dispose();
                    _ScoreboardByUsername.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                //CloseHandle(handle);
                //handle = IntPtr.Zero;

                // Note disposing has been done.
                _disposed = true;
            }

        }

    }
}
