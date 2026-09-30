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
        public static event EventHandler ActiveProfileChanged;

        public static int ActiveProfileIndex { get; private set; }
        public static string ActiveProfileName { get; private set; }

        static ProfileManager()
        {
            ActiveProfileIndex = -1;
            ActiveProfileName = "";
        }

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
            if (ActiveProfileIndex == index)
            {
                ActiveProfileIndex = -1;
                ActiveProfileName = "";
                if (ActiveProfileChanged != null)
                    ActiveProfileChanged(null, EventArgs.Empty);
            }
            else if (index < ActiveProfileIndex)
            {
                ActiveProfileIndex--;
                if (ActiveProfileChanged != null)
                    ActiveProfileChanged(null, EventArgs.Empty);
            }
        }

        public static bool ActivateProfile(int index)
        {
            var profiles = SettingsManager.Current.Profiles;
            if (profiles == null || index < 0 || index >= profiles.Count)
                return false;

            var profile = profiles[index];
            if (!profile.IsEnabled) return false;

            VibranceController.SetVibrance(profile.VibranceValue);
            SetActive(index, profile.Name);

            if (ProfileActivated != null)
                ProfileActivated(null, new ProfileEventArgs(profile.Name, profile.VibranceValue));

            return true;
        }

        public static void UpdateProfile(int index, string name, string processName, int vibrance)
        {
            var profiles = SettingsManager.Current.Profiles;
            if (profiles == null || index < 0 || index >= profiles.Count)
                return;

            profiles[index].Name = name;
            profiles[index].ProcessName = processName;
            profiles[index].VibranceValue = vibrance;
            SettingsManager.Save();

            if (index == ActiveProfileIndex)
            {
                ActiveProfileName = name;
                if (ActiveProfileChanged != null)
                    ActiveProfileChanged(null, EventArgs.Empty);
            }
        }

        public static void SetProfileEnabled(int index, bool enabled)
        {
            var profiles = SettingsManager.Current.Profiles;
            if (profiles == null || index < 0 || index >= profiles.Count)
                return;

            profiles[index].IsEnabled = enabled;
            SettingsManager.Save();

            if (index == ActiveProfileIndex && !enabled)
            {
                ActiveProfileIndex = -1;
                ActiveProfileName = "";
                if (ActiveProfileChanged != null)
                    ActiveProfileChanged(null, EventArgs.Empty);
            }
            else
            {
                if (ActiveProfileChanged != null)
                    ActiveProfileChanged(null, EventArgs.Empty);
            }
        }

        public static bool TryActivateProfile(string processName, string exePath)
        {
            var profile = SettingsManager.FindMatchingProfile(processName, exePath);
            var profiles = SettingsManager.Current.Profiles;
            if (profile == null || profiles == null)
            {
                ClearActive();
                return false;
            }

            VibranceController.SetVibrance(profile.VibranceValue);

            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i] == profile)
                {
                    SetActive(i, profile.Name);
                    break;
                }
            }

            if (ProfileActivated != null)
                ProfileActivated(null, new ProfileEventArgs(profile.Name, profile.VibranceValue));

            return true;
        }

        static void SetActive(int index, string name)
        {
            ActiveProfileIndex = index;
            ActiveProfileName = name;
            if (ActiveProfileChanged != null)
                ActiveProfileChanged(null, EventArgs.Empty);
        }

        static void ClearActive()
        {
            if (ActiveProfileIndex >= 0)
            {
                ActiveProfileIndex = -1;
                ActiveProfileName = "";
                if (ActiveProfileChanged != null)
                    ActiveProfileChanged(null, EventArgs.Empty);
            }
        }

        public static string GetProfileDescription(int index)
        {
            var p = SettingsManager.Current.Profiles;
            if (p == null || index < 0 || index >= p.Count)
                return "";

            var profile = p[index];
            return string.Format("{0}  \u2192  {1}%  ({2})",
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