using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DigitalVibrance.UI
{
    class SliderBar : Control
    {
        int _value, _min, _max;
        bool _dragging, _hovered;
        Rectangle _trackRect;

        public event EventHandler ValueChanged;

        public int Value
        {
            get { return _value; }
            set
            {
                int v = Math.Max(_min, Math.Min(_max, value));
                if (v != _value) { _value = v; Invalidate(); var h = ValueChanged; if (h != null) h(this, EventArgs.Empty); }
            }
        }
        public int Minimum { get { return _min; } set { _min = value; Invalidate(); } }
        public int Maximum { get { return _max; } set { _max = value; Invalidate(); } }

        public SliderBar()
        {
            _min = 0; _max = 100; _value = 50;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 44; Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Parent != null ? Parent.BackColor : Theme.Bg);

            int trackH = 6, trackY = Height / 2 - trackH / 2;
            _trackRect = new Rectangle(8, trackY, Width - 16, trackH);

            using (var br = new SolidBrush(Theme.Surface3))
                g.FillRound(br, _trackRect, 3);

            float pct = (float)(_value - _min) / (_max - _min);
            int fillW = (int)((_trackRect.Width - 4) * pct);
            Color fillColor = Theme.ValueColor(_value);
            if (fillW > 0)
            {
                var fillR = new Rectangle(_trackRect.X + 2, _trackRect.Y + 1, fillW, _trackRect.Height - 2);
                using (var br = new SolidBrush(fillColor))
                    g.FillRound(br, fillR, 2);
            }

            int thumbS = _hovered || _dragging ? 18 : 14;
            int thumbX = _trackRect.X + 2 + Math.Max(0, fillW - thumbS / 2);
            int thumbY = _trackRect.Y + _trackRect.Height / 2 - thumbS / 2;
            var thumbR = new Rectangle(thumbX, thumbY, thumbS, thumbS);
            using (var br = new SolidBrush(Color.White))
            using (var pen = new Pen(fillColor, 2))
            {
                g.FillEllipse(br, thumbR);
                g.DrawEllipse(pen, thumbR);
            }

            using (var br = new SolidBrush(Theme.TextMuted))
            using (var f = new Font("Segoe UI", 7.5f))
            {
                g.DrawString(_min + "%", f, br, _trackRect.X, _trackRect.Bottom + 4);
                string maxT = _max + "%";
                float mxW = g.MeasureString(maxT, f).Width;
                g.DrawString(maxT, f, br, _trackRect.Right - mxW, _trackRect.Bottom + 4);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { _dragging = true; UpdateVal(e.X); }
            base.OnMouseDown(e);
        }
        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging) UpdateVal(e.X);
            base.OnMouseMove(e);
        }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; _dragging = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _dragging = false; Invalidate(); base.OnMouseUp(e); }

        void UpdateVal(int mx)
        {
            int tl = _trackRect.X + 2, tw = _trackRect.Width - 4;
            if (tw <= 0) return;
            Value = (int)(_min + (float)(mx - tl) / tw * (_max - _min));
        }
    }

    static class RoundHelper
    {
        public static void FillRound(this Graphics g, Brush br, Rectangle r, int rad)
        {
            using (var p = RoundRect(r, rad)) g.FillPath(br, p);
        }
        public static void DrawRound(this Graphics g, Pen pen, Rectangle r, int rad)
        {
            using (var p = RoundRect(r, rad)) g.DrawPath(pen, p);
        }
        public static GraphicsPath RoundRect(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            int d = rad * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}