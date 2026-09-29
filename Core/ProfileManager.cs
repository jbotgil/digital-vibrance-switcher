using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DigitalVibrance.Core
{
    public class ProfileEventArgs : EventArgs
    {
        public string ProcessName { get; private set; }
        public int VibranceValue { get; private set; }

        public ProfileEventArgs(string process, int vibrance)
        {
            ProcessName = process;
            VibranceValue = vibrance;
        }
    }

    static class ProfileManager
    {
        public static event EventHandler<ProfileEventArgs> ProfileActivated;

        public static void AddProfile(string name, string processName, int vibrance)
        {
            var profile = new AppProfile
            {
                Name = name,
                ProcessName = processName,
                VibranceValue = vibrance,
                IsEnabled = true
            };
            SettingsManager.AddProfile(profile);
        }

        public static void RemoveProfile(int index)
        {
            SettingsManager.RemoveProfile(index);
        }

        public static bool TryActivateProfile(string processName)
        {
            var profile = SettingsManager.FindMatchingProfile(processName);
            if (profile == null)
                return false;

            int previous = VibranceController.CurrentValue;
            VibranceController.SetVibrance(profile.VibranceValue);

            if (ProfileActivated != null)
                ProfileActivated(null, new ProfileEventArgs(profile.Name, profile.VibranceValue));

            return true;
        }

        public static string GetProfileDescription(int index)
        {
            var p = SettingsManager.Current.Profiles;
            if (p == null || index < 0 || index >= p.Count)
                return "";

            var profile = p[index];
            return string.Format("{0}  →  {1}%  ({2})",
                profile.Name, profile.VibranceValue, profile.ProcessName);
        }

        public static int FindIndexByProcess(string processName)
        {
            var profiles = SettingsManager.Current.Profiles;
            if (profiles == null || string.IsNullOrEmpty(processName))
                return -1;

            for (int i = 0; i < profiles.Count; i++)
            {
                if (string.Equals(profiles[i].ProcessName, processName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
    }
}