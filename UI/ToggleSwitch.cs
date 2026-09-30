using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DigitalVibrance.UI
{
    class ToggleSwitch : Control
    {
        bool _checked;
        bool _hovered;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (value != _checked)
                {
                    _checked = value;
                    Invalidate();
                    if (CheckedChanged != null)
                        CheckedChanged(this, EventArgs.Empty);
                }
            }
        }

        public ToggleSwitch()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer, true);
            Size = new Size(160, 22);
            Font = Theme.FontReg(9);
            ForeColor = Theme.Text;
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            DoubleBuffered = true;
        }

        public void SetText(string t) { Text = t; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Bg);

            int boxSize = 16;
            int boxY = (Height - boxSize) / 2;
            var boxRect = new Rectangle(0, boxY, boxSize, boxSize);

            Color boxBg = _checked ? Theme.Primary : Theme.Surface3;
            Color boxBorder = _hovered ? Theme.Primary : Theme.Border;

            using (var br = new SolidBrush(boxBg))
                g.FillRoundedRect(br, boxRect, 3);
            using (var pen = new Pen(boxBorder, 1.5f))
                g.DrawRoundedRect(pen, boxRect, 3);

            if (_checked)
            {
                using (var pen = new Pen(Color.White, 2))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    int pad = 3;
                    g.DrawLine(pen, boxRect.Left + pad, boxRect.Top + boxSize / 2,
                               boxRect.Left + boxSize / 2, boxRect.Bottom - pad);
                    g.DrawLine(pen, boxRect.Left + boxSize / 2, boxRect.Bottom - pad,
                               boxRect.Right - pad, boxRect.Top + pad);
                }
            }

            var textRect = new Rectangle(boxSize + 6, 0, Width - boxSize - 6, Height);
            var fmt = new StringFormat { LineAlignment = StringAlignment.Center, Trimming = StringTrimming.EllipsisCharacter };
            using (var br = new SolidBrush(Enabled ? ForeColor : Theme.TextMuted))
            using (var f = Theme.FontReg(9))
                g.DrawString(Text, f, br, textRect, fmt);
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { Checked = !Checked; base.OnMouseDown(e); }
    }

    static class GraphicsExt
    {
        public static void FillRoundedRect(this Graphics g, Brush brush, Rectangle rect, int r)
        {
            using (var p = RoundedRect(rect, r)) g.FillPath(brush, p);
        }
        public static void DrawRoundedRect(this Graphics g, Pen pen, Rectangle rect, int r)
        {
            using (var p = RoundedRect(rect, r)) g.DrawPath(pen, p);
        }
        static GraphicsPath RoundedRect(Rectangle rect, int r)
        {
            var p = new GraphicsPath();
            int d = r * 2;
            p.AddArc(rect.X, rect.Y, d, d, 180, 90);
            p.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            p.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            p.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}