using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace CineLuxe.UI
{
    /// <summary>Màu sắc, font và các thành phần giao diện dùng chung (phong cách Ruby của thiết kế Figma).</summary>
    public static class Theme
    {
        public static readonly Color RUBY = Color.FromArgb(224, 24, 72);
        public static readonly Color RUBY_DARK = Color.FromArgb(184, 0, 48);
        public static readonly Color RUBY_SOFT = Color.FromArgb(254, 236, 240);
        public static readonly Color BG = Color.FromArgb(247, 247, 250);
        public static readonly Color TEXT = Color.FromArgb(31, 36, 48);
        public static readonly Color MUTED = Color.FromArgb(107, 114, 128);
        public static readonly Color BORDER = Color.FromArgb(229, 231, 235);
        public static readonly Color GREEN = Color.FromArgb(22, 163, 74);
        public static readonly Color GREEN_SOFT = Color.FromArgb(220, 252, 231);
        public static readonly Color AMBER = Color.FromArgb(217, 119, 6);
        public static readonly Color AMBER_SOFT = Color.FromArgb(254, 243, 199);
        public static readonly Color DARK = Color.FromArgb(24, 24, 32);

        public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        /// <summary>Font Segoe UI, kích thước tính bằng pixel (giống bản Java).</summary>
        public static Font Fnt(FontStyle style, float px)
        {
            return new Font("Segoe UI", px, style, GraphicsUnit.Pixel);
        }

        public static string Money(long v)
        {
            return v.ToString("N0", Inv).Replace(',', '.') + "đ";
        }

        /// <summary>Màu trắng trong suốt đặt trên nền tối (dùng cho chữ - chữ không hỗ trợ alpha).</summary>
        public static Color OnDark(int alpha)
        {
            double a = alpha / 255.0;
            return Color.FromArgb((int)(40 + (255 - 40) * a), (int)(14 + (255 - 14) * a), (int)(28 + (255 - 28) * a));
        }

        public static Color Mix(Color a, Color b, double t)
        {
            return Color.FromArgb((int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }

        public static Color Darker(Color c)
        {
            return Color.FromArgb(c.A, (int)(c.R * 0.8), (int)(c.G * 0.8), (int)(c.B * 0.8));
        }

        /// <summary>Hash chuỗi ổn định giữa các lần chạy (giống String.hashCode của Java) để màu poster không đổi.</summary>
        public static int StableHash(string s)
        {
            int h = 0;
            unchecked { foreach (char ch in s) h = 31 * h + ch; }
            return h;
        }

        /// <summary>HSB -> RGB (giống Color.getHSBColor của Java).</summary>
        public static Color FromHsb(float hue, float sat, float bri)
        {
            int r = 0, g = 0, b = 0;
            if (sat == 0) { r = g = b = (int)(bri * 255.0f + 0.5f); }
            else
            {
                float h = (hue - (float)Math.Floor(hue)) * 6.0f;
                float f = h - (float)Math.Floor(h);
                float p = bri * (1.0f - sat), q = bri * (1.0f - sat * f), t = bri * (1.0f - sat * (1.0f - f));
                switch ((int)h)
                {
                    case 0: r = (int)(bri * 255.0f + 0.5f); g = (int)(t * 255.0f + 0.5f); b = (int)(p * 255.0f + 0.5f); break;
                    case 1: r = (int)(q * 255.0f + 0.5f); g = (int)(bri * 255.0f + 0.5f); b = (int)(p * 255.0f + 0.5f); break;
                    case 2: r = (int)(p * 255.0f + 0.5f); g = (int)(bri * 255.0f + 0.5f); b = (int)(t * 255.0f + 0.5f); break;
                    case 3: r = (int)(p * 255.0f + 0.5f); g = (int)(q * 255.0f + 0.5f); b = (int)(bri * 255.0f + 0.5f); break;
                    case 4: r = (int)(t * 255.0f + 0.5f); g = (int)(p * 255.0f + 0.5f); b = (int)(bri * 255.0f + 0.5f); break;
                    default: r = (int)(bri * 255.0f + 0.5f); g = (int)(p * 255.0f + 0.5f); b = (int)(q * 255.0f + 0.5f); break;
                }
            }
            return Color.FromArgb(r, g, b);
        }

        public static GraphicsPath Rounded(RectangleF r, float arc)
        {
            GraphicsPath p = new GraphicsPath();
            float d = Math.Min(arc, Math.Min(r.Width, r.Height));
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ---------------- nhãn ----------------
        /// <summary>Nhãn chữ. wrap &gt; 0: tự xuống dòng khi vượt quá chiều rộng (pixel).</summary>
        public static Label Lbl(string text, FontStyle style, float size, Color color, int wrap = 0)
        {
            Label l = new Label();
            l.Text = text; l.Font = Fnt(style, size); l.ForeColor = color;
            l.AutoSize = true; l.BackColor = Color.Transparent; l.Margin = Padding.Empty; l.UseMnemonic = false;
            if (wrap > 0) l.MaximumSize = new Size(wrap, 0);
            return l;
        }

        public static Label Lbl(string text) { return Lbl(text, FontStyle.Regular, 14, TEXT); }
        public static Label Title(string text) { return Lbl(text, FontStyle.Bold, 22, TEXT); }
        public static Label Muted(string text) { return Lbl(text, FontStyle.Regular, 13, MUTED); }

        public static Label Badge(string text, Color fg, Color bg)
        {
            Label l = Lbl(text, FontStyle.Bold, 11, fg);
            l.BackColor = bg; l.Padding = new Padding(6, 2, 6, 2);
            return l;
        }

        // ---------------- thành phần ----------------
        /// <summary>Thẻ trắng bo góc, viền nhạt.</summary>
        public static RoundPanel Card()
        {
            RoundPanel p = new RoundPanel(18, Color.White, BORDER);
            p.Padding = new Padding(18, 16, 18, 16);
            return p;
        }

        public static RoundButton Primary(string t) { return new RoundButton(t, RUBY, Color.White, Color.Empty); }
        public static RoundButton Outline(string t) { return new RoundButton(t, Color.White, TEXT, BORDER); }
        public static RoundButton Ghost(string t) { return new RoundButton(t, Color.Empty, MUTED, Color.Empty); }

        /// <summary>Nút điều hướng dạng chữ (menu).</summary>
        public static Button NavButton(string text)
        {
            Button b = new Button();
            b.Text = text; b.Font = Fnt(FontStyle.Bold, 13); b.ForeColor = MUTED;
            b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = RUBY_SOFT; b.FlatAppearance.MouseDownBackColor = RUBY_SOFT;
            b.BackColor = Color.White; b.AutoSize = true; b.Padding = new Padding(8, 4, 8, 4);
            b.Cursor = Cursors.Hand; b.TabStop = false; b.Margin = new Padding(2, 0, 2, 0); b.UseMnemonic = false;
            return b;
        }

        public static Input Field() { return new Input(false); }
        public static Input Password() { return new Input(true); }

        public static DataGridView Table(params string[] cols)
        {
            DataGridView t = new DataGridView();
            t.ReadOnly = true; t.AllowUserToAddRows = false; t.AllowUserToDeleteRows = false; t.AllowUserToResizeRows = false;
            t.AllowUserToOrderColumns = false; t.RowHeadersVisible = false; t.MultiSelect = false;
            t.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            t.BackgroundColor = Color.White; t.BorderStyle = BorderStyle.None; t.GridColor = BORDER;
            t.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            t.EnableHeadersVisualStyles = false;
            t.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            t.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            t.ColumnHeadersHeight = 36;
            t.RowTemplate.Height = 34;
            t.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            t.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(249, 250, 251);
            t.ColumnHeadersDefaultCellStyle.ForeColor = MUTED;
            t.ColumnHeadersDefaultCellStyle.Font = Fnt(FontStyle.Bold, 12);
            t.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(249, 250, 251);
            t.ColumnHeadersDefaultCellStyle.SelectionForeColor = MUTED;
            t.DefaultCellStyle.Font = Fnt(FontStyle.Regular, 13);
            t.DefaultCellStyle.ForeColor = TEXT;
            t.DefaultCellStyle.BackColor = Color.White;
            t.DefaultCellStyle.SelectionBackColor = RUBY_SOFT;
            t.DefaultCellStyle.SelectionForeColor = TEXT;
            foreach (string c in cols)
            {
                int i = t.Columns.Add(c, c);
                t.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            return t;
        }

        /// <summary>Chỉ số dòng đang chọn trong bảng (-1 nếu chưa chọn).</summary>
        public static int SelectedIndex(DataGridView t)
        {
            return t.SelectedRows.Count > 0 ? t.SelectedRows[0].Index : -1;
        }

        public static void ClearSel(DataGridView t)
        {
            t.ClearSelection();
            t.CurrentCell = null;
        }

        public static void Info(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, "CineLuxury", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Error(IWin32Window owner, string msg)
        {
            MessageBox.Show(owner, msg, "CineLuxury", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool Confirm(IWin32Window owner, string msg)
        {
            return MessageBox.Show(owner, msg, "CineLuxury", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }

    // =====================================================================
    // CÁC CONTROL TỰ VẼ
    // =====================================================================

    /// <summary>Panel bo góc có màu nền và viền (nền trong suốt ở 4 góc).</summary>
    public class RoundPanel : Panel
    {
        private readonly int arc;
        private Color fill, line;

        public RoundPanel(int arc, Color fill, Color line)
        {
            this.arc = arc; this.fill = fill; this.line = line;
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            BackColor = Color.Transparent;
            AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Margin = Padding.Empty;
        }

        public void SetFill(Color c) { fill = c; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = Theme.Rounded(r, arc))
            {
                using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, p);
                if (line != Color.Empty) using (Pen pen = new Pen(line)) g.DrawPath(pen, p);
            }
        }
    }

    /// <summary>Nút bo góc tự vẽ (màu nền / chữ / viền đổi được).</summary>
    public class RoundButton : Control
    {
        private Color bg, fg, border;
        private bool hover, down;

        public RoundButton(string text, Color bg, Color fg, Color border)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.Selectable, true);
            this.bg = bg; this.fg = fg; this.border = border;
            Text = text; Font = Theme.Fnt(FontStyle.Bold, 13); ForeColor = fg;
            BackColor = Color.Transparent; Cursor = Cursors.Hand;
            AutoSize = true; Margin = new Padding(0, 0, 0, 0); TabStop = false;
        }

        public void SetColors(Color bg, Color fg, Color border)
        {
            this.bg = bg; this.fg = fg; this.border = border; ForeColor = fg; Invalidate();
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size s = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            int w = s.Width + 36, h = s.Height + 20;
            w = Math.Max(w, MinimumSize.Width); h = Math.Max(h, MinimumSize.Height);
            if (MaximumSize.Width > 0) w = Math.Min(w, MaximumSize.Width);
            if (MaximumSize.Height > 0) h = Math.Min(h, MaximumSize.Height);
            return new Size(w, h);
        }

        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); if (AutoSize) { Size = GetPreferredSize(Size.Empty); } Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Cursor = Enabled ? Cursors.Hand : Cursors.Default; Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); hover = false; down = false; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); down = true; Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); down = false; Invalidate(); }

        // Xử lý Click sau khi sự kiện chuột kết thúc, để handler được phép dựng lại / hủy chính nút này.
        protected override void OnClick(EventArgs e)
        {
            if (!Enabled) return;
            if (IsHandleCreated) BeginInvoke(new Action(() => { if (!IsDisposed) RaiseClick(e); }));
            else RaiseClick(e);
        }

        private void RaiseClick(EventArgs e) { base.OnClick(e); }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (Enabled && (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)) OnClick(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            RectangleF r = new RectangleF(0, 0, Width - 1, Height - 1);
            Color fill = !Enabled ? Color.FromArgb(209, 213, 219)
                    : (hover || down) && bg != Color.Empty ? Theme.Darker(bg) : bg;
            using (GraphicsPath p = Theme.Rounded(r, 14))
            {
                if (fill != Color.Empty) using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, p);
                if (border != Color.Empty && Enabled) using (Pen pen = new Pen(border)) g.DrawPath(pen, p);
            }
            Color txt = !Enabled ? Color.White : (hover && bg == Color.Empty ? Theme.TEXT : fg);
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, txt,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
        }
    }

    /// <summary>Ô nhập liệu có viền bo nhẹ (bọc một TextBox).</summary>
    public class Input : Panel
    {
        public readonly TextBox Box = new TextBox();
        public event EventHandler Submit;

        public Input(bool password)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Height = 40; MinimumSize = new Size(40, 40); MaximumSize = new Size(0, 40);
            Padding = new Padding(12, 11, 12, 4);
            BackColor = Color.White; Margin = Padding.Empty;
            Box.BorderStyle = BorderStyle.None; Box.Dock = DockStyle.Fill;
            Box.Font = Theme.Fnt(FontStyle.Regular, 14); Box.BackColor = Color.White; Box.ForeColor = Theme.TEXT;
            Box.UseSystemPasswordChar = password;
            Box.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    if (Submit != null) Submit(this, EventArgs.Empty);
                }
            };
            Controls.Add(Box);
        }

        public override string Text { get { return Box.Text; } set { Box.Text = value; } }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, Width - 1, Height - 1), 10))
            using (Pen pen = new Pen(Box.Focused ? Theme.RUBY : Theme.BORDER)) e.Graphics.DrawPath(pen, p);
        }

        protected override void OnEnter(EventArgs e) { base.OnEnter(e); Invalidate(); }
        protected override void OnLeave(EventArgs e) { base.OnLeave(e); Invalidate(); }
    }

    /// <summary>Poster vẽ bằng gradient (thay ảnh poster). Nếu có file posters/&lt;id&gt;.jpg sẽ dùng ảnh thật.</summary>
    public class Poster : Control
    {
        private readonly string title, sub;
        private readonly Color c1, c2;
        private Image img;

        public Poster(string movieId, string title, string sub, Size size, bool stretchWidth = false)
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint
                    | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            this.title = title; this.sub = sub;
            int h = Math.Abs(Theme.StableHash(movieId) % 360);
            float hue = h / 360f;
            c1 = Theme.FromHsb(hue, 0.65f, 0.55f); c2 = Theme.FromHsb((hue + 0.08f) % 1f, 0.8f, 0.2f);
            string f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "posters", movieId + ".jpg");
            if (File.Exists(f))
            {
                try { using (Image tmp = Image.FromFile(f)) img = new Bitmap(tmp); } catch (Exception) { img = null; }
            }
            Size = size; Margin = Padding.Empty;
            if (stretchWidth) { MinimumSize = new Size(1, size.Height); MaximumSize = new Size(0, size.Height); }
            else { MinimumSize = size; MaximumSize = size; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            Rectangle rc = new Rectangle(0, 0, Width, Height);
            using (GraphicsPath clip = Theme.Rounded(rc, 16))
            {
                g.SetClip(clip);
                if (img != null) g.DrawImage(img, rc);
                else
                {
                    using (LinearGradientBrush b = new LinearGradientBrush(rc, c1, c2, 45f)) g.FillRectangle(b, rc);
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                        g.FillEllipse(b, -Width / 3, Height / 2, Width * 2, Width * 2);
                }
                int y0 = (int)(Height * 0.45);
                Rectangle shade = new Rectangle(0, y0, Width, Math.Max(1, Height - y0));
                using (LinearGradientBrush b = new LinearGradientBrush(shade, Color.FromArgb(0, 0, 0, 0), Color.FromArgb(190, 0, 0, 0), 90f))
                    g.FillRectangle(b, shade);
                using (Font f = Theme.Fnt(FontStyle.Bold, 15))
                using (StringFormat sf = new StringFormat())
                {
                    sf.LineAlignment = StringAlignment.Far;
                    RectangleF box = new RectangleF(12, 0, Width - 24, Height - 14 - (sub == null ? 0 : 18));
                    g.DrawString(title, f, Brushes.White, box, sf);
                }
                if (sub != null)
                    using (Font f = Theme.Fnt(FontStyle.Regular, 11))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
                        g.DrawString(sub, f, b, 12, Height - 12 - f.Height);
                g.ResetClip();
            }
        }
    }

    // =====================================================================
    // BỐ CỤC
    // =====================================================================

    /// <summary>Xếp các control từ trên xuống dưới (mỗi control một hàng, tự co theo nội dung).</summary>
    public class Stack : TableLayoutPanel
    {
        public Stack()
        {
            ColumnCount = 1;
            AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Dock = DockStyle.Top; BackColor = Color.Transparent; Margin = Padding.Empty;
            ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        }

        /// <summary>Xóa toàn bộ hàng (giải phóng các control con).</summary>
        public void Reset()
        {
            SuspendLayout();
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                Control c = Controls[i];
                Controls.RemoveAt(i);
                c.Dispose();
            }
            RowStyles.Clear();
            RowCount = 0;
            ResumeLayout(true);
        }

        /// <summary>Thêm một hàng. gapTop: khoảng cách phía trên; stretch=false thì giữ kích thước riêng, canh trái.</summary>
        public T Add<T>(T c, int gapTop = 0, bool stretch = true) where T : Control
        {
            RowStyles.Add(new RowStyle(SizeType.AutoSize));
            RowCount = RowStyles.Count;
            c.Margin = new Padding(0, gapTop, 0, 0);
            if (stretch) c.Dock = DockStyle.Fill;
            else c.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Controls.Add(c, 0, RowCount - 1);
            return c;
        }
    }

    public static class Lay
    {
        /// <summary>Hàng ngang (các control xếp từ trái sang phải, cách nhau gap pixel).</summary>
        public static FlowLayoutPanel Row(int gap, params Control[] items)
        {
            FlowLayoutPanel f = new FlowLayoutPanel();
            f.FlowDirection = FlowDirection.LeftToRight; f.WrapContents = false;
            f.AutoSize = true; f.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            f.BackColor = Color.Transparent; f.Margin = Padding.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                items[i].Margin = new Padding(0, 0, i == items.Length - 1 ? 0 : gap, 0);
                items[i].Anchor = AnchorStyles.Left;          // canh giữa theo chiều dọc
                f.Controls.Add(items[i]);
            }
            return f;
        }

        /// <summary>Bố cục West - Center - East (giống BorderLayout): giữa co giãn, hai bên theo nội dung.</summary>
        public static TableLayoutPanel Border(Control west, Control center, Control east)
        {
            TableLayoutPanel t = new TableLayoutPanel();
            t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.Dock = DockStyle.Fill;
            t.BackColor = Color.Transparent; t.Margin = Padding.Empty;
            t.RowCount = 1; t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.ColumnCount = 3;
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            if (west != null) { west.Margin = Padding.Empty; west.Anchor = AnchorStyles.Left; t.Controls.Add(west, 0, 0); }
            if (center != null) { center.Margin = Padding.Empty; center.Dock = DockStyle.Fill; t.Controls.Add(center, 1, 0); }
            if (east != null) { east.Margin = Padding.Empty; east.Anchor = AnchorStyles.Right; t.Controls.Add(east, 2, 0); }
            return t;
        }

        /// <summary>Lưới nhiều cột bằng nhau (giống GridLayout). Các ô được thêm lần lượt từ trái sang phải, trên xuống dưới.</summary>
        public static TableLayoutPanel Grid(int cols, int hgap, int vgap, IList<Control> items)
        {
            TableLayoutPanel t = new TableLayoutPanel();
            t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.Dock = DockStyle.Fill;
            t.BackColor = Color.Transparent; t.Margin = Padding.Empty;
            t.ColumnCount = cols;
            for (int i = 0; i < cols; i++) t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / cols));
            int rows = (items.Count + cols - 1) / cols;
            for (int r = 0; r < rows; r++) t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            t.RowCount = rows;
            for (int i = 0; i < items.Count; i++)
            {
                Control c = items[i];
                int col = i % cols, row = i / cols;
                c.Dock = DockStyle.Fill;
                c.Margin = new Padding(0, row == 0 ? 0 : vgap, col == cols - 1 ? 0 : hgap, 0);
                t.Controls.Add(c, col, row);
            }
            return t;
        }

        public static TableLayoutPanel Grid(int cols, int hgap, params Control[] items)
        {
            return Grid(cols, hgap, 0, items);
        }

        /// <summary>Hai cột với tỉ lệ cho trước (ví dụ 62 / 38).</summary>
        public static TableLayoutPanel Split(Control left, Control right, float leftPercent, int gap)
        {
            TableLayoutPanel t = new TableLayoutPanel();
            t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.Dock = DockStyle.Fill;
            t.BackColor = Color.Transparent; t.Margin = Padding.Empty;
            t.ColumnCount = 2; t.RowCount = 1;
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, leftPercent));
            t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 - leftPercent));
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.Dock = DockStyle.Fill; left.Margin = new Padding(0, 0, gap, 0);
            right.Dock = DockStyle.Fill; right.Margin = new Padding(gap, 0, 0, 0);
            t.Controls.Add(left, 0, 0); t.Controls.Add(right, 1, 0);
            return t;
        }

        /// <summary>Bố cục Bắc - Giữa - Nam: tiêu đề trên, nội dung co giãn, chân dưới.</summary>
        public static TableLayoutPanel NCS(Control north, Control center, Control south, int gap)
        {
            TableLayoutPanel t = new TableLayoutPanel();
            t.AutoSize = true; t.AutoSizeMode = AutoSizeMode.GrowAndShrink; t.Dock = DockStyle.Fill;
            t.BackColor = Color.Transparent; t.Margin = Padding.Empty;
            t.ColumnCount = 1; t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            int r = 0;
            foreach (Control c in new[] { north, center, south })
            {
                if (c == null) continue;
                t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                c.Dock = DockStyle.Fill; c.Margin = new Padding(0, r == 0 ? 0 : gap, 0, 0);
                t.Controls.Add(c, 0, r); r++;
            }
            t.RowCount = r;
            return t;
        }

        /// <summary>Vùng cuộn dọc chứa một nội dung (nội dung tự co theo chiều rộng).</summary>
        public static Panel Scroll(Control content)
        {
            Panel p = new Panel();
            p.Dock = DockStyle.Fill; p.AutoScroll = true; p.BackColor = Theme.BG;
            content.Dock = DockStyle.Top;
            p.Controls.Add(content);
            return p;
        }

        /// <summary>Tách một control vào panel trong suốt có khoảng đệm.</summary>
        public static Panel Pad(Control c, int l, int t, int r, int b)
        {
            Panel p = new Panel();
            p.AutoSize = true; p.AutoSizeMode = AutoSizeMode.GrowAndShrink; p.BackColor = Color.Transparent;
            p.Padding = new Padding(l, t, r, b); p.Margin = Padding.Empty;
            c.Dock = DockStyle.Top;
            p.Controls.Add(c);
            return p;
        }
    }
}
