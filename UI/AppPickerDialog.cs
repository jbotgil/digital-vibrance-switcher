using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DigitalVibrance.Core;

namespace DigitalVibrance.UI
{
    class AppPickerDialog : Form
    {
        public string SelectedProcess { get; private set; }
        public string SelectedName { get; private set; }

        TextBox _search;
        ListBox _list;
        InstalledAppScanner.AppEntry[] _allApps;

        public AppPickerDialog()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Select application";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(380, 460);
            BackColor = Theme.Bg;
            ForeColor = Theme.Text;
            Font = Theme.FontReg(9);

            var tlp = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16, 12, 16, 12), ColumnCount = 1, RowCount = 3, BackColor = Color.Transparent };
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));

            _search = new TextBox
            {
                Dock = DockStyle.Fill, Font = Theme.FontReg(9),
                ForeColor = Theme.Text, BackColor = Theme.Surface2,
                BorderStyle = BorderStyle.FixedSingle
            };
            _search.TextChanged += (s, e) => RefreshList();
            tlp.Controls.Add(_search, 0, 0);

            _list = new ListBox
            {
                Dock = DockStyle.Fill, Font = Theme.FontReg(8.5f),
                ForeColor = Theme.Text, BackColor = Theme.Surface2,
                BorderStyle = BorderStyle.None, IntegralHeight = false,
                DisplayMember = "Name"
            };
            _list.DoubleClick += (s, e) => Accept();
            tlp.Controls.Add(_list, 0, 1);

            var btnRow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, BackColor = Color.Transparent };
            var ok = new Button { Text = "Select", Width = 80, Height = 30, DialogResult = DialogResult.OK, BackColor = Theme.PrimaryDark, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 0 }, Cursor = Cursors.Hand };
            ok.Click += (s, e) => Accept();
            btnRow.Controls.Add(ok);
            var cancel = new Button { Text = "Cancel", Width = 80, Height = 30, DialogResult = DialogResult.Cancel, BackColor = Theme.Surface2, ForeColor = Theme.TextSec, FlatStyle = FlatStyle.Flat, FlatAppearance = { BorderSize = 1, BorderColor = Theme.Border }, Cursor = Cursors.Hand, Margin = new Padding(8, 0, 0, 0) };
            btnRow.Controls.Add(cancel);
            tlp.Controls.Add(btnRow, 0, 2);

            Controls.Add(tlp);

            // Scan once (cached), then filter in-memory on each keystroke
            _allApps = InstalledAppScanner.Scan().ToArray();
            RefreshList();
        }

        void RefreshList()
        {
            string q = _search.Text.Trim();
            _list.Items.Clear();
            if (_allApps == null) return;
            foreach (var a in _allApps)
                if (string.IsNullOrEmpty(q) || a.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    a.ProcessName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                    _list.Items.Add(a);
            if (_list.Items.Count > 0) _list.SelectedIndex = 0;
        }

        void Accept()
        {
            var app = _list.SelectedItem as InstalledAppScanner.AppEntry;
            if (app == null) return;
            SelectedProcess = app.ProcessName;
            SelectedName = app.Name;
            DialogResult = DialogResult.OK;
        }
    }
}