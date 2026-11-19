using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Giao diện Quản lý hệ thống: Tổng quan / Quản lý phim / Suất chiếu &amp; Phòng / Báo cáo &amp; Doanh thu.</summary>
    public class AdminForm : AppForm
    {
        private readonly User admin;
        private readonly Panel content = new Panel();
        private readonly Button[] nav = new Button[4];
        private readonly Panel dashboard = new Panel();
        private readonly AdminMoviesPanel movies = new AdminMoviesPanel();
        private readonly AdminSchedulePanel schedule = new AdminSchedulePanel();
        private readonly AdminReportPanel report = new AdminReportPanel();
        private string chartMode = "Ngày";

        public AdminForm(User admin) : base("CineLuxury - Cổng quản trị hệ thống", 1320, 840)
        {
            this.admin = admin;
            content.Dock = DockStyle.Fill; content.BackColor = Theme.BG;
            dashboard.Dock = DockStyle.Fill; dashboard.BackColor = Theme.BG;
            content.Controls.Add(dashboard);
            content.Controls.Add(movies);
            content.Controls.Add(schedule);
            content.Controls.Add(report);
            Controls.Add(content);
            Controls.Add(BuildSidebar());           // thêm sau để dock bên trái
            ShowPage(0);
        }

        private Control BuildSidebar()
        {
            Panel s = new Panel();
            s.Dock = DockStyle.Left; s.Width = 250; s.BackColor = Theme.DARK;
            s.Padding = new Padding(16, 22, 16, 18);

            Stack top = new Stack();
            top.Add(Theme.Lbl("CINELUXURY", FontStyle.Bold, 24, Color.White));
            top.Add(Theme.Lbl("SYSTEM PORTAL", FontStyle.Bold, 11, Color.FromArgb(255, 120, 150)));
            top.Add(Theme.Lbl("BẢNG ĐIỀU KHIỂN", FontStyle.Bold, 11, Color.FromArgb(156, 163, 175)), 28);
            string[] names = { "Tổng quan hệ thống", "Quản lý phim", "Suất chiếu & Phòng", "Báo cáo & Doanh thu" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                Button b = new Button();
                b.Text = names[i]; b.Font = Theme.Fnt(FontStyle.Bold, 13);
                b.TextAlign = ContentAlignment.MiddleLeft; b.Padding = new Padding(10, 0, 0, 0);
                b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 70);
                b.MinimumSize = new Size(0, 42); b.Cursor = Cursors.Hand; b.TabStop = false; b.UseMnemonic = false;
                b.Click += (sender, e) => ShowPage(idx);
                nav[i] = b;
                top.Add(b, i == 0 ? 8 : 4);
            }

            Stack bottom = new Stack();
            bottom.Dock = DockStyle.Bottom;
            bottom.Add(Theme.Lbl(admin.FullName, FontStyle.Bold, 14, Color.White));
            bottom.Add(Theme.Lbl("Quản trị viên • Super Manager", FontStyle.Regular, 11, Color.FromArgb(156, 163, 175)));
            RoundButton outBtn = Theme.Primary("Đăng xuất");
            outBtn.MinimumSize = new Size(0, 40);
            outBtn.Click += (sender, e) => Logout();
            bottom.Add(outBtn, 10);

            s.Controls.Add(top);
            s.Controls.Add(bottom);
            return s;
        }

        public void ShowPage(int idx)
        {
            for (int i = 0; i < 4; i++)
            {
                nav[i].BackColor = i == idx ? Theme.RUBY : Theme.DARK;
                nav[i].ForeColor = i == idx ? Color.White : Color.FromArgb(209, 213, 219);
            }
            switch (idx)
            {
                case 0: BuildDashboard(); break;
                case 1: movies.RefreshList(); break;
                case 2: schedule.RefreshList(); break;
                default: report.RefreshView(); break;
            }
            Control page = idx == 0 ? (Control)dashboard : idx == 1 ? movies : idx == 2 ? (Control)schedule : report;
            foreach (Control c in content.Controls) c.Visible = c == page;
            page.BringToFront();
        }

        // ============================ TỔNG QUAN ============================
        private void BuildDashboard()
        {
            for (int i = dashboard.Controls.Count - 1; i >= 0; i--)
            {
                Control c = dashboard.Controls[i];
                dashboard.Controls.RemoveAt(i);
                c.Dispose();
            }
            Stack body = new Stack();

            // tiêu đề + nút
            Stack t = new Stack();
            t.Dock = DockStyle.None;
            t.Add(Theme.Lbl("ADMIN PORTAL • Hệ thống vận hành thời gian thực", FontStyle.Bold, 11, Theme.RUBY));
            t.Add(Theme.Lbl("Tổng Quan & Quản Lý Hệ Thống Rạp", FontStyle.Bold, 26, Theme.TEXT));
            t.Add(Theme.Lbl("Giám sát doanh thu phòng vé, điều phối lịch chiếu và tối ưu công suất phòng chiếu.", FontStyle.Regular, 12, Theme.MUTED));
            RoundButton b1 = Theme.Outline("Xuất báo cáo Excel (CSV)"), b2 = Theme.Outline("+ Xếp lịch chiếu"), b3 = Theme.Primary("+ Thêm phim mới");
            b1.Click += (s, e) => { ShowPage(3); report.ExportCsv(); };
            b2.Click += (s, e) => { ShowPage(2); schedule.OpenAddDialog(); };
            b3.Click += (s, e) => { ShowPage(1); movies.OpenAddDialog(); };
            body.Add(Lay.Border(t, null, Lay.Row(8, b1, b2, b3)));

            // KPI
            DateTime today = DateTime.Today;
            long rev = StatsService.Revenue(today), revY = StatsService.Revenue(today.AddDays(-1));
            int sold = StatsService.TicketsSold(today);
            long fnb = StatsService.FnbRevenue(today);
            double fill = StatsService.FillRate(today);
            string trend = revY == 0 ? "—" : ((rev - revY) * 100.0 / revY).ToString("+0.0;-0.0;0.0", Theme.Inv) + "% so với hôm qua";
            body.Add(Lay.Grid(4, 14, 0, new Control[] {
                KpiCard("TỔNG DOANH THU HÔM NAY", Theme.Money(rev), trend, rev >= revY ? Theme.GREEN : Theme.RUBY),
                KpiCard("SỐ VÉ ĐÃ BÁN", sold.ToString(), DataStore.Get().Rooms.Count + " phòng chiếu đang hoạt động", Theme.MUTED),
                KpiCard("TỶ LỆ LẤP ĐẦY PHÒNG", (fill * 100).ToString("0.0", Theme.Inv) + "%", "Tính trên các suất chiếu trong ngày", Theme.MUTED),
                KpiCard("DOANH SỐ BẮP NƯỚC", Theme.Money(fnb), sold == 0 ? "Trung bình: 0đ/vé" : "Trung bình: " + Theme.Money(fnb / sold) + "/vé", Theme.MUTED) }), 14);

            // biểu đồ + thị phần
            body.Add(Lay.Split(ChartCard(), ShareCard(), 62, 16), 16);

            dashboard.Controls.Add(Lay.Scroll(Lay.Pad(body, 28, 22, 28, 28)));
        }

        private Control KpiCard(string cap, string value, string sub, Color subColor)
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Theme.Lbl(cap, FontStyle.Bold, 11, Theme.MUTED));
            s.Add(Theme.Lbl(value, FontStyle.Bold, 24, Theme.TEXT), 4);
            s.Add(Theme.Lbl(sub, FontStyle.Bold, 12, subColor), 2);
            c.Controls.Add(s);
            return c;
        }

        private Control ChartCard()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            Stack t = new Stack();
            t.Dock = DockStyle.None;
            t.Add(Theme.Lbl("Biểu Đồ Doanh Thu Phòng Vé", FontStyle.Bold, 17, Theme.TEXT));
            t.Add(Theme.Lbl("Phân tích doanh thu thực tế theo mốc thời gian • cập nhật mỗi lần mở", FontStyle.Regular, 12, Theme.MUTED, 340));
            List<Control> tabs = new List<Control>();
            foreach (string m in new[] { "Ngày", "Tuần", "Tháng" })
            {
                string mode = m;
                RoundButton b = mode == chartMode ? Theme.Primary(mode) : Theme.Outline(mode);
                b.Click += (sender, e) => { chartMode = mode; BuildDashboard(); };
                tabs.Add(b);
            }
            s.Add(Lay.Border(t, null, Lay.Row(6, tabs.ToArray())));

            List<string> labels = new List<string>(); List<long> vals = new List<long>();
            DateTime today = DateTime.Today;
            if (chartMode == "Ngày")
            {
                foreach (StatsService.Row r in StatsService.ByDay(today.AddDays(-6), today)) { labels.Add(r.Label.Substring(0, 5)); vals.Add(r.Revenue); }
            }
            else if (chartMode == "Tuần")
            {
                List<StatsService.Row> days = StatsService.ByDay(today.AddDays(-55), today);
                for (int w = 0; w < 8; w++)
                {
                    long sum = 0;
                    for (int i = 0; i < 7; i++) sum += days[w * 7 + i].Revenue;
                    labels.Add(days[w * 7].Label.Substring(0, 5)); vals.Add(sum);
                }
            }
            else
            {
                foreach (StatsService.Row r in StatsService.ByMonth(new DateTime(today.Year, today.Month, 1).AddMonths(-5), today))
                { labels.Add(r.Label); vals.Add(r.Revenue); }
            }
            s.Add(new BarChart(labels, vals), 10);
            c.Controls.Add(s);
            return c;
        }

        private Control ShareCard()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            DateTime today = DateTime.Today;
            s.Add(Theme.Lbl("Thị Phần Doanh Thu Theo Phim", FontStyle.Bold, 17, Theme.TEXT));
            s.Add(Theme.Lbl("7 ngày gần nhất • phân bổ tỷ trọng vé bán", FontStyle.Regular, 12, Theme.MUTED));
            List<StatsService.Row> rows = StatsService.ByMovie(today.AddDays(-6), today);
            long total = rows.Sum(r => r.Revenue);
            long other = 0;
            for (int i = 0; i < rows.Count; i++)
            {
                StatsService.Row r = rows[i];
                if (i >= 4) { other += r.Revenue; continue; }
                s.Add(ShareRow(r.Label, r.Revenue, total), 14);
            }
            if (other > 0) s.Add(ShareRow("Khác", other, total), 14);
            if (rows.Count == 0) s.Add(Theme.Muted("Chưa có dữ liệu doanh thu."), 14);
            c.Controls.Add(s);
            return c;
        }

        private class ProgressLine : Control
        {
            private readonly double pct;
            public ProgressLine(double pct)
            {
                this.pct = pct;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                MinimumSize = new Size(10, 10); MaximumSize = new Size(0, 10); Size = new Size(100, 10);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(243, 244, 246)))
                using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, Width, 8), 8)) g.FillPath(b, p);
                int w = (int)(Width * pct / 100);
                if (w > 2)
                    using (SolidBrush b = new SolidBrush(Theme.RUBY))
                    using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, w, 8), 8)) g.FillPath(b, p);
            }
        }

        private Control ShareRow(string name, long v, long total)
        {
            double pct = total == 0 ? 0 : v * 100.0 / total;
            Stack p = new Stack();
            p.Dock = DockStyle.Fill;
            p.Add(Lay.Border(Theme.Lbl(name, FontStyle.Bold, 13, Theme.TEXT), null,
                    Theme.Lbl(pct.ToString("0", Theme.Inv) + "% (" + (v / 1000000.0).ToString("0.0", Theme.Inv) + "M)", FontStyle.Regular, 12, Theme.MUTED)));
            p.Add(new ProgressLine(pct), 3);
            return p;
        }

        /// <summary>Biểu đồ cột doanh thu (tự vẽ, không dùng thư viện).</summary>
        private class BarChart : Control
        {
            private readonly List<string> labels; private readonly List<long> vals;

            public BarChart(List<string> labels, List<long> vals)
            {
                this.labels = labels; this.vals = vals;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw
                        | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                MinimumSize = new Size(300, 250); MaximumSize = new Size(0, 250); Size = new Size(300, 250);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                int n = vals.Count;
                if (n == 0) return;
                long max = Math.Max(1, vals.Max());
                int left = 10, bottom = 28, top = 24, w = Width - left * 2, h = Height - bottom - top;
                int slot = w / n, bw = Math.Min(54, slot * 6 / 10);
                using (Pen grid = new Pen(Theme.BORDER))
                    for (int i = 0; i <= 4; i++) { int y = top + h - h * i / 4; g.DrawLine(grid, left, y, left + w, y); }
                using (Font fv = Theme.Fnt(FontStyle.Bold, 11), fl = Theme.Fnt(FontStyle.Regular, 11))
                using (StringFormat center = new StringFormat())
                {
                    center.Alignment = StringAlignment.Center;
                    for (int i = 0; i < n; i++)
                    {
                        long v = vals[i];
                        int bh = (int)(h * v / (double)max);
                        int x = left + i * slot + (slot - bw) / 2, y = top + h - bh;
                        bool last = i == n - 1;
                        using (SolidBrush b = new SolidBrush(last ? Theme.RUBY : Color.FromArgb(251, 182, 200)))
                        using (GraphicsPath p = Theme.Rounded(new RectangleF(x, y, bw, Math.Max(bh, 2)), 10)) g.FillPath(b, p);
                        using (SolidBrush tb = new SolidBrush(Theme.TEXT))
                            g.DrawString((v / 1000000.0).ToString("0.0", Theme.Inv) + "M", fv, tb, new RectangleF(x - 20, y - 20, bw + 40, 18), center);
                        using (SolidBrush mb = new SolidBrush(Theme.MUTED))
                            g.DrawString(labels[i] + (last ? " •" : ""), fl, mb, new RectangleF(x - 20, Height - 22, bw + 40, 18), center);
                    }
                }
            }
        }
    }
}
