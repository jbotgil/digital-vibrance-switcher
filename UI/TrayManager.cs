using System;
using System.Drawing;
using System.Windows.Forms;
using DigitalVibrance.Core;

namespace DigitalVibrance.UI
{
    static class TrayManager
    {
        static NotifyIcon _trayIcon;
        static MainForm _mainForm;

        public static void Initialize(MainForm form)
        {
            _mainForm = form;

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = CreateAppIcon();
            _trayIcon.Text = string.Format("Digital Vibrance — {0}%", VibranceController.CurrentValue);
            _trayIcon.Visible = true;

            BuildContextMenu();
            _trayIcon.DoubleClick += (s, e) => ShowForm();
            VibranceController.ValueChanged += (s, e) => UpdateTrayText(e.NewValue);
        }

        public static void ShowForm()
        {
            if (_mainForm != null)
            {
                _mainForm.Show();
                _mainForm.WindowState = FormWindowState.Normal;
                _mainForm.Activate();
            }
        }

        static void BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.BackColor = Theme.Surface;
            menu.ForeColor = Theme.Text;

            int[] presets = { 40, 50, 60, 70, 80, 100 };
            foreach (int p in presets)
            {
                int v = p;
                var item = menu.Items.Add(p == 100 ? "MAX" : v + "%");
                item.Click += (s, e) => VibranceController.SetVibrance(v);
            }

            menu.Items.Add(new ToolStripSeparator());

            var showItem = menu.Items.Add("Show Window");
            showItem.Click += (s, e) => ShowForm();

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = menu.Items.Add("Exit");
            exitItem.ForeColor = Color.FromArgb(200, 60, 60);
            exitItem.Click += (s, e) =>
            {
                GameDetector.Stop();
                VibranceController.Shutdown();
                _trayIcon.Visible = false;
                Application.Exit();
            };

            _trayIcon.ContextMenuStrip = menu;
        }

        static void UpdateTrayText(int value)
        {
            _trayIcon.Text = string.Format("Digital Vibrance — {0}%", value);
            BuildContextMenu();
        }

        public static void ShowBalloon(string title, string text, ToolTipIcon icon)
        {
            if (_trayIcon != null)
                _trayIcon.ShowBalloonTip(2000, title, text, icon);
        }

        public static Icon CreateAppIcon()
        {
            int size = 16;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var bg = new SolidBrush(Theme.Primary))
                using (var border = new Pen(Theme.PrimaryDark, 1.5f))
                {
                    g.FillEllipse(bg, 1, 1, size - 2, size - 2);
                    g.DrawEllipse(border, 1, 1, size - 2, size - 2);
                }

                using (var txtBrush = new SolidBrush(Color.White))
                using (var font = new Font("Segoe UI", 7, FontStyle.Bold))
                {
                    g.DrawString("DV", font, txtBrush, 1, 2);
                }
            }
            return Icon.FromHandle(bmp.GetHicon());
        }

        public static void Dispose()
        {
            if (_trayIcon != null)
                _trayIcon.Dispose();
        }
    }
}