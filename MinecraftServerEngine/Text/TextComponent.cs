using System.Text;

namespace MinecraftServerEngine.Text
{
    public readonly struct TextComponent
    {
        private static string EscapeJsonString(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static string GenerateJsonString(params TextComponent[] components)
        {
            if (components == null || components.Length == 0)
            {
                return "{\"text\":\"\"}";
            }

            //var extra = new object[components.Length];

            //for (int i = 0; i < components.Length; ++i)
            //{
            //    TextComponent component = components[i];

            //    extra[i] = new
            //    {
            //        text = component.Text,
            //        color = component.Color.GetName(),
            //    };
            //}

            //var chat = new
            //{
            //    text = "",
            //    extra = extra,
            //};

            //string data = System.Text.Json.JsonSerializer.Serialize(chat);

            StringBuilder extra = new();

            for (int i = 0; i < components.Length; ++i)
            {
                if (i > 0)
                {
                    extra.Append(',');
                }
                
                TextComponent component = components[i];

                extra.Append("{")
                     .Append($"\"text\":\"{EscapeJsonString(component.Text)}\",")
                     .Append($"\"color\":\"{component.Color.GetName()}\",")
                     .Append($"\"bold\":{component.Bold.ToString().ToLower()},")
                     .Append($"\"italic\":{component.Italic.ToString().ToLower()},")
                     .Append($"\"underlined\":{component.Underlined.ToString().ToLower()},")
                     .Append($"\"strikethrough\":{component.StrikeThrough.ToString().ToLower()},")
                     .Append($"\"obfuscated\":{component.Obfuscated.ToString().ToLower()}")
                     .Append("}");
            }

            return $"\"{{\"text\":\"\",\"extra\":[{extra}]}}\"";
        }

        

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
