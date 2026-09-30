using System;
using System.IO;
using System.Windows.Forms;
using DigitalVibrance.Core;
using DigitalVibrance.Native;
using DigitalVibrance.UI;

namespace DigitalVibrance
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                WriteLog("=== Digital Vibrance Switcher START ===");

                bool autoStart = args.Length > 0 && args[0] == "--hide";
                if (autoStart)
                    WriteLog("Launched via auto-start (--hide)");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                WriteLog("Loading settings...");
                SettingsManager.Load();

                WriteLog("Initializing NVAPI...");
                if (!VibranceController.Initialize())
                {
                    string msg = string.Format(
                        "Cannot connect to NVIDIA driver.\n\n{0}\n\n" +
                        "Make sure you have:\n" +
                        "  \u2022 An NVIDIA GPU\n" +
                        "  \u2022 Latest drivers installed\n" +
                        "  \u2022 nvapi64.dll available",
                        NvApi.LastErrorMessage);

                    WriteLog("NVAPI ERROR: " + NvApi.LastErrorMessage);
                    MessageBox.Show(msg, "Digital Vibrance Switcher",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                WriteLog("NVAPI OK. Applying saved value...");
                int saved = SettingsManager.Current.LastVibrance;
                VibranceController.SetVibrance(saved);

                WriteLog("Creating MainForm...");
                var form = new MainForm();

                TrayManager.Initialize(form);
                if (!autoStart)
                {
                    WriteLog("Showing main form...");
                    form.Show();
                }
                else
                {
                    WriteLog("Auto-start mode — running in system tray");
                }

                WriteLog("Entering Application.Run()");
                Application.Run();

                WriteLog("=== App exiting normally ===");
                GameDetector.Stop();
                VibranceController.Shutdown();
            }
            catch (Exception ex)
            {
                string err = "FATAL: " + ex.ToString();
                WriteLog(err);
                MessageBox.Show(err, "Digital Vibrance Switcher - Fatal Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void WriteLog(string msg)
        {
            try
            {
                string logDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DigitalVibrance");
                Directory.CreateDirectory(logDir);
                File.AppendAllText(Path.Combine(logDir, "debug.log"),
                    DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg + Environment.NewLine);
            }
            catch { }
        }
    }
}