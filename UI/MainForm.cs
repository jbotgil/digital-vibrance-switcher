using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DigitalVibrance.Core;

namespace DigitalVibrance.UI
{
    class MainForm : Form
    {
        ModernTrackBar _slider;
        NumericUpDown _numBox;
        Button[] _presetBtns;
        Panel _circleBox;
        Label _pctLabel;

        ModernCheckBox _startupCb, _hotkeyCb, _autoCb;
        NumericUpDown _speedBox;

        ListBox _profileList;
        TextBox _profName, _profProc;
        NumericUpDown _profVal;
        Label _profEmpty;

        Label _footerLabel, _gpuLabel;

        bool _ignoreEvents;
        int _circleSz = 72;

        public MainForm()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(480, 500);
            Size = new Size(640, 680);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Text = "Digital Vibrance Switcher";
            Icon = TrayManager.CreateAppIcon();
            FormClosing += (s, e) => { if (e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; Hide(); } };

            Controls.Add(Build());
            LoadData();
            RefreshProfiles();

            VibranceController.ValueChanged += (s, e) => SafeCall(() =>
            {
                if (!IsDisposed) SyncAll(e.NewValue);
            });
            GameDetector.ForegroundAppChanged += (s, e) => SafeCall(() =>
            {
                if (!IsDisposed)
                {
                    _footerLabel.Text = e.ProcessName;
                    _footerLabel.ForeColor = ProfileManager.FindIndexByProcess(e.ProcessName) >= 0 ? Theme.Primary : Theme.TextMuted;
                }
            });
        }

        // ─── BUILD: pure % layout ───────────────────────────────
        TableLayoutPanel Build()
        {
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 8, 20, 6),
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Color.Transparent
            };
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 7f));    // header
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1.5f));   // gap
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 22f));   // circle + %
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1.5f));  // gap
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 9f));    // slider
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1.5f));  // gap
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 11f));   // presets
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));   // settings + profiles

            tlp.Controls.Add(MkHeader(), 0, 0);
            tlp.Controls.Add(MkCircle(), 0, 2);
            tlp.Controls.Add(MkSlider(), 0, 4);
            tlp.Controls.Add(MkPresets(), 0, 6);
            tlp.Controls.Add(MkBottom(), 0, 7);

            return tlp;
        }

        // ─── HEADER ─────────────────────────────────────────
        Panel MkHeader()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, MinimumSize = new Size(0, 38) };
            var ic = new PictureBox
            {
                Image = TrayManager.CreateAppIcon().ToBitmap(),
                Size = new Size(16, 16), Location = new Point(0, 6),
                SizeMode = PictureBoxSizeMode.StretchImage, BackColor = Color.Transparent
            };
            p.Controls.Add(ic);
            p.Controls.Add(new Label
            {
                Text = "Digital Vibrance", Location = new Point(20, 4), AutoSize = true,
                Font = Theme.FontBold(11), ForeColor = Theme.Text, BackColor = Color.Transparent
            });
            p.Controls.Add(new Label
            {
                Text = "Display color", Location = new Point(20, 20), AutoSize = true,
                Font = Theme.FontReg(7), ForeColor = Theme.TextSec, BackColor = Color.Transparent
            });
            return p;
        }

        // ─── CIRCLE + % ────────────────────────────────────
        Panel MkCircle()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };

            _circleBox = new Panel { Size = new Size(_circleSz, _circleSz), BackColor = Theme.Bg };
            _circleBox.Paint += (s, e) =>
            {
                var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
                int v = VibranceController.CurrentValue;
                Color c = Theme.ValueColor(v);
                int d = _circleBox.Width;
                using (var pen = new Pen(Theme.Surface3, 4)) g.DrawEllipse(pen, 2, 2, d - 4, d - 4);
                using (var pen = new Pen(c, 4)) g.DrawArc(pen, 2, 2, d - 4, d - 4, 135, 270f * v / 100f);
                using (var br = new SolidBrush(Theme.Bg)) g.FillEllipse(br, 8, 8, d - 16, d - 16);
            };
            p.Controls.Add(_circleBox);

            _pctLabel = new Label
            {
                Text = "50%",
                Font = Theme.FontBold(26),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            p.Controls.Add(_pctLabel);

            p.Resize += (s, e) => ResizeValue(p);
            return p;
        }

        void ResizeValue(Panel p)
        {
            int cx = p.Width / 2;
            int topY = Math.Max(2, (p.Height - _circleSz - 40) / 2);
            _circleBox.Location = new Point(cx - _circleSz / 2, topY);
            if (_circleBox.Bottom + 36 > p.Height)
                _circleBox.Location = new Point(cx - _circleSz / 2, Math.Max(2, p.Height - _circleSz - 36));
            _pctLabel.SetBounds(0, _circleBox.Bottom, p.Width, 36);
        }

        // ─── SLIDER ─────────────────────────────────────────
        Panel MkSlider()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var tbl = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, BackColor = Color.Transparent };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));

            _slider = new ModernTrackBar { Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
            _slider.ValueChanged += (s, e) => { _pctLabel.Text = _slider.Value + "%"; };
            _slider.MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    _numBox.Value = _slider.Value;
                    ClearHl();
                    Go(_slider.Value);
                }
            };
            _slider.KeyUp += (s, e) => { _numBox.Value = _slider.Value; ClearHl(); Go(_slider.Value); };
            tbl.Controls.Add(_slider, 0, 0);

            _numBox = new NumericUpDown
            {
                Dock = DockStyle.Fill, Minimum = 0, Maximum = 100,
                Font = Theme.FontBold(11), ForeColor = Theme.Text,
                BackColor = Theme.Surface2, BorderStyle = BorderStyle.None,
                TextAlign = HorizontalAlignment.Center
            };
            foreach (Control c in _numBox.Controls) { c.BackColor = Theme.Surface2; c.ForeColor = Theme.Text; }
            _numBox.ValueChanged += (s, e) => { if (!_ignoreEvents) _slider.Value = (int)_numBox.Value; };
            _numBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) Go((int)_numBox.Value); e.SuppressKeyPress = true; };
            tbl.Controls.Add(_numBox, 1, 0);

            p.Controls.Add(tbl);
            return p;
        }

        // ─── PRESETS ────────────────────────────────────────
        Panel MkPresets()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true, BackColor = Color.Transparent,
                Padding = new Padding(0, 4, 0, 0)
            };

            int[] vals = { 40, 50, 60, 70, 80, 100 };
            _presetBtns = new Button[vals.Length];
            for (int i = 0; i < vals.Length; i++)
            {
                int v = vals[i];
                var b = new Button
                {
                    Text = v == 100 ? "MAX" : v + "%",
                    Size = new Size(54, 26), FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 1, BorderColor = Theme.Border },
                    Font = Theme.FontBold(v == 100 ? 7 : 8.5f),
                    Cursor = Cursors.Hand, Margin = new Padding(0, 0, 4, 0),
                    BackColor = Theme.Surface2, ForeColor = Theme.TextSec
                };
                b.FlatAppearance.MouseOverBackColor = Theme.Surface3;
                b.FlatAppearance.MouseDownBackColor = Theme.PrimaryDark;
                b.Click += (s, e) => PickPreset(v, b);
                _presetBtns[i] = b;
                flp.Controls.Add(b);
            }

            var rst = new Button
            {
                Text = "\u21BA", Size = new Size(28, 26),
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1, BorderColor = Theme.Border },
                Font = Theme.FontBold(10), Cursor = Cursors.Hand,
                BackColor = Theme.Surface2, ForeColor = Theme.TextSec
            };
            rst.FlatAppearance.MouseOverBackColor = Theme.Surface3;
            rst.Click += (s, e) => {
                var d = Native.NvApi.Displays;
                Go(d != null && d.Count > 0 ? d[0].Default : 50);
            };
            flp.Controls.Add(rst);
            p.Controls.Add(flp);
            return p;
        }

        // ─── BOTTOM: settings + profiles + footer ──────────
        TableLayoutPanel MkBottom()
        {
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 4));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));

            tlp.Controls.Add(MkSettings(), 0, 0);
            tlp.Controls.Add(MkProfiles(), 1, 0);
            tlp.Controls.Add(MkFooter(), 0, 2);
            tlp.SetColumnSpan(tlp.GetControlFromPosition(0, 2), 2);

            return tlp;
        }

        Panel MkSettings()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Padding = new Padding(10, 6, 8, 6) };
            p.Paint += (s, e) => { using (var pen = new Pen(Theme.Border, 1)) e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };

            var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.Transparent };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            inner.Controls.Add(new Label { Text = "SETTINGS", Font = Theme.FontBold(7.5f), ForeColor = Theme.TextMuted, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);

            var spdRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, Height = 24 };
            _speedBox = new NumericUpDown { Width = 52, Minimum = 0, Maximum = 10, Font = Theme.FontBold(12), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.None, Margin = new Padding(0, 0, 4, 0) };
            foreach (Control c in _speedBox.Controls) { c.BackColor = Theme.Surface2; c.ForeColor = Theme.Text; }
            _speedBox.ValueChanged += (s, e) => { SettingsManager.Current.TransitionSpeed = (int)_speedBox.Value; SettingsManager.Save(); };
            spdRow.Controls.Add(_speedBox);
            spdRow.Controls.Add(new Label { Text = "0=instant", Font = Theme.FontReg(6.5f), ForeColor = Theme.TextMuted, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 5, 0, 0) });
            inner.Controls.Add(spdRow, 0, 1);

            var chk = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent };
            _startupCb = new ModernCheckBox(); _startupCb.SetText("Run at startup");
            _startupCb.CheckedChanged += (s, e) => SettingsManager.SetAutoStart(_startupCb.Checked);
            chk.Controls.Add(_startupCb);

            _hotkeyCb = new ModernCheckBox(); _hotkeyCb.SetText("Hotkeys C+A+1..4");
            _hotkeyCb.CheckedChanged += (s, e) =>
            {
                SettingsManager.Current.EnableHotkeys = _hotkeyCb.Checked; SettingsManager.Save();
                if (_hotkeyCb.Checked) HotkeyManager.RegisterAll(Handle); else HotkeyManager.UnregisterAll(Handle);
            };
            chk.Controls.Add(_hotkeyCb);

            _autoCb = new ModernCheckBox(); _autoCb.SetText("Auto-switch apps");
            _autoCb.CheckedChanged += (s, e) =>
            {
                SettingsManager.Current.AutoDetectApps = _autoCb.Checked; SettingsManager.Save();
                if (_autoCb.Checked) GameDetector.Start(); else GameDetector.Stop();
            };
            chk.Controls.Add(_autoCb);

            inner.Controls.Add(chk, 0, 2);
            p.Controls.Add(inner);
            return p;
        }

        Panel MkProfiles()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Padding = new Padding(8, 6, 10, 6) };
            p.Paint += (s, e) => { using (var pen = new Pen(Theme.Border, 1)) e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1); };

            var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, BackColor = Color.Transparent };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 20));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));

            inner.Controls.Add(new Label { Text = "PROFILES", Font = Theme.FontBold(7.5f), ForeColor = Theme.TextMuted, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);

            var listPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface2 };
            _profileList = new ListBox
            {
                Dock = DockStyle.Fill, BackColor = Theme.Surface2, ForeColor = Theme.Text,
                Font = Theme.FontReg(8), BorderStyle = BorderStyle.None, IntegralHeight = false,
                Margin = new Padding(2)
            };
            _profileList.SelectedIndexChanged += (s, e) =>
            {
                int idx = _profileList.SelectedIndex;
                var p2 = SettingsManager.Current.Profiles;
                if (p2 != null && idx >= 0 && idx < p2.Count)
                {
                    _profName.Text = p2[idx].Name;
                    _profProc.Text = p2[idx].ProcessName;
                    _profVal.Value = p2[idx].VibranceValue;
                }
            };
            listPanel.Controls.Add(_profileList);

            _profEmpty = new Label
            {
                Text = "No profiles", Font = Theme.FontReg(7.5f), ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter, Visible = false
            };
            listPanel.Controls.Add(_profEmpty);
            inner.Controls.Add(listPanel, 0, 1);

            var editor = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var eflp = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent };

            eflp.Controls.Add(new Label { Text = "Name", Font = Theme.FontReg(6.5f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, Height = 10 });
            _profName = new TextBox { Font = Theme.FontReg(8), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.FixedSingle, Height = 18 };
            eflp.Controls.Add(_profName);

            eflp.Controls.Add(new Label { Text = "Process", Font = Theme.FontReg(6.5f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, Height = 10, Margin = new Padding(0, 1, 0, 0) });
            _profProc = new TextBox { Font = Theme.FontReg(8), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.FixedSingle, Height = 18 };
            eflp.Controls.Add(_profProc);

            eflp.Controls.Add(new Label { Text = "DV%", Font = Theme.FontReg(6.5f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, Height = 10, Margin = new Padding(0, 1, 0, 0) });
            _profVal = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 70, Width = 46, Font = Theme.FontBold(8.5f), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.None };
            foreach (Control c in _profVal.Controls) { c.BackColor = Theme.Surface2; c.ForeColor = Theme.Text; }
            eflp.Controls.Add(_profVal);

            var br = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, Margin = new Padding(0, 2, 0, 0) };

            var addB = new Button { Text = "Add", Font = Theme.FontBold(7.5f), ForeColor = Color.White, BackColor = Theme.PrimaryDark, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Size = new Size(36, 20), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 2, 0) };
            addB.FlatAppearance.MouseOverBackColor = Theme.Primary;
            addB.Click += (s, e) =>
            {
                string n = _profName.Text.Trim(), p3 = _profProc.Text.Trim().Replace(".exe", "");
                if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(p3)) return;
                ProfileManager.AddProfile(n, p3, (int)_profVal.Value);
                RefreshProfiles();
                _profName.Text = ""; _profProc.Text = ""; _profVal.Value = 70;
            };
            br.Controls.Add(addB);

            var delB = new Button { Text = "Del", Font = Theme.FontBold(7.5f), ForeColor = Color.White, BackColor = Theme.Error, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Size = new Size(30, 20), Cursor = Cursors.Hand, Margin = new Padding(0, 0, 2, 0) };
            delB.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 120);
            delB.Click += (s, e) => { int idx = _profileList.SelectedIndex; if (idx >= 0) { ProfileManager.RemoveProfile(idx); RefreshProfiles(); } };
            br.Controls.Add(delB);

            var detB = new Button { Text = "\u25B6", Font = Theme.FontBold(8.5f), ForeColor = Theme.TextSec, BackColor = Theme.Surface2, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1, BorderColor = Theme.Border }, Size = new Size(20, 20), Cursor = Cursors.Hand };
            detB.FlatAppearance.MouseOverBackColor = Theme.Surface3;
            new ToolTip().SetToolTip(detB, "Detect app");
            detB.Click += (s, e) =>
            {
                _profProc.Text = GameDetector.CurrentProcess;
                if (!string.IsNullOrEmpty(_profProc.Text) && string.IsNullOrEmpty(_profName.Text))
                    _profName.Text = _profProc.Text;
            };
            br.Controls.Add(detB);

            eflp.Controls.Add(br);
            editor.Controls.Add(eflp);
            inner.Controls.Add(editor, 0, 2);

            p.Controls.Add(inner);
            return p;
        }

        Panel MkFooter()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            _footerLabel = new Label
            {
                Text = "—", Dock = DockStyle.Left, AutoSize = true,
                Font = Theme.FontBold(7), ForeColor = Theme.TextMuted, BackColor = Color.Transparent
            };
            p.Controls.Add(_footerLabel);

            _gpuLabel = new Label
            {
                Dock = DockStyle.Right, AutoSize = true,
                Font = Theme.FontReg(6.5f), ForeColor = Theme.TextMuted, BackColor = Color.Transparent
            };
            _gpuLabel.Text = VibranceController.GetDisplayInfo().Replace("\n", "  ");
            p.Controls.Add(_gpuLabel);
            return p;
        }

        // ─── HELPERS ─────────────────────────────────────────
        void LoadData()
        {
            var s = SettingsManager.Current;
            _ignoreEvents = true;
            _slider.Value = s.LastVibrance;
            _numBox.Value = s.LastVibrance;
            _pctLabel.Text = s.LastVibrance + "%";
            _ignoreEvents = false;
            _startupCb.Checked = SettingsManager.IsAutoStartEnabled();
            _hotkeyCb.Checked = s.EnableHotkeys;
            _autoCb.Checked = s.AutoDetectApps;
            _speedBox.Value = s.TransitionSpeed;
        }

        void ClearHl()
        {
            foreach (var b in _presetBtns) { b.BackColor = Theme.Surface2; b.ForeColor = Theme.TextSec; b.FlatAppearance.BorderColor = Theme.Border; }
        }

        void PickPreset(int v, Button a)
        {
            Go(v);
            foreach (var b in _presetBtns)
            {
                if (b == a) { b.BackColor = Theme.Primary; b.ForeColor = Color.White; b.FlatAppearance.BorderColor = Theme.Primary; }
                else { b.BackColor = Theme.Surface2; b.ForeColor = Theme.TextSec; b.FlatAppearance.BorderColor = Theme.Border; }
            }
        }

        void Go(int v)
        {
            try
            {
                VibranceController.SetVibrance(v);
                SyncAll(v);
            }
            catch { }
        }

        void SyncAll(int v)
        {
            _numBox.Value = v;
            _pctLabel.Text = v + "%";
            _circleBox.Invalidate();
            _slider.Value = v;
        }

        void RefreshProfiles()
        {
            _profileList.Items.Clear();
            var p = SettingsManager.Current.Profiles;
            if (p == null || p.Count == 0) { _profileList.Visible = false; _profEmpty.Visible = true; return; }
            _profileList.Visible = true; _profEmpty.Visible = false;
            foreach (var x in p)
                _profileList.Items.Add(x.Name + "  " + x.VibranceValue + "%  (" + x.ProcessName + ")");
        }

        void SafeCall(Action a) { if (InvokeRequired) Invoke(a); else a(); }

        protected override void WndProc(ref Message m)
        {
            if (HotkeyManager.TryHandle(ref m)) return;
            base.WndProc(ref m);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var s = SettingsManager.Current;
            if (s.EnableHotkeys) HotkeyManager.RegisterAll(Handle);
            if (s.AutoDetectApps) GameDetector.Start();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            var p = _pctLabel.Parent as Panel;
            if (p != null) ResizeValue(p);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            GameDetector.Stop(); HotkeyManager.UnregisterAll(Handle);
            base.OnFormClosed(e);
        }
    }
}