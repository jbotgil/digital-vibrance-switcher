using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
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
        NumericUpDown _defaultBox;

        ListBox _profileList;
        TextBox _profName, _profProc;
        NumericUpDown _profVal;
        ModernCheckBox _profEnabled;
        Label _profEmpty;
        int _editingProfileIndex = -1;

        Label _footerLabel, _gpuLabel;

        bool _ignoreEvents;
        int _circleSz = 72;

        public MainForm()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            MinimumSize = new Size(480, 500);
            Size = new Size(640, 720);
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
                if (!IsDisposed)
                {
                    SyncAll(e.NewValue);
                    _gpuLabel.Text = VibranceController.GetDisplayInfo().Replace("\n", "  ");
                }
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
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 6f));    // header
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));    // gap
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 18f));   // circle + %
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));    // gap
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 8f));    // slider
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 1f));    // gap
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 8f));    // presets
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 57f));   // settings + profiles

            tlp.Controls.Add(MkHeader(), 0, 0);
            tlp.Controls.Add(MkCircle(), 0, 2);
            tlp.Controls.Add(MkSlider(), 0, 4);
            tlp.Controls.Add(MkPresets(), 0, 6);
            tlp.Controls.Add(MkBottom(), 0, 7);

            return tlp;
        }

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
                using (var pen = new Pen(c, 4)) g.DrawArc(pen, 2, 2, d - 4, d - 4, 135, 360f * v / 100f);
                using (var br = new SolidBrush(Theme.Bg)) g.FillEllipse(br, 8, 8, d - 16, d - 16);
            };
            p.Controls.Add(_circleBox);

            _pctLabel = new Label
            {
                Text = "50%",
                Font = Theme.FontBold(24),
                ForeColor = Theme.Text,
                BackColor = Color.Transparent,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None
            };
            p.Controls.Add(_pctLabel);

            p.Resize += (s, e) => ResizeValue(p);
            return p;
        }

        void ResizeValue(Panel p)
        {
            int cx = p.Width / 2;
            int lblH = 36;

            int circleTop = 2;
            int maxCircleTop = Math.Max(2, p.Height - _circleSz - lblH);
            circleTop = Math.Min(circleTop, maxCircleTop);

            _circleBox.Location = new Point(cx - _circleSz / 2, circleTop);

            int lblY = _circleBox.Bottom;
            if (lblY + lblH > p.Height)
                lblY = Math.Max(0, p.Height - lblH);
            _pctLabel.SetBounds(0, lblY, p.Width, lblH);
        }

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

        TableLayoutPanel MkBottom()
        {
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
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

            var inner = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            inner.Controls.Add(new Label { Text = "SETTINGS", Font = Theme.FontBold(7.5f), ForeColor = Theme.TextMuted, BackColor = Color.Transparent, AutoSize = true, Height = 16 });

            var spdRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 2, 0, 0) };
            _speedBox = new NumericUpDown { Width = 64, Height = 24, Minimum = 0, Maximum = 10, Font = Theme.FontBold(11), ForeColor = Theme.Text, BackColor = Theme.Surface3, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center, Margin = new Padding(0, 1, 6, 0) };
            foreach (Control c in _speedBox.Controls) { c.BackColor = Theme.Surface3; c.ForeColor = Theme.Text; }
            _speedBox.ValueChanged += (s, e) => { SettingsManager.Current.TransitionSpeed = (int)_speedBox.Value; SettingsManager.Save(); };
            spdRow.Controls.Add(_speedBox);
            spdRow.Controls.Add(new Label { Text = "transition", Font = Theme.FontReg(7f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 4, 0, 0) });
            inner.Controls.Add(spdRow);

            var defRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 2, 0, 0) };
            _defaultBox = new NumericUpDown { Width = 64, Height = 24, Minimum = 0, Maximum = 100, Font = Theme.FontBold(11), ForeColor = Theme.Text, BackColor = Theme.Surface3, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center, Margin = new Padding(0, 1, 6, 0) };
            foreach (Control c in _defaultBox.Controls) { c.BackColor = Theme.Surface3; c.ForeColor = Theme.Text; }
            _defaultBox.ValueChanged += (s, e) => { SettingsManager.Current.DefaultVibrance = (int)_defaultBox.Value; SettingsManager.Save(); };
            defRow.Controls.Add(_defaultBox);
            defRow.Controls.Add(new Label { Text = "default %", Font = Theme.FontReg(7f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 4, 0, 0) });
            inner.Controls.Add(defRow);

            _startupCb = new ModernCheckBox(); _startupCb.SetText("Run at startup");
            _startupCb.CheckedChanged += (s, e) => SettingsManager.SetAutoStart(_startupCb.Checked);
            inner.Controls.Add(_startupCb);

            _hotkeyCb = new ModernCheckBox(); _hotkeyCb.SetText("Hotkeys C+A+1..4");
            _hotkeyCb.CheckedChanged += (s, e) =>
            {
                SettingsManager.Current.EnableHotkeys = _hotkeyCb.Checked; SettingsManager.Save();
                if (_hotkeyCb.Checked) HotkeyManager.RegisterAll(Handle); else HotkeyManager.UnregisterAll(Handle);
            };
            inner.Controls.Add(_hotkeyCb);

            _autoCb = new ModernCheckBox(); _autoCb.SetText("Auto-switch apps");
            _autoCb.CheckedChanged += (s, e) =>
            {
                SettingsManager.Current.AutoDetectApps = _autoCb.Checked; SettingsManager.Save();
                if (_autoCb.Checked) GameDetector.Start(); else GameDetector.Stop();
            };
            inner.Controls.Add(_autoCb);

            p.Controls.Add(inner);
            return p;
        }

        Panel MkProfiles()
        {
            var outer = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface, Padding = new Padding(8, 6, 10, 6) };
            outer.Paint += (s, e) => { using (var pen = new Pen(Theme.Border, 1)) e.Graphics.DrawRectangle(pen, 0, 0, outer.Width - 1, outer.Height - 1); };

            var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, BackColor = Color.Transparent };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 4));
            inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var header = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            header.Controls.Add(new Label { Text = "PROFILES", Font = Theme.FontBold(7.5f), ForeColor = Theme.TextMuted, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft });
            inner.Controls.Add(header, 0, 0);

            var listPanel = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface2 };
            _profileList = new ListBox
            {
                Dock = DockStyle.Fill, BackColor = Theme.Surface2, ForeColor = Theme.Text,
                Font = Theme.FontReg(8), BorderStyle = BorderStyle.None, IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed, ItemHeight = 38
            };
            _profileList.DrawItem += ProfileList_DrawItem;
            _profileList.SelectedIndexChanged += ProfileList_SelectedIndexChanged;
            _profileList.MouseClick += ProfileList_MouseClick;
            _profileList.MouseDoubleClick += (s, e) =>
            {
                int idx = _profileList.IndexFromPoint(e.Location);
                if (idx >= 0) ProfileManager.ActivateProfile(idx);
            };
            listPanel.Controls.Add(_profileList);

            _profEmpty = new Label
            {
                Text = "No profiles  |  Fill in the fields below and press Add",
                Font = Theme.FontReg(7f), ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter, Visible = false
            };
            listPanel.Controls.Add(_profEmpty);
            inner.Controls.Add(listPanel, 0, 1);

            var editor = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            var edFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent, Dock = DockStyle.Top, AutoSize = true };

            var fieldRow1 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            fieldRow1.Controls.Add(new Label { Text = "Name", Font = Theme.FontReg(7f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 3, 4, 0) });
            _profName = new TextBox { Font = Theme.FontReg(7.5f), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.FixedSingle, Height = 18, Width = 90, Margin = new Padding(0, 2, 10, 0) };
            fieldRow1.Controls.Add(_profName);
            fieldRow1.Controls.Add(new Label { Text = "DV%", Font = Theme.FontReg(7f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 3, 2, 0) });
            _profVal = new NumericUpDown { Minimum = 0, Maximum = 100, Value = 70, Width = 44, Height = 18, Font = Theme.FontBold(7.5f), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.None, Margin = new Padding(0, 2, 0, 0) };
            foreach (Control c in _profVal.Controls) { c.BackColor = Theme.Surface2; c.ForeColor = Theme.Text; }
            fieldRow1.Controls.Add(_profVal);
            edFlow.Controls.Add(fieldRow1);

            var fieldRow2 = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 2, 0, 0) };
            fieldRow2.Controls.Add(new Label { Text = "Process", Font = Theme.FontReg(7f), ForeColor = Theme.TextSec, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 3, 4, 0) });
            _profProc = new TextBox { Font = Theme.FontReg(7.5f), ForeColor = Theme.Text, BackColor = Theme.Surface2, BorderStyle = BorderStyle.FixedSingle, Height = 18, Width = 140, Margin = new Padding(0, 2, 4, 0) };
            fieldRow2.Controls.Add(_profProc);
            var browseB = new Button { Text = "Browse\u2026", Font = Theme.FontBold(7), ForeColor = Theme.TextSec, BackColor = Theme.Surface2, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1, BorderColor = Theme.Border }, Height = 20, Width = 56, Cursor = Cursors.Hand, Margin = new Padding(0, 1, 0, 0) };
            browseB.FlatAppearance.MouseOverBackColor = Theme.Surface3;
            browseB.Click += (s, e) =>
            {
                using (var loading = new LoadingDialog("Scanning installed applications\u2026"))
                {
                    InstalledAppScanner.AppEntry[] apps = null;
                    var scanThread = new Thread(() => { apps = InstalledAppScanner.Scan().ToArray(); });
                    scanThread.Start();
                    loading.StartPosition = FormStartPosition.CenterScreen;
                    loading.Show();
                    while (scanThread.IsAlive) { Application.DoEvents(); Thread.Sleep(20); }
                    loading.Close();
                }
                using (var dlg = new AppPickerDialog())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        _profProc.Text = dlg.SelectedProcess;
                        if (string.IsNullOrEmpty(_profName.Text))
                            _profName.Text = dlg.SelectedName;
                    }
                }
            };
            fieldRow2.Controls.Add(browseB);
            edFlow.Controls.Add(fieldRow2);

            var actionRow = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, WrapContents = true, BackColor = Color.Transparent, AutoSize = true, Margin = new Padding(0, 6, 0, 0) };

            var delB = new Button { Text = "\u2715", Font = Theme.FontBold(8), ForeColor = Color.White, BackColor = Theme.Error, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Width = 24, Height = 22, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0) };
            delB.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 120);
            new ToolTip().SetToolTip(delB, "Delete selected profile");
            delB.Click += (s, e) =>
            {
                int idx = _profileList.SelectedIndex;
                if (idx < 0) return;
                var profiles = SettingsManager.Current.Profiles;
                if (profiles == null || idx >= profiles.Count) return;
                string name = profiles[idx].Name;
                if (MessageBox.Show("Delete profile \"" + name + "\"?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    ProfileManager.RemoveProfile(idx);
                    RefreshProfiles();
                    ClearProfileEditor();
                }
            };
            actionRow.Controls.Add(delB);

            var saveB = new Button { Text = "Save", Font = Theme.FontBold(7.5f), ForeColor = Color.White, BackColor = Theme.Warning, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Width = 38, Height = 22, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0) };
            saveB.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 200, 90);
            new ToolTip().SetToolTip(saveB, "Update selected profile");
            saveB.Click += (s, e) =>
            {
                int idx = _profileList.SelectedIndex;
                if (idx < 0) return;
                string n = _profName.Text.Trim(), p3 = _profProc.Text.Trim().Replace(".exe", "");
                if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(p3)) return;
                ProfileManager.SetProfileEnabled(idx, _profEnabled.Checked);
                ProfileManager.UpdateProfile(idx, n, p3, (int)_profVal.Value);
                RefreshProfiles();
                _profileList.SelectedIndex = idx;
                ClearProfileEditor();
            };
            actionRow.Controls.Add(saveB);

            var addB = new Button { Text = "Add", Font = Theme.FontBold(7.5f), ForeColor = Color.White, BackColor = Theme.PrimaryDark, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Width = 34, Height = 22, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0) };
            addB.FlatAppearance.MouseOverBackColor = Theme.Primary;
            addB.Click += (s, e) =>
            {
                string n = _profName.Text.Trim(), p3 = _profProc.Text.Trim().Replace(".exe", "");
                if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(p3)) return;
                ProfileManager.AddProfile(n, p3, (int)_profVal.Value);
                RefreshProfiles();
                _profileList.SelectedIndex = SettingsManager.Current.Profiles.Count - 1;
                ClearProfileEditor();
            };
            actionRow.Controls.Add(addB);

            var playB = new Button { Text = "\u25B6", Font = Theme.FontBold(9), ForeColor = Color.White, BackColor = Theme.Success, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Width = 26, Height = 22, Cursor = Cursors.Hand, Margin = new Padding(2, 0, 2, 0) };
            playB.FlatAppearance.MouseOverBackColor = Color.FromArgb(90, 220, 150);
            new ToolTip().SetToolTip(playB, "Apply selected profile");
            playB.Click += (s, e) => { int idx = _profileList.SelectedIndex; if (idx >= 0) ProfileManager.ActivateProfile(idx); };
            actionRow.Controls.Add(playB);

            _profEnabled = new ModernCheckBox();
            _profEnabled.SetText("Active");
            _profEnabled.Margin = new Padding(0, 0, 6, 0);
            actionRow.Controls.Add(_profEnabled);

            edFlow.Controls.Add(actionRow);

            editor.Controls.Add(edFlow);
            inner.Controls.Add(editor, 0, 3);

            outer.Controls.Add(inner);

            ProfileManager.ActiveProfileChanged += (s, e) => SafeCall(() =>
            {
                if (!IsDisposed) RefreshProfiles();
            });

            return outer;
        }

        void ProfileList_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            if (e.Index >= ((ListBox)sender).Items.Count) return;

            var profiles = SettingsManager.Current.Profiles;
            if (profiles == null || e.Index >= profiles.Count) return;

            var profile = profiles[e.Index];
            bool isActive = e.Index == ProfileManager.ActiveProfileIndex && ProfileManager.ActiveProfileIndex >= 0;
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool isDisabled = !profile.IsEnabled;

            e.DrawBackground();

            int w = e.Bounds.Width;
            int h = e.Bounds.Height;
            int x = e.Bounds.X;
            int y = e.Bounds.Y;
            int pad = 4;

            Color nameColor = isDisabled ? Theme.TextMuted : Theme.Text;
            Color secColor = isDisabled ? Theme.TextMuted : Theme.TextSec;
            Color bgColor = Theme.Surface2;

            if (isActive)
                bgColor = Color.FromArgb(40, 40, 60);
            else if (isSelected)
                bgColor = Theme.Hover;

            using (var bgBrush = new SolidBrush(bgColor))
                e.Graphics.FillRectangle(bgBrush, e.Bounds);

            if (isActive)
            {
                using (var accentPen = new Pen(Theme.Primary, 2))
                    e.Graphics.DrawLine(accentPen, x, y, x, y + h - 1);
            }

            Font nameFont = Theme.FontBold(8.5f);
            Font smallFont = Theme.FontReg(7f);

            string checkMark = profile.IsEnabled ? "\u2611" : "\u2610";
            Color checkColor = profile.IsEnabled ? Theme.Primary : Theme.TextMuted;
            using (var checkBrush = new SolidBrush(checkColor))
                e.Graphics.DrawString(checkMark, nameFont, checkBrush, x + pad, y + 3);

            int nameX = x + pad + 20;
            string displayName = profile.Name.Length > 22 ? profile.Name.Substring(0, 20) + "\u2026" : profile.Name;
            using (var nameBrush = new SolidBrush(nameColor))
                e.Graphics.DrawString(displayName, nameFont, nameBrush, nameX, y + 3);

            using (var procBrush = new SolidBrush(secColor))
                e.Graphics.DrawString(profile.ProcessName, smallFont, procBrush, nameX, y + 20);

            int badgeX = x + w - 52;
            int badgeY = y + 6;
            int badgeW = 46;
            int badgeH = 16;

            Color badgeColor = Theme.ValueColor(profile.VibranceValue);
            using (var badgeBg = new SolidBrush(Color.FromArgb(60, badgeColor)))
            using (var badgePen = new Pen(Color.FromArgb(120, badgeColor)))
            {
                var badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);
                e.Graphics.FillRectangle(badgeBg, badgeRect);
                e.Graphics.DrawRectangle(badgePen, badgeRect);
            }

            string badgeText = profile.VibranceValue + "%";
            using (var badgeBrush = new SolidBrush(badgeColor))
                e.Graphics.DrawString(badgeText, Theme.FontBold(7.5f), badgeBrush, badgeX, badgeY + 1);

            if (isDisabled)
            {
                using (var dimBrush = new SolidBrush(Color.FromArgb(80, Theme.Surface2)))
                    e.Graphics.FillRectangle(dimBrush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        void ProfileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = _profileList.SelectedIndex;
            var profiles = SettingsManager.Current.Profiles;
            if (profiles != null && idx >= 0 && idx < profiles.Count)
            {
                _profName.Text = profiles[idx].Name;
                _profProc.Text = profiles[idx].ProcessName;
                _profVal.Value = profiles[idx].VibranceValue;
                _profEnabled.Checked = profiles[idx].IsEnabled;
                _editingProfileIndex = idx;
            }
        }

        void ProfileList_MouseClick(object sender, MouseEventArgs e)
        {
            int idx = _profileList.IndexFromPoint(e.Location);
            if (idx < 0) return;

            if (e.X < 24)
            {
                var profiles = SettingsManager.Current.Profiles;
                if (profiles != null && idx < profiles.Count)
                {
                    bool newState = !profiles[idx].IsEnabled;
                    ProfileManager.SetProfileEnabled(idx, newState);
                    RefreshProfiles();
                    if (_profileList.SelectedIndex == idx)
                        _profEnabled.Checked = newState;
                }
            }
        }

        void ClearProfileEditor()
        {
            _profName.Text = "";
            _profProc.Text = "";
            _profVal.Value = 70;
            _profEnabled.Checked = true;
            _editingProfileIndex = -1;
        }

        Panel MkFooter()
        {
            var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            _footerLabel = new Label
            {
                Text = "\u2014", Dock = DockStyle.Left, AutoSize = true,
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
            _defaultBox.Value = s.DefaultVibrance;
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
            for (int i = 0; i < p.Count; i++)
                _profileList.Items.Add(p[i].ProcessName);
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