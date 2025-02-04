

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{

    public sealed class WinnerStage : IGameStage
    {
        public readonly static Time WinnerDisplayDuration = Time.FromSeconds(5);
        public readonly static Time FireworkLaunchTime_0 = Time.FromMilliseconds(100);
        public readonly static Time FireworkLaunchTime_1 = Time.FromMilliseconds(310);
        public readonly static Time FireworkLaunchTime_2 = Time.FromMilliseconds(790);
        public readonly static Time FireworkLaunchTime_3 = Time.FromMilliseconds(950);
        public readonly static Time FireworkLaunchTime_4 = Time.FromMilliseconds(1140);
        public readonly static Time FireworkLaunchTime_5 = Time.FromMilliseconds(1590);
        public readonly static Time FireworkLaunchTime_6 = Time.FromMilliseconds(1890);
        public readonly static Time FireworkLaunchTime_7 = Time.FromMilliseconds(2190);
        public readonly static Time FireworkLaunchTime_8 = Time.FromMilliseconds(2590);
        public readonly static Time FireworkLaunchTime_9 = Time.FromMilliseconds(3310);

        public readonly static Time FireworkBlastTime_0 = Time.FromMilliseconds(610);
        public readonly static Time FireworkBlastTime_1 = Time.FromMilliseconds(1040);
        public readonly static Time FireworkBlastTime_2 = Time.FromMilliseconds(1370);
        public readonly static Time FireworkBlastTime_3 = Time.FromMilliseconds(1610);
        public readonly static Time FireworkBlastTime_4 = Time.FromMilliseconds(1820);
        public readonly static Time FireworkBlastTime_5 = Time.FromMilliseconds(2390);
        public readonly static Time FireworkBlastTime_6 = Time.FromMilliseconds(2790);
        public readonly static Time FireworkBlastTime_7 = Time.FromMilliseconds(3110);
        public readonly static Time FireworkBlastTime_8 = Time.FromMilliseconds(3500);
        public readonly static Time FireworkBlastTime_9 = Time.FromMilliseconds(3720);

        public readonly static Time FireworkTwinkleTime_0 = Time.FromMilliseconds(740);
        public readonly static Time FireworkTwinkleTime_1 = Time.FromMilliseconds(1140);
        public readonly static Time FireworkTwinkleTime_2 = Time.FromMilliseconds(1570);
        public readonly static Time FireworkTwinkleTime_3 = Time.FromMilliseconds(1710);
        public readonly static Time FireworkTwinkleTime_4 = Time.FromMilliseconds(1920);
        public readonly static Time FireworkTwinkleTime_5 = Time.FromMilliseconds(2590);
        public readonly static Time FireworkTwinkleTime_6 = Time.FromMilliseconds(2900);
        public readonly static Time FireworkTwinkleTime_7 = Time.FromMilliseconds(3210);
        public readonly static Time FireworkTwinkleTime_8 = Time.FromMilliseconds(3600);
        public readonly static Time FireworkTwinkleTime_9 = Time.FromMilliseconds(3820);

        private readonly System.Random _Random = new();

        private Time _startTime = Time.Now();


        private readonly List<SuperPlayer> _Winners = new();
        private bool _displayWinner = false;
        private SuperPlayer _winner = null;

        private bool _init = false;


        private Time _intervalTime = Time.FromMilliseconds(250);
        private Time _time;

        private int _repeat = 3;
        private int i = 0;


        private bool _fireworkLaunch_0 = false;
        private bool _fireworkLaunch_1 = false;
        private bool _fireworkLaunch_2 = false;
        private bool _fireworkLaunch_3 = false;
        private bool _fireworkLaunch_4 = false;
        private bool _fireworkLaunch_5 = false;
        private bool _fireworkLaunch_6 = false;
        private bool _fireworkLaunch_7 = false;
        private bool _fireworkLaunch_8 = false;
        private bool _fireworkLaunch_9 = false;

        private bool _fireworkBlast_0 = false;
        private bool _fireworkBlast_1 = false;
        private bool _fireworkBlast_2 = false;
        private bool _fireworkBlast_3 = false;
        private bool _fireworkBlast_4 = false;
        private bool _fireworkBlast_5 = false;
        private bool _fireworkBlast_6 = false;
        private bool _fireworkBlast_7 = false;
        private bool _fireworkBlast_8 = false;
        private bool _fireworkBlast_9 = false;

        private bool _fireworkTwinkle_0 = false;
        private bool _fireworkTwinkle_1 = false;
        private bool _fireworkTwinkle_2 = false;
        private bool _fireworkTwinkle_3 = false;
        private bool _fireworkTwinkle_4 = false;
        private bool _fireworkTwinkle_5 = false;
        private bool _fireworkTwinkle_6 = false;
        private bool _fireworkTwinkle_7 = false;
        private bool _fireworkTwinkle_8 = false;
        private bool _fireworkTwinkle_9 = false;


        public IGameStage CreateNextNode(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            _Winners.Dispose();

            return new GameEndStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            Time elapsedTime = Time.Now() - _startTime;

            if (_winner != null)
            {
                if (_displayWinner == false)
                {
                    System.Diagnostics.Debug.Assert(_winner != null);
                    world.DisplayTitle(
                        Time.Zero, Time.FromSeconds(5), Time.FromSeconds(5),
                        new TextComponent($"! {_winner.Username} !", TextColor.BrightGreen));

                    ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);

                    _displayWinner = true;
                }

                if (_fireworkLaunch_0 == false && elapsedTime > FireworkLaunchTime_0)
                {
                    _fireworkLaunch_0 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_1 == false && elapsedTime > FireworkLaunchTime_1)
                {
                    _fireworkLaunch_1 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_2 == false && elapsedTime > FireworkLaunchTime_2)
                {
                    _fireworkLaunch_2 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_3 == false && elapsedTime > FireworkLaunchTime_3)
                {
                    _fireworkLaunch_3 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_4 == false && elapsedTime > FireworkLaunchTime_4)
                {
                    _fireworkLaunch_4 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_5 == false && elapsedTime > FireworkLaunchTime_5)
                {
                    _fireworkLaunch_5 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_6 == false && elapsedTime > FireworkLaunchTime_6)
                {
                    _fireworkLaunch_6 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_7 == false && elapsedTime > FireworkLaunchTime_7)
                {
                    _fireworkLaunch_7 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_8 == false && elapsedTime > FireworkLaunchTime_8)
                {
                    _fireworkLaunch_8 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                }
                else if (_fireworkLaunch_9 == false && elapsedTime > FireworkLaunchTime_9)
                {
                    _fireworkLaunch_9 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                }

                if (_fireworkBlast_0 == false && elapsedTime > FireworkBlastTime_0)
                {
                    _fireworkBlast_0 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_1 == false && elapsedTime > FireworkBlastTime_1)
                {
                    _fireworkBlast_1 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_2 == false && elapsedTime > FireworkBlastTime_2)
                {
                    _fireworkBlast_2 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_3 == false && elapsedTime > FireworkBlastTime_3)
                {
                    _fireworkBlast_3 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_4 == false && elapsedTime > FireworkBlastTime_4)
                {
                    _fireworkBlast_4 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_5 == false && elapsedTime > FireworkBlastTime_5)
                {
                    _fireworkBlast_5 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_6 == false && elapsedTime > FireworkBlastTime_6)
                {
                    _fireworkBlast_6 = true;

                    ctx.PlaySound("entity.firework.launch", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_7 == false && elapsedTime > FireworkBlastTime_7)
                {
                    _fireworkBlast_7 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_8 == false && elapsedTime > FireworkBlastTime_8)
                {
                    _fireworkBlast_8 = true;

                    //ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkBlast_9 == false && elapsedTime > FireworkBlastTime_9)
                {
                    _fireworkBlast_9 = true;

                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }

                if (_fireworkTwinkle_0 == false && elapsedTime > FireworkTwinkleTime_0)
                {
                    _fireworkTwinkle_0 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_1 == false && elapsedTime > FireworkTwinkleTime_1)
                {
                    _fireworkTwinkle_1 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_2 == false && elapsedTime > FireworkTwinkleTime_2)
                {
                    _fireworkTwinkle_2 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_3 == false && elapsedTime > FireworkTwinkleTime_3)
                {
                    _fireworkTwinkle_3 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_4 == false && elapsedTime > FireworkTwinkleTime_4)
                {
                    _fireworkTwinkle_4 = true;

                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_5 == false && elapsedTime > FireworkTwinkleTime_5)
                {
                    _fireworkTwinkle_5 = true;

                    ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_6 == false && elapsedTime > FireworkTwinkleTime_6)
                {
                    _fireworkTwinkle_6 = true;

                    ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_7 == false && elapsedTime > FireworkTwinkleTime_7)
                {
                    _fireworkTwinkle_7 = true;

                    ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.large_blast_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_8 == false && elapsedTime > FireworkTwinkleTime_8)
                {
                    _fireworkTwinkle_8 = true;

                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                }
                else if (_fireworkTwinkle_9 == false && elapsedTime > FireworkTwinkleTime_9)
                {
                    _fireworkTwinkle_9 = true;

                    ctx.PlaySound("entity.firework.large_blast", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.blast_far", 0, 0.5, 1.0);
                    //ctx.PlaySound("entity.firework.twinkle", 0, 0.5, 1.0);
                    ctx.PlaySound("entity.firework.twinkle_far", 0, 0.5, 1.0);
                }

                if (elapsedTime > WinnerDisplayDuration)
                {
                    return true;
                }

                return false;
            }


            if (_Winners.Length > 0)
            {
                if (_Winners.Length == 1)
                {
                    _winner = _Winners.Shift(null);

                    System.Diagnostics.Debug.Assert(_winner != null);

                    _startTime = Time.Now();

                    return false;
                }

                if (i >= _Winners.Length * _repeat)
                {
                    int j = _Random.Next(_Winners.Length);

                    SuperPlayer extracted = _Winners.Extract(j, null);

                    System.Diagnostics.Debug.Assert(extracted != null);

                    _intervalTime += Time.FromMilliseconds(100);

                    //_repeat += 1;
                    i = 0;

                    ctx.PlaySound("entity.item.pickup", 0, 0.5, 1.0);
                }
                else
                {
                    Time time = Time.Now() - _time;

                    if (time > _intervalTime)
                    {
                        int j = i % _Winners.Length;
                        SuperPlayer player = _Winners[j];

                        //MyConsole.Debug($"j: {j}");

                        world.DisplayTitle(
                            Time.Zero, _intervalTime, Time.Zero,
                            new TextComponent($"{player.Username}", TextColor.Gray));

                        ++i;

                        _time = Time.Now();

                        ctx.PlaySound("entity.item.pickup", 0, 0.5, 1.0);
                    }

                }

                return false;
            }



            if (_init == false)
            {
                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(4), Time.FromSeconds(1),
                    new TextComponent($"우승자는!", TextColor.Gold));

                ctx.PlaySound("block.note.pling", 0, 0.5, 1.0);

                _init = true;
            }

            if (elapsedTime > Time.FromSeconds(5))
            {
                System.Diagnostics.Debug.Assert(_Winners.Length == 0);
                ctx.DetermineWinners(_Winners);

                System.Diagnostics.Debug.Assert(_Winners.Length > 0);

                _intervalTime /= _Winners.Length;
                _time = Time.Now() - _intervalTime;

                ctx.PlaySound("block.note.pling", 0, 0.5, 1.0);
            }

            return false;
        }
    }

}
