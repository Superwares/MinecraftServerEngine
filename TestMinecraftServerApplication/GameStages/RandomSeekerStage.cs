

using Common;
using Containers;

using MinecraftServerEngine;
using MinecraftServerEngine.ProgressBars;
using MinecraftServerEngine.Text;

namespace TestMinecraftServerApplication.GameStages
{


    public sealed class RandomSeekerStage : IGameStage
    {
        private readonly System.Random _Random = new();

        private readonly List<int> _NonSeekerIndexList;

        public readonly TextComponent[] Message = [
            new TextComponent($"============SUPERDEK=============\n", TextColor.DarkGray),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"모든 플레이어는 무작위로 술래로 지정됩니다.\n",  TextColor.White),
            new TextComponent($"단, 한 번 술래가 된 플레이어는 다음 라운드\n", TextColor.White),
            new TextComponent($"에서는 술래로 선정되지 않습니다.\n", TextColor.White),
            new TextComponent($"\n", TextColor.White),
            new TextComponent($"=================================", TextColor.DarkGray),
        ];
        private bool printMessage = false;

        private Time _intervalTime = Time.FromMilliseconds(250);
        private Time _time;

        private int _repeat = 3;
        private int i = 0;


        public RandomSeekerStage(List<int> nonSeekerIndexList)
        {
            System.Diagnostics.Debug.Assert(nonSeekerIndexList != null);
            _NonSeekerIndexList = nonSeekerIndexList;

            _intervalTime /= nonSeekerIndexList.Length;
            _time = Time.Now() - _intervalTime;

        }

        public IGameStage CreateNextStage(GameContext ctx)
        {
            System.Diagnostics.Debug.Assert(ctx != null);

            System.Diagnostics.Debug.Assert(_NonSeekerIndexList != null);
            _NonSeekerIndexList.Flush();
            _NonSeekerIndexList.Dispose();

            return new SeekerCountStage();
        }

        public bool StartRoutine(GameContext ctx, SuperWorld world)
        {
            System.Diagnostics.Debug.Assert(ctx != null);
            System.Diagnostics.Debug.Assert(world != null);

            if (ctx.IsBeforeFirstRound == true && printMessage == false)
            {
                world.WriteMessageInChatBox(Message);

                printMessage = true;
            }

            System.Diagnostics.Debug.Assert(ctx.Players != null);
            IReadOnlyList<SuperPlayer> players = ctx.Players;

            if (_NonSeekerIndexList.Length == 1)
            {
                int j = _NonSeekerIndexList[0];

                SuperPlayer seeker = players[j];

                world.DisplayTitle(
                    Time.Zero, Time.FromSeconds(1), Time.FromSeconds(1),
                    new TextComponent($"{seeker.Username}", TextColor.DarkGreen));

                ctx.SeletSeeker(seeker);

                ctx.PlaySound("entity.player.levelup", 0, 0.5, 1.0);

                return true;
            }

            if (i >= _NonSeekerIndexList.Length * _repeat)
            {
                int j = _Random.Next(_NonSeekerIndexList.Length);
                int k = _NonSeekerIndexList[j];

                _NonSeekerIndexList.Extract(_j => _j == j, -1);

                //SuperPlayer player = players[k];

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
                    int j = i % _NonSeekerIndexList.Length;
                    int k = _NonSeekerIndexList[j];

                    //MyConsole.Debug($"j: {j}");

                    SuperPlayer player = players[k];

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
    }

}
