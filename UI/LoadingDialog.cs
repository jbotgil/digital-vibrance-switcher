using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DigitalVibrance.Core;

namespace DigitalVibrance.UI
{
    class LoadingDialog : Form
    {
        int _angle;
        Timer _spin;

        public LoadingDialog(string message)
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(240, 110);
            BackColor = Theme.Surface;
            Padding = new Padding(0);
            DoubleBuffered = true;

            var panel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
            panel.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Theme.Border, 1))
                {
                    var r = panel.ClientRectangle; r.Width--; r.Height--;
                    g.DrawRectangle(pen, r);
                }
                // Spinner
                int cx = panel.Width / 2, cy = 32, rad = 18;
                using (var pen = new Pen(Theme.Surface3, 4))
                    g.DrawEllipse(pen, cx - rad, cy - rad, rad * 2, rad * 2);
                using (var pen = new Pen(Theme.Primary, 4))
                {
                    pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, cx - rad, cy - rad, rad * 2, rad * 2, _angle, 110);
                }
                using (var br = new SolidBrush(Theme.TextSec))
                using (var f = Theme.FontReg(9))
                {
                    var m = g.MeasureString(message, f);
                    g.DrawString(message, f, br, (panel.Width - m.Width) / 2, 62);
                }
            };

            _spin = new Timer { Interval = 15 };
            _spin.Tick += (s, e) =>
            {
                _angle = (_angle + 10) % 360;
                panel.Invalidate();
            };
            _spin.Start();

            Controls.Add(panel);
            Shown += (s, e) =>
            {
                // Force dead-center on the screen
                var wa = Screen.FromControl(this).WorkingArea;
                Location = new Point(wa.Left + (wa.Width - Width) / 2,
                                     wa.Top + (wa.Height - Height) / 2);
                panel.Invalidate();
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_spin != null) { _spin.Stop(); _spin.Dispose(); _spin = null; }
            base.OnFormClosing(e);
        }
    }
}