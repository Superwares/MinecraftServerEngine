

namespace MinecraftPrimitives
{
    public static class TextColorExtensions
    {
        public static string GetName(this TextColor color)
        {
            switch (color)
            {
                default:
                    throw new System.ArgumentException("Invalid color value");
                case TextColor.Black:
                    return "black";
                case TextColor.DarkBlue:
                    return "dark_blue";
                case TextColor.DarkGreen:
                    return "dark_green";
                case TextColor.DarkCyan:
                    return "dark_aqua";
                case TextColor.DarkRed:
                    return "dark_red";
                case TextColor.Purple:
                    return "dark_purple";
                case TextColor.Gold:
                    return "gold";
                case TextColor.Gray:
                    return "gray";
                case TextColor.DarkGray:
                    return "dark_gray";
                case TextColor.Blue:
                    return "blue";
                case TextColor.BrightGreen:
                    return "green";
                case TextColor.Cyan:
                    return "aqua";
                case TextColor.Red:
                    return "red";
                case TextColor.Pink:
                    return "light_purple";
                case TextColor.Yellow:
                    return "yellow";
                case TextColor.White:
                    return "white";

            }

        }
    }
}
