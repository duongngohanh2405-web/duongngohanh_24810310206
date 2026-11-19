using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CineLuxe.UI
{
    /// <summary>Khối bo góc tô gradient chéo (hero, thẻ ưu đãi, thẻ hội viên).</summary>
    public class GradCard : Panel
    {
        public Color C1, C2;
        public int Arc = 22;
        public bool WhiteFade;      // lớp phủ trắng mờ dần từ trái sang (hero)
        public bool Circle;         // vòng tròn sáng ở góc phải (thẻ hội viên)

        public GradCard(Color c1, Color c2)
        {
            C1 = c1; C2 = c2;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint
                    | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rc = new Rectangle(0, 0, Math.Max(2, Width - 1), Math.Max(2, Height - 1));
            using (GraphicsPath path = Theme.Rounded(rc, Arc))
            {
                g.SetClip(path);
                using (LinearGradientBrush b = new LinearGradientBrush(rc, C1, C2, 30f)) g.FillRectangle(b, rc);
                if (WhiteFade)
                {
                    Rectangle fr = new Rectangle(0, 0, (int)(rc.Width * 0.65), rc.Height);
                    using (LinearGradientBrush b = new LinearGradientBrush(fr, Color.FromArgb(235, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 0f))
                        g.FillRectangle(b, fr);
                }
                if (Circle)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(25, 255, 255, 255))) g.FillEllipse(b, Width - 260, -100, 360, 360);
                g.ResetClip();
            }
        }
    }
}
