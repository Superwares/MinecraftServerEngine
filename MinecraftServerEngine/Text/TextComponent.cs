namespace MinecraftServerEngine.Text
{
    public readonly struct TextComponent
    {
        public readonly string Text;
        public readonly TextColor Color;
        public readonly bool Bold;
        public readonly bool Italic;
        public readonly bool Underlined;
        public readonly bool StrikeThrough;
        public readonly bool Obfuscated;

        public TextComponent(string text, TextColor color)
        {
            if (text == null)
            {
                throw new System.ArgumentNullException(nameof(text));
            }

            Text = text;
            Color = color;
            Bold = false;
            Italic = false;
            Underlined = false;
            StrikeThrough = false;
            Obfuscated = false;
        }

        public TextComponent(
            string text, TextColor color,
            bool bold, bool italic, bool underlined, bool strikeThrough, bool obfuscated)
        {
            if (text == null)
            {
                throw new System.ArgumentNullException(nameof(text));
            }

            Text = text;
            Color = color;
            Bold = bold;
            Italic = italic;
            Underlined = underlined;
            StrikeThrough = strikeThrough;
            Obfuscated = obfuscated;
        }
    }
}
