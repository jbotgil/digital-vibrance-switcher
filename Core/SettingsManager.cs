using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;

namespace DigitalVibrance.Core
{
    public class AppProfile
    {
        public string Name { get; set; }
        public string ProcessName { get; set; }
        public int VibranceValue { get; set; }
        public bool IsEnabled { get; set; }

        public AppProfile()
        {
            Name = "";
            ProcessName = "";
            VibranceValue = 50;
            IsEnabled = true;
        }
    }

    public class AppSettings
    {
        public int LastVibrance { get; set; }
        public bool AutoStart { get; set; }
        public bool MinimizeToTrayOnStart { get; set; }
        public bool ShowBalloonTips { get; set; }
        public bool EnableHotkeys { get; set; }
        public int HotkeyVibrance1 { get; set; }
        public int HotkeyVibrance2 { get; set; }
        public int HotkeyVibrance3 { get; set; }
        public int HotkeyVibrance4 { get; set; }
        public int TransitionSpeed { get; set; }
        public bool AutoDetectApps { get; set; }
        public bool RestoreAfterApp { get; set; }
        public List<AppProfile> Profiles { get; set; }

        public AppSettings()
        {
            LastVibrance = 50;
            AutoStart = false;
            MinimizeToTrayOnStart = false;
            ShowBalloonTips = true;
            EnableHotkeys = true;
            HotkeyVibrance1 = 50;
            HotkeyVibrance2 = 60;
            HotkeyVibrance3 = 70;
            HotkeyVibrance4 = 80;
            TransitionSpeed = 0;
            AutoDetectApps = false;
            RestoreAfterApp = true;
            Profiles = new List<AppProfile>();
        }
    }

    static class SettingsManager
    {
        static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DigitalVibrance", "settings.json");

        static DataContractJsonSerializer _serializer;
        static AppSettings _current;

        public static AppSettings Current
        {
            get { return _current; }
        }

        public static void Load()
        {
            _current = new AppSettings();
            _serializer = new DataContractJsonSerializer(typeof(AppSettings));

            try
            {
                if (!File.Exists(FilePath)) return;

                using (var fs = File.OpenRead(FilePath))
                {
                    _current = (AppSettings)_serializer.ReadObject(fs);
                }
                if (_current.Profiles == null)
                    _current.Profiles = new List<AppProfile>();
            }
            catch
            {
                _current = new AppSettings();
            }
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var ms = new MemoryStream())
                {
                    _serializer.WriteObject(ms, _current);
                    File.WriteAllText(FilePath, Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
            catch { }
        }

        public static void SetAutoStart(bool enable)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null) return;

                    if (enable)
                        key.SetValue("DigitalVibrance", "\"" + Application.ExecutablePath + "\"");
                    else if (key.GetValue("DigitalVibrance") != null)
                        key.DeleteValue("DigitalVibrance");
                }

                _current.AutoStart = enable;
                Save();
            }
            catch { }
        }

        public static bool IsAutoStartEnabled()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run", false))
                {
                    if (key == null) return false;
                    var val = key.GetValue("DigitalVibrance") as string;
                    return val != null && val.Contains("DigitalVibrance.exe");
                }
            }
            catch { return false; }
        }

        public static void AddProfile(AppProfile profile)
        {
            if (_current.Profiles == null)
                _current.Profiles = new List<AppProfile>();
            _current.Profiles.Add(profile);
            Save();
        }

        public static void RemoveProfile(int index)
        {
            if (_current.Profiles != null && index >= 0 && index < _current.Profiles.Count)
            {
                _current.Profiles.RemoveAt(index);
                Save();
            }
        }

        public static AppProfile FindMatchingProfile(string processName)
        {
            if (_current.Profiles == null || string.IsNullOrEmpty(processName))
                return null;

            foreach (AppProfile p in _current.Profiles)
            {
                if (!p.IsEnabled) continue;
                if (string.IsNullOrEmpty(p.ProcessName)) continue;

                if (string.Equals(p.ProcessName, processName, StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            return null;
        }
    }
}