using System.Drawing;

namespace DigitalVibrance.UI
{
    static class Theme
    {
        // Backgrounds
        public static readonly Color Bg = Color.FromArgb(11, 12, 16);
        public static readonly Color Surface = Color.FromArgb(17, 19, 26);
        public static readonly Color Surface2 = Color.FromArgb(23, 25, 35);
        public static readonly Color Surface3 = Color.FromArgb(29, 32, 42);

        // Border
        public static readonly Color Border = Color.FromArgb(37, 40, 52);

        // Accent
        public static readonly Color Primary = Color.FromArgb(0, 168, 255);
        public static readonly Color PrimaryBright = Color.FromArgb(25, 185, 255);
        public static readonly Color PrimaryDark = Color.FromArgb(0, 119, 182);

        // Text
        public static readonly Color Text = Color.FromArgb(243, 245, 247);
        public static readonly Color TextSec = Color.FromArgb(154, 159, 172);
        public static readonly Color TextMuted = Color.FromArgb(94, 99, 112);

        // Semantic
        public static readonly Color Success = Color.FromArgb(85, 217, 139);
        public static readonly Color Warning = Color.FromArgb(255, 184, 77);
        public static readonly Color Error = Color.FromArgb(255, 92, 108);

        public static readonly Color Hover = Color.FromArgb(33, 36, 48);
        public static readonly Color ActiveBg = Color.FromArgb(0, 168, 255);

        // Border radius
        public const int RadiusCard = 8;
        public const int RadiusBtn = 6;
        public const int RadiusInput = 6;

        public static Font FontReg(float size) { return new Font("Segoe UI", size, FontStyle.Regular); }
        public static Font FontBold(float size) { return new Font("Segoe UI", size, FontStyle.Bold); }
        public static Font FontSemi(float size) { return new Font("Segoe UI", size, FontStyle.Bold); }

        public static Color ValueColor(int v)
        {
            if (v < 30) return Error;
            if (v < 60) return Primary;
            return Success;
        }
    }
}