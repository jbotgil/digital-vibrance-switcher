using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DigitalVibrance.Core
{
    static class InstalledAppScanner
    {
        public class AppEntry
        {
            public string Name { get; set; }
            public string ExePath { get; set; }
            public string ProcessName { get { return Path.GetFileNameWithoutExtension(ExePath); } }
        }

        static List<AppEntry> _cache;
        static readonly object _lock = new object();

        public static List<AppEntry> Scan()
        {
            lock (_lock)
            {
                if (_cache != null)
                    return _cache;

                var found = new Dictionary<string, AppEntry>(StringComparer.OrdinalIgnoreCase);
                AddRunningProcesses(found);
                AddSteamLibraries(found);
                AddRiotGames(found);
                AddCommonPaths(found);
                _cache = found.Values.OrderBy(a => a.Name).ToList();
                return _cache;
            }
        }

        // ── Steam: parse libraryfolders.vdf and scan each library ──
        static void AddSteamLibraries(Dictionary<string, AppEntry> found)
        {
            // Common Steam install locations
            string[] steamRoots =
            {
                @"C:\Program Files (x86)\Steam",
                @"C:\Program Files\Steam",
                @"D:\SteamLibrary",
                @"E:\SteamLibrary"
            };

            foreach (var root in steamRoots)
            {
                var vdf = Path.Combine(root, "steamapps\\libraryfolders.vdf");
                if (File.Exists(vdf))
                    AddSteamLibraryFromVdf(found, vdf);

                // also scan the root's own common dir
                var ownCommon = Path.Combine(root, "steamapps\\common");
                if (Directory.Exists(ownCommon))
                    AddCommonDir(found, ownCommon);
            }
        }

        static void AddSteamLibraryFromVdf(Dictionary<string, AppEntry> found, string vdf)
        {
            try
            {
                var text = File.ReadAllText(vdf);
                // Extract each "path" value
                var matches = Regex.Matches(text, "\"path\"\\s*\"([^\"]+)\"");
                foreach (Match m in matches)
                {
                    string lib = m.Groups[1].Value.Replace("\\\\", "\\");
                    string common = Path.Combine(lib, "steamapps\\common");
                    if (Directory.Exists(common))
                        AddCommonDir(found, common);
                }
            }
            catch { }
        }

        static void AddCommonDir(Dictionary<string, AppEntry> found, string commonDir)
        {
            try
            {
                foreach (var gameDir in Directory.GetDirectories(commonDir))
                {
                    AddExeRecursive(found, gameDir, 0, 2);
                }
            }
            catch { }
        }

        // ── Riot Games (Valorant, LoL, TFT...) ──
        static void AddRiotGames(Dictionary<string, AppEntry> found)
        {
            string[] roots = { @"C:\Riot Games", @"D:\Riot Games", @"E:\Riot Games" };
            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) continue;
                try
                {
                    foreach (var gameDir in Directory.GetDirectories(root))
                        AddExeRecursive(found, gameDir, 0, 3);
                }
                catch { }
            }
        }

        // ── Generic Program Files ──
        static void AddCommonPaths(Dictionary<string, AppEntry> found)
        {
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pfx = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            foreach (var root in new[] { pf, pfx })
            {
                if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;

                string[] rels = { "Epic Games", "GOG Games", "Battle.net\\Games" };
                foreach (var rel in rels)
                {
                    string dir = Path.Combine(root, rel);
                    if (Directory.Exists(dir))
                        foreach (var sub in Directory.GetDirectories(dir))
                            AddExeRecursive(found, sub, 0, 2);
                }

                // top-level exe files only
                try
                {
                    foreach (var exe in Directory.GetFiles(root, "*.exe", SearchOption.TopDirectoryOnly))
                        AddUnique(found, Path.GetFileNameWithoutExtension(exe), exe);
                }
                catch { }
            }
        }

        // ── Recursive exe search, depth-limited, skipping system/big dirs ──
        static void AddExeRecursive(Dictionary<string, AppEntry> found, string dir, int depth, int maxDepth)
        {
            if (depth > maxDepth) return;
            try
            {
                foreach (var exe in Directory.GetFiles(dir, "*.exe", SearchOption.TopDirectoryOnly))
                    AddUnique(found, Path.GetFileNameWithoutExtension(exe), exe);

                foreach (var sub in Directory.GetDirectories(dir))
                {
                    string name = Path.GetFileName(sub);
                    // skip huge / irrelevant folders
                    if (name.Equals("Binaries", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Redist", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Engine", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Content", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Uninstall", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Update", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("logs", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith(".", StringComparison.Ordinal))
                        continue;
                    AddExeRecursive(found, sub, depth + 1, maxDepth);
                }
            }
            catch { }
        }

        // ── Running processes ──
        static void AddRunningProcesses(Dictionary<string, AppEntry> found)
        {
            Process[] procs = null;
            try { procs = Process.GetProcesses(); }
            catch { return; }
            if (procs == null) return;

            foreach (var p in procs)
            {
                try
                {
                    string name = p.ProcessName;
                    if (string.IsNullOrEmpty(name) || found.ContainsKey(name)) continue;
                    AddUnique(found, name, p.MainModule.FileName);
                }
                catch { }
            }
        }

        static void AddUnique(Dictionary<string, AppEntry> found, string name, string exePath)
        {
            if (string.IsNullOrEmpty(exePath)) return;
            string key = Path.GetFileNameWithoutExtension(exePath);
            if (string.IsNullOrEmpty(key) || found.ContainsKey(key)) return;
            found[key] = new AppEntry { Name = name, ExePath = exePath };
        }

        public static void InvalidateCache()
        {
            lock (_lock) { _cache = null; }
        }
    }
}