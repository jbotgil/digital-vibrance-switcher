using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DigitalVibrance.Core
{
    static class HotkeyManager
    {
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_NOREPEAT = 0x4000;
        public const int WM_HOTKEY = 0x0312;

        const int BASE_ID = 9000;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, Keys vk);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public static int GetHotkeyId(int index)
        {
            return BASE_ID + index;
        }

        public static void RegisterAll(IntPtr handle)
        {
            var s = SettingsManager.Current;
            if (!s.EnableHotkeys) return;

            RegisterHotKey(handle, GetHotkeyId(0), MOD_ALT | MOD_CONTROL | MOD_NOREPEAT, Keys.D1);
            RegisterHotKey(handle, GetHotkeyId(1), MOD_ALT | MOD_CONTROL | MOD_NOREPEAT, Keys.D2);
            RegisterHotKey(handle, GetHotkeyId(2), MOD_ALT | MOD_CONTROL | MOD_NOREPEAT, Keys.D3);
            RegisterHotKey(handle, GetHotkeyId(3), MOD_ALT | MOD_CONTROL | MOD_NOREPEAT, Keys.D4);
        }

        public static void UnregisterAll(IntPtr handle)
        {
            for (int i = 0; i < 4; i++)
                UnregisterHotKey(handle, GetHotkeyId(i));
        }

        public static bool TryHandle(ref Message m)
        {
            if (m.Msg != WM_HOTKEY) return false;

            int id = m.WParam.ToInt32();
            int index = id - BASE_ID;

            if (index < 0 || index > 3) return false;

            var s = SettingsManager.Current;
            int[] values = new int[] {
                s.HotkeyVibrance1,
                s.HotkeyVibrance2,
                s.HotkeyVibrance3,
                s.HotkeyVibrance4
            };

            VibranceController.SetVibrance(values[index]);
            Debug.WriteLine(string.Format("Hotkey Ctrl+Alt+{0}: Vibrance -> {1}%",
                index + 1, values[index]));
            return true;
        }
    }
}