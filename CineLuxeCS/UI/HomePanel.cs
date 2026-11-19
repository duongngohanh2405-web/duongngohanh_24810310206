using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Màn hình "Trang chủ CineLuxe Web": hero, phim đang chiếu (có bộ lọc + tìm kiếm), ưu đãi, phim sắp chiếu.</summary>
    public class HomePanel : Panel
    {
        private readonly CustomerForm frame;
        private readonly Stack body = new Stack();
        private string query = "";
        private string filter = "Tất Cả";

        public HomePanel(CustomerForm frame)
        {
            this.frame = frame;
            Dock = DockStyle.Fill; BackColor = Theme.BG;
            Controls.Add(Lay.Scroll(Lay.Pad(body, 32, 20, 32, 32)));
            Rebuild();
        }

        public void SetQuery(string q)
        {
            query = q == null ? "" : q.Trim().ToLowerInvariant();
            Rebuild();
        }

        private bool MatchesQuery(Movie m)
        {
            return query.Length == 0 || m.Title.ToLowerInvariant().Contains(query) || m.Genre.ToLowerInvariant().Contains(query)
                    || m.Director.ToLowerInvariant().Contains(query) || m.Id.ToLowerInvariant().Contains(query);
        }

        private bool MatchesFilter(Movie m)
        {
            switch (filter)
            {
                case "IMAX Laser": return m.Formats.Contains("IMAX");
                case "VIP Lounge": return ScheduleService.UpcomingOf(m.Id).Any(s => s.RoomId == "R4");
                case "Phim Việt Nam": return m.Vietnamese;
                default: return true;
            }
        }

        private void Rebuild()
        {
            body.SuspendLayout();
            body.Reset();
            DataStore d = DataStore.Get();
            List<Movie> showing = new List<Movie>(), soon = new List<Movie>();
            foreach (Movie m in d.Movies)
            {
                if (!MatchesQuery(m)) continue;
                if (m.Status == MovieStatus.DANG_CHIEU && MatchesFilter(m)) showing.Add(m);
                else if (m.Status == MovieStatus.SAP_CHIEU && filter == "Tất Cả") soon.Add(m);
            }
            Movie featured = d.Movies.FirstOrDefault(m => m.Status == MovieStatus.DANG_CHIEU);
            if (query.Length == 0 && featured != null) body.Add(Hero(featured), 0);
            else if (query.Length > 0)
                body.Add(Theme.Lbl("Kết quả tìm kiếm cho \"" + query + "\": " + (showing.Count + soon.Count) + " phim", FontStyle.Bold, 16, Theme.TEXT));

            body.Add(SectionHeader("ĐANG CÓ MẶT TẠI RẠP", "Phim Đang Chiếu", true), featured != null && query.Length == 0 ? 26 : 12);
            body.Add(MovieGrid(showing, false), 12);

            if (query.Length == 0)
            {
                body.Add(SectionHeader("ĐẶC QUYỀN ĐIỆN ẢNH", "Ưu Đãi Độc Quyền CineLuxury", false), 28);
                body.Add(Promos(), 12);
            }
            if (soon.Count > 0)
            {
                body.Add(SectionHeader("CHỜ ĐỢI BOM TẤN", "Phim Sắp Chiếu", false), 28);
                body.Add(MovieGrid(soon, true), 12);
            }
            body.ResumeLayout(true);
        }

        // ---------------- hero ----------------
        private Control Hero(Movie m)
        {
            GradCard p = new GradCard(Color.FromArgb(255, 214, 160), Color.FromArgb(190, 90, 60));
            p.Arc = 28; p.WhiteFade = true;
            p.MinimumSize = new Size(0, 330); p.MaximumSize = new Size(0, 330);

            Stack box = new Stack();
            box.Dock = DockStyle.None; box.Location = new Point(36, 24);
            box.MinimumSize = new Size(620, 0); box.MaximumSize = new Size(620, 0);
            box.Add(Theme.Badge("TÂM ĐIỂM PHÒNG VÉ TOÀN CẦU", Theme.RUBY, Theme.RUBY_SOFT), 0, false);
            FlowLayoutPanel badges = Lay.Row(6,
                    Theme.Badge("IMDb " + m.Imdb.ToString("0.0", Theme.Inv), Theme.AMBER, Theme.AMBER_SOFT),
                    Theme.Badge(m.Age + " - Dành cho khán giả từ " + m.Age.Substring(1) + " tuổi", Theme.RUBY, Theme.RUBY_SOFT),
                    Theme.Badge("IMAX LASER 2D", Color.White, Theme.DARK),
                    Theme.Badge("DOLBY ATMOS", Color.White, Theme.DARK));
            box.Add(badges, 10, false);
            box.Add(Theme.Lbl(m.Title.ToUpperInvariant(), FontStyle.Bold, 38, Theme.TEXT, 620), 8);
            box.Add(Theme.Lbl(m.Description, FontStyle.Regular, 13, Theme.MUTED, 470), 2);
            box.Add(Theme.Lbl(m.Duration + " Phút  •  " + m.Genre + "  •  Phụ đề Tiếng Việt", FontStyle.Bold, 13, Theme.TEXT), 8);
            RoundButton b1 = Theme.Primary("Đặt Vé Nhanh"), b2 = Theme.Outline("Xem Trailer");
            b1.Click += (s, e) => frame.OpenBooking(m);
            b2.Click += (s, e) => Theme.Info(this, "Trailer \"" + m.Title + "\" sẽ được cập nhật sau.");
            box.Add(Lay.Row(10, b1, b2), 16, false);
            p.Controls.Add(box);
            return p;
        }

        private Control SectionHeader(string kicker, string title, bool withTabs)
        {
            Stack left = new Stack();
            left.Dock = DockStyle.None;
            left.Add(Theme.Lbl(kicker, FontStyle.Bold, 11, Theme.RUBY));
            left.Add(Theme.Lbl(title, FontStyle.Bold, 24, Theme.TEXT));
            Control right = null;
            if (withTabs)
            {
                List<Control> tabs = new List<Control>();
                foreach (string f in new[] { "Tất Cả", "IMAX Laser", "VIP Lounge", "Phim Việt Nam" })
                {
                    string name = f;
                    RoundButton b = name == filter ? Theme.Primary(name) : Theme.Outline(name);
                    b.Click += (s, e) => { filter = name; Rebuild(); };
                    tabs.Add(b);
                }
                right = Lay.Row(6, tabs.ToArray());
            }
            return Lay.Border(left, null, right);
        }

        // ---------------- lưới phim ----------------
        private Control MovieGrid(List<Movie> movies, bool soon)
        {
            if (movies.Count == 0) return Theme.Muted("Không có phim phù hợp.");
            List<Control> cards = new List<Control>();
            foreach (Movie m in movies) cards.Add(MovieCard(m, soon));
            return Lay.Grid(4, 16, 16, cards);
        }

        private Control MovieCard(Movie m, bool soon)
        {
            RoundPanel c = Theme.Card();
            c.Padding = new Padding(10, 10, 10, 14);
            Stack s = new Stack();
            s.Add(new Poster(m.Id, m.Title, null, new Size(200, 270), true));
            s.Add(Lay.Row(4, Theme.Badge(m.Age, Theme.RUBY, Theme.RUBY_SOFT),
                    Theme.Badge("IMDb " + m.Imdb.ToString("0.0", Theme.Inv), Theme.AMBER, Theme.AMBER_SOFT)), 10, false);
            s.Add(Theme.Lbl(m.Title, FontStyle.Bold, 14, Theme.TEXT, 250), 6);
            s.Add(Theme.Lbl(m.Genre + " • " + m.Duration + "'", FontStyle.Regular, 12, Theme.MUTED, 250), 2);

            if (soon)
            {
                int days = Math.Max(0, (int)(m.ReleaseDate.Date - DateTime.Today).TotalDays);
                s.Add(Theme.Lbl("Khởi chiếu sau: " + days + " ngày", FontStyle.Bold, 12, Theme.RUBY), 10);
                RoundButton b = Theme.Outline("Nhận Thông Báo");
                b.Click += (sender, e) => Theme.Info(this, "Đã đăng ký nhận thông báo khi \"" + m.Title + "\" mở bán vé.");
                s.Add(b, 6);
            }
            else
            {
                string pills = string.Join("  ", ScheduleService.UpcomingOf(m.Id).Take(4).Select(st => Fmt.Time(st.Start)));
                s.Add(Theme.Lbl(pills.Length == 0 ? "Chưa có suất chiếu" : pills, FontStyle.Regular, 12, Theme.MUTED, 250), 10);
                RoundButton b = Theme.Primary("Đặt Vé Ngay");
                b.Click += (sender, e) => frame.OpenBooking(m);
                s.Add(b, 6);
            }
            c.Controls.Add(s);
            return c;
        }

        // ---------------- ưu đãi ----------------
        private Control Promos()
        {
            List<Control> l = new List<Control>();
            l.Add(Promo("RUBY PRIVILEGE", "Nâng Tầm Trải Nghiệm Cùng Thẻ VIP Ruby",
                    "Tặng ngay 02 vé xem phim IMAX Laser, Welcome Drink tại sảnh chờ và tích lũy 10% điểm thưởng cho mỗi đơn bắp nước.",
                    "Đăng Ký Thành Viên", Color.FromArgb(120, 10, 40), Color.FromArgb(224, 24, 72)));
            l.Add(Promo("CINELUXE DAY", "Thứ Tư Vui Vẻ • Đồng Giá Vé 75.000đ",
                    "Đồng giá tất cả suất chiếu 2D tiêu chuẩn mọi khung giờ vào mỗi thứ 4. Tặng voucher giảm 30% combo bắp caramel & nước ngọt.",
                    "Săn Vé Thứ 4 Ngay", Color.FromArgb(30, 41, 90), Color.FromArgb(99, 102, 241)));
            return Lay.Grid(2, 16, 0, l);
        }

        private Control Promo(string kicker, string title, string desc, string btn, Color c1, Color c2)
        {
            GradCard p = new GradCard(c1, c2);
            p.Arc = 22; p.Padding = new Padding(22, 18, 22, 18);
            p.MinimumSize = new Size(0, 190);
            Stack s = new Stack();
            s.Add(Theme.Badge(kicker, Color.White, Theme.Mix(c1, Color.White, 0.25)), 0, false);
            s.Add(Theme.Lbl(title, FontStyle.Bold, 18, Color.White, 400), 8);
            s.Add(Theme.Lbl(desc, FontStyle.Regular, 12, Theme.Mix(c2, Color.White, 0.85), 400), 4);
            RoundButton b = new RoundButton(btn, Color.White, Theme.TEXT, Color.Empty);
            b.Click += (sender, e) => Theme.Info(this, "Chương trình \"" + title + "\" áp dụng khi đặt vé trên CineLuxury.");
            s.Add(b, 10, false);
            p.Controls.Add(s);
            return p;
        }
    }
}
