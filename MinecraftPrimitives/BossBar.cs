
namespace MinecraftPrimitives
{
    public struct BossBar
    {
        public readonly System.Guid Id = System.Guid.NewGuid();
        public readonly TextComponent[] Title;
        public readonly string TitleData;

        private double _health;
        public readonly double Health => _health;

        public readonly BossBarColor Color;
        public readonly BossBarDivision Division;


        public BossBar(
            TextComponent[] title, double health,
            BossBarColor color, BossBarDivision division)
        {
            System.Diagnostics.Debug.Assert(Id != System.Guid.Empty);

            if (health < 0.0 || health > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(health));
            }

            if (title == null)
            {
                title = [];
            }

            Title = new TextComponent[title.Length];
            System.Array.Copy(title, Title, title.Length);

            {
                System.Diagnostics.Debug.Assert(title != null);

                var extra = new object[title.Length];

                for (int i = 0; i < title.Length; ++i)
                {
                    TextComponent component = title[i];

                    extra[i] = new
                    {
                        text = component.Text,
                        color = component.Color.GetName(),
                    };
                }

                var _data = new
                {
                    text = "",
                    extra = extra,
                };

                TitleData = System.Text.Json.JsonSerializer.Serialize(_data);
            }

            _health = health;
            Color = color;
            Division = division;
        }

        public void UpdateHealth(double health)
        {
            if (health < 0.0 || health > 1.0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(health));
            }

            _health = health;
        }

    }
}
