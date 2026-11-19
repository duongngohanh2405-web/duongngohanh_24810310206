using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Màn hình "Vé của tôi &amp; Lịch sử giao dịch" (use case Quản lý đơn hàng: xem, lọc, hủy vé).</summary>
    public class MyTicketsPanel : Panel
    {
        private static readonly string[] TABS = { "Tất cả đơn hàng", "Vé sắp xem", "Lịch sử vé đã xem", "Đã hủy" };
        private readonly CustomerForm frame;
        private readonly User user;
        private readonly DataStore db = DataStore.Get();
        private readonly Stack body = new Stack();
        private int tab = 0;

        public MyTicketsPanel(CustomerForm frame, User user)
        {
            this.frame = frame; this.user = user;
            Dock = DockStyle.Fill; BackColor = Theme.BG;
            Controls.Add(Lay.Scroll(Lay.Pad(body, 32, 20, 32, 32)));
        }

        private bool IsUpcoming(Booking o)
        {
            return o.Status == BookingStatus.DA_THANH_TOAN && db.GetShowtime(o.ShowtimeId).End > DateTime.Now;
        }

        private bool InTab(Booking o)
        {
            switch (tab)
            {
                case 1: return IsUpcoming(o);
                case 2: return o.Status == BookingStatus.DA_SOAT || (o.Status == BookingStatus.DA_THANH_TOAN && !IsUpcoming(o));
                case 3: return o.Status == BookingStatus.DA_HUY;
                default: return true;
            }
        }

        public void RefreshList()
        {
            body.SuspendLayout();
            body.Reset();
            List<Booking> all = BookingService.OrdersOf(user);

            Stack t = new Stack();
            t.Dock = DockStyle.None;
            t.Add(Theme.Lbl("Tài khoản  /  Vé của tôi", FontStyle.Regular, 12, Theme.MUTED));
            t.Add(Theme.Lbl("Vé của tôi & Lịch sử giao dịch", FontStyle.Bold, 26, Theme.TEXT));
            RoundButton rf = Theme.Outline("Làm mới");
            rf.Click += (s, e) => RefreshList();
            body.Add(Lay.Border(t, null, rf));

            List<Control> tabs = new List<Control>();
            for (int i = 0; i < TABS.Length; i++)
            {
                int idx = i;
                RoundButton b = i == tab ? Theme.Primary(TABS[i]) : Theme.Outline(TABS[i]);
                b.Click += (s, e) => { tab = idx; RefreshList(); };
                tabs.Add(b);
            }
            body.Add(Lay.Row(8, tabs.ToArray()), 14, false);

            // vé nổi bật: suất sắp chiếu gần nhất
            Booking next = null;
            if (tab <= 1)
                foreach (Booking o in all)
                    if (IsUpcoming(o) && (next == null || db.GetShowtime(o.ShowtimeId).Start < db.GetShowtime(next.ShowtimeId).Start)) next = o;
            if (next != null) body.Add(Highlight(next), 14);

            body.Add(Theme.Lbl(tab == 3 ? "Đơn vé đã hủy" : "Danh sách đơn hàng", FontStyle.Bold, 18, Theme.TEXT), next != null ? 22 : 14);
            int n = 0;
            foreach (Booking o in all)
            {
                if (!InTab(o) || (o == next && tab == 1)) continue;
                body.Add(OrderRow(o), 10); n++;
            }
            if (n == 0) body.Add(Theme.Muted("Chưa có đơn hàng nào trong mục này."), 10);
            body.ResumeLayout(true);
        }

        // ---------------- vé nổi bật ----------------
        private Control Highlight(Booking o)
        {
            Showtime st = db.GetShowtime(o.ShowtimeId);
            Movie m = db.GetMovie(st.MovieId);
            Room room = db.GetRoom(st.RoomId);

            RoundPanel card = new RoundPanel(22, Color.White, Theme.BORDER);
            card.Padding = new Padding(1);
            Stack s = new Stack();

            long mins = BookingService.MinutesUntil(st);
            string when;
            if (mins < 0) when = "ĐANG CHIẾU";
            else if (st.Start.Date == DateTime.Today) when = "SUẤT CHIẾU HÔM NAY • Sẽ bắt đầu sau " + (mins / 60) + " giờ " + (mins % 60) + " phút";
            else when = "SUẤT CHIẾU SẮP TỚI • " + Fmt.DateTimeStr(st.Start);
            Panel ribbon = new Panel();
            ribbon.BackColor = Theme.RUBY; ribbon.Padding = new Padding(18, 8, 18, 8);
            ribbon.AutoSize = true; ribbon.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableLayoutPanel rb = Lay.Border(Theme.Lbl(when, FontStyle.Bold, 12, Color.White), null,
                    Theme.Lbl("Mã đặt vé: " + o.Id, FontStyle.Bold, 12, Color.White));
            rb.Dock = DockStyle.Top;
            ribbon.Controls.Add(rb);
            s.Add(ribbon);

            // thông tin chính
            Poster poster = new Poster(m.Id, m.Title, null, new Size(130, 190));
            Stack info = new Stack();
            info.Dock = DockStyle.None;
            info.Add(Theme.Lbl(m.Genre.ToUpperInvariant() + " • " + m.Duration + " PHÚT", FontStyle.Bold, 11, Theme.RUBY));
            info.Add(Theme.Lbl(m.Title, FontStyle.Bold, 24, Theme.TEXT, 460));
            info.Add(Theme.Lbl("Phụ đề Tiếng Việt • " + room.Format, FontStyle.Regular, 12, Theme.MUTED));
            Control[] specs = {
                Spec("THỜI GIAN", Fmt.Day(st.Start) + ", " + Fmt.Range(st.Start, st.End)),
                Spec("RẠP CHIẾU", Fmt.CINEMA + " • " + room.Name),
                Spec("GHẾ NGỒI", string.Join(", ", o.Seats)) };
            info.Add(Lay.Grid(3, 12, 0, specs), 10);
            info.Add(Theme.Lbl("Combo đã mua kèm: " + ComboText(o), FontStyle.Regular, 12, Theme.TEXT, 460), 10);
            info.Add(Theme.Badge("Đã thanh toán • " + Theme.Money(o.Total) + " • " + o.Payment, Theme.GREEN, Theme.GREEN_SOFT), 6, false);

            Stack qr = new Stack();
            qr.Dock = DockStyle.None;
            qr.Add(Theme.Lbl("MÃ CHECK-IN KHÔNG CHẠM", FontStyle.Bold, 11, Theme.MUTED));
            qr.Add(new QrMock(o.CheckInCode), 4, false);
            qr.Add(Theme.Lbl(o.CheckInCode, FontStyle.Bold, 16, Theme.TEXT), 4);
            qr.Add(Theme.Lbl("Đưa mã này cho nhân viên\nsoát vé tại cửa phòng chiếu.", FontStyle.Regular, 11, Theme.MUTED));

            TableLayoutPanel main = Lay.Border(poster, Lay.Pad(info, 18, 0, 18, 0), qr);
            Panel mainWrap = Lay.Pad(main, 18, 16, 18, 10);
            s.Add(mainWrap);

            RoundButton save = Theme.Outline("Lưu vé (.txt)"), mail = Theme.Outline("Gửi Email"), cancel = Theme.Outline("Đổi / Hủy");
            save.Click += (e, a) => SaveTicket(o);
            mail.Click += (e, a) => Theme.Info(this, "Đã gửi vé " + o.Id + " tới " + user.Email + ".");
            cancel.Click += (e, a) => CancelOrder(o);
            s.Add(Lay.Pad(Lay.Row(8, save, mail, cancel), 18, 0, 18, 14), 0);
            card.Controls.Add(s);
            return card;
        }

        private Control Spec(string cap, string val)
        {
            Stack p = new Stack();
            p.Dock = DockStyle.Fill;
            p.Add(Theme.Lbl(cap, FontStyle.Bold, 10, Theme.MUTED));
            p.Add(Theme.Lbl(val, FontStyle.Bold, 13, Theme.TEXT, 170));
            return p;
        }

        private string ComboText(Booking o)
        {
            if (o.Combos.Count == 0) return "không có";
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, int> kv in o.Combos) sb.Append(kv.Value).Append("x ").Append(db.GetCombo(kv.Key).Name).Append("; ");
            return sb.ToString();
        }

        // ---------------- danh sách đơn ----------------
        private Control OrderRow(Booking o)
        {
            Showtime st = db.GetShowtime(o.ShowtimeId);
            Movie m = db.GetMovie(st.MovieId);
            RoundPanel row = Theme.Card();

            bool expired = o.Status == BookingStatus.DA_THANH_TOAN && !IsUpcoming(o);
            Label badge;
            if (o.Status == BookingStatus.DA_HUY) badge = Theme.Badge("Đã hủy", Theme.RUBY, Theme.RUBY_SOFT);
            else if (o.Status == BookingStatus.DA_SOAT) badge = Theme.Badge("Đã hoàn thành", Theme.GREEN, Theme.GREEN_SOFT);
            else if (expired) badge = Theme.Badge("Chưa sử dụng", Theme.AMBER, Theme.AMBER_SOFT);
            else badge = Theme.Badge("Sắp xem", Theme.GREEN, Theme.GREEN_SOFT);
            Panel left = new Panel();
            left.Size = new Size(120, 26); left.BackColor = Color.Transparent;
            badge.Location = new Point(0, 0);
            left.Controls.Add(badge);

            Stack mid = new Stack();
            mid.Dock = DockStyle.None;
            mid.Add(Theme.Lbl(m.Title, FontStyle.Bold, 15, Theme.TEXT));
            mid.Add(Theme.Lbl(Fmt.CINEMA + " • " + db.GetRoom(st.RoomId).Name + " • Ghế " + string.Join(", ", o.Seats)
                    + " • " + Fmt.DateTimeStr(st.Start), FontStyle.Regular, 12, Theme.MUTED, 520), 2);
            mid.Add(Theme.Lbl("Mã đơn: " + o.Id + " • " + Theme.Money(o.Total) + " • " + o.Payment, FontStyle.Regular, 12, Theme.MUTED), 2);

            Control right = null;
            if (IsUpcoming(o))
            {
                RoundButton c = Theme.Outline("Hủy vé");
                c.Click += (e, a) => CancelOrder(o);
                right = c;
            }
            else if (o.Status == BookingStatus.DA_SOAT)
            {
                RoundButton rate = Theme.Outline("Đánh giá"), again = Theme.Primary("Đặt lại");
                rate.Click += (e, a) =>
                {
                    string v = Dialogs.Choose(this, "Đánh giá", "Chấm điểm phim \"" + m.Title + "\":",
                            new[] { "5 sao", "4 sao", "3 sao", "2 sao", "1 sao" });
                    if (v != null) Theme.Info(this, "Cảm ơn bạn đã đánh giá " + v + "!");
                };
                again.Click += (e, a) => frame.OpenBooking(m);
                right = Lay.Row(8, rate, again);
            }
            row.Controls.Add(Lay.Border(left, mid, right));
            return row;
        }

        private void CancelOrder(Booking o)
        {
            if (!BookingService.CanCancel(o))
            {
                Theme.Error(this, "Chỉ được hủy vé chưa sử dụng, trước giờ chiếu tối thiểu " + BookingService.CancelBeforeHours + " tiếng.");
                return;
            }
            if (!Theme.Confirm(this, "Bạn chắc chắn muốn hủy đơn " + o.Id + "?\nĐiểm đã dùng sẽ được hoàn lại.")) return;
            try
            {
                BookingService.Cancel(o);
                frame.RefreshUser();
                Theme.Info(this, "Đã hủy đơn " + o.Id + ". Tiền sẽ được hoàn về " + o.Payment + " trong 3-5 ngày làm việc.");
                RefreshList();
            }
            catch (InvalidOperationException ex) { Theme.Error(this, ex.Message); }
        }

        private void SaveTicket(Booking o)
        {
            Showtime st = db.GetShowtime(o.ShowtimeId);
            using (SaveFileDialog fc = new SaveFileDialog())
            {
                fc.FileName = "ve_" + o.Id + ".txt";
                fc.Filter = "Văn bản (*.txt)|*.txt";
                if (fc.ShowDialog(this) != DialogResult.OK) return;
                string txt = "CINELUXE - VÉ ĐIỆN TỬ\r\n----------------------\r\nMã đặt vé: " + o.Id + "\r\nMã check-in: " + o.CheckInCode
                        + "\r\nPhim: " + db.GetMovie(st.MovieId).Title + "\r\nRạp: " + Fmt.CINEMA + " - " + db.GetRoom(st.RoomId).Name
                        + "\r\nSuất chiếu: " + Fmt.DateTimeStr(st.Start) + "\r\nGhế: " + string.Join(", ", o.Seats)
                        + "\r\nTổng thanh toán: " + Theme.Money(o.Total) + "\r\n";
                try { File.WriteAllText(fc.FileName, txt, new UTF8Encoding(false)); Theme.Info(this, "Đã lưu vé."); }
                catch (IOException ex) { Theme.Error(this, "Không lưu được file: " + ex.Message); }
            }
        }

        /// <summary>Hình QR mô phỏng sinh từ mã check-in (chỉ để minh họa giao diện).</summary>
        private class QrMock : Control
        {
            private readonly bool[,] m = new bool[25, 25];

            public QrMock(string code)
            {
                Random r = new Random(Theme.StableHash(code));
                for (int i = 0; i < 25; i++) for (int j = 0; j < 25; j++) m[i, j] = r.Next(100) < 48;
                int[][] corners = { new[] { 0, 0 }, new[] { 0, 18 }, new[] { 18, 0 } };
                foreach (int[] c in corners)
                    for (int i = 0; i < 7; i++)
                        for (int j = 0; j < 7; j++)
                        {
                            bool edge = i == 0 || j == 0 || i == 6 || j == 6, core = i >= 2 && i <= 4 && j >= 2 && j <= 4;
                            m[c[0] + i, c[1] + j] = edge || core;
                        }
                Size = new Size(140, 140); MinimumSize = Size; MaximumSize = Size;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(Color.White);
                int s = Math.Min(Width, Height) / 25;
                using (SolidBrush b = new SolidBrush(Theme.DARK))
                    for (int i = 0; i < 25; i++)
                        for (int j = 0; j < 25; j++)
                            if (m[i, j]) e.Graphics.FillRectangle(b, j * s, i * s, s, s);
            }
        }
    }
}
