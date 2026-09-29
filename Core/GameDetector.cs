using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DigitalVibrance.Core
{
    public class AppSwitchEventArgs : EventArgs
    {
        public string ProcessName { get; private set; }
        public string WindowTitle { get; private set; }

        public AppSwitchEventArgs(string process, string title)
        {
            ProcessName = process;
            WindowTitle = title;
        }
    }

    static class GameDetector
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);

        public static event EventHandler<AppSwitchEventArgs> ForegroundAppChanged;

        static Timer _pollTimer;
        static string _lastProcess = "";
        static bool _wasRestored = true;

        public static void Start()
        {
            if (_pollTimer != null) return;

            _pollTimer = new Timer();
            _pollTimer.Interval = 1000;
            _pollTimer.Tick += OnPoll;
            _pollTimer.Start();
        }

        public static void Stop()
        {
            if (_pollTimer != null)
            {
                _pollTimer.Stop();
                _pollTimer.Dispose();
                _pollTimer = null;
            }
        }

        static void OnPoll(object sender, EventArgs e)
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return;

            string processName = GetProcessName(hwnd);
            string windowTitle = GetWindowTitle(hwnd);

            if (string.IsNullOrEmpty(processName))
                return;

            if (processName != _lastProcess)
            {
                _lastProcess = processName;

                bool matched = ProfileManager.TryActivateProfile(processName);

                if (matched)
                {
                    _wasRestored = false;
                }
                else if (!_wasRestored && SettingsManager.Current.RestoreAfterApp)
                {
                    int def = SettingsManager.Current.LastVibrance;
                    VibranceController.SetVibrance(def);
                    _wasRestored = true;
                }

                if (ForegroundAppChanged != null)
                    ForegroundAppChanged(null, new AppSwitchEventArgs(processName, windowTitle));
            }
        }

        static string GetProcessName(IntPtr hwnd)
        {
            try
            {
                uint pid;
                GetWindowThreadProcessId(hwnd, out pid);
                using (var p = Process.GetProcessById((int)pid))
                {
                    return p.ProcessName;
                }
            }
            catch
            {
                return "";
            }
        }

        static string GetWindowTitle(IntPtr hwnd)
        {
            var sb = new System.Text.StringBuilder(256);
            int len = GetWindowText(hwnd, sb, sb.Capacity);
            return len > 0 ? sb.ToString() : "";
        }

        public static string CurrentProcess
        {
            get { return _lastProcess; }
        }

        public static bool IsRunning
        {
            get { return _pollTimer != null; }
        }
    }
}