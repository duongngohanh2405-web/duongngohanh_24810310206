using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Giao diện Nhân viên: xác thực vé (use case Xác thực vé) - nhập/quét mã, kiểm tra, xác nhận vào rạp.</summary>
    public class StaffForm : AppForm
    {
        private readonly User staff;
        private readonly DataStore db = DataStore.Get();

        private readonly ComboBox cbShow = new ComboBox();
        private readonly Label kTotal = Theme.Lbl("0", FontStyle.Bold, 28, Theme.TEXT), kOk = Theme.Lbl("0", FontStyle.Bold, 28, Theme.GREEN),
                kWait = Theme.Lbl("0", FontStyle.Bold, 28, Theme.AMBER), kBad = Theme.Lbl("0", FontStyle.Bold, 28, Theme.RUBY);
        private readonly Label kTitle = Theme.Lbl("", FontStyle.Bold, 12, Theme.MUTED);
        private readonly Input txtCode = Theme.Field();
        private readonly Stack resultHolder = new Stack();
        private readonly DataGridView log = Theme.Table("MÃ VÉ / BOOKING", "PHÒNG", "KHÁCH HÀNG", "GHẾ", "TRẠNG THÁI", "GIỜ SOÁT");
        private Booking current;
        private ScanResult currentResult;
        private bool currentWrongShow;

        public StaffForm(User staff) : base("CineLuxury - Nhân viên soát vé & Quầy POS", 1280, 840)
        {
            this.staff = staff;
            KeyPreview = true;

            Stack main = new Stack();
            main.Add(BuildKpi());
            main.Add(Lay.Split(BuildScanner(), resultHolder, 40, 14), 14);
            main.Add(BuildHistory(), 14);
            Controls.Add(Lay.Scroll(Lay.Pad(main, 24, 16, 24, 24)));
            Controls.Add(BuildTopBar());

            cbShow.DropDownStyle = ComboBoxStyle.DropDownList; cbShow.Font = Theme.Fnt(FontStyle.Regular, 13);
            cbShow.FormattingEnabled = true;
            cbShow.Format += (s, e) =>
            {
                Showtime st = e.ListItem as Showtime;
                if (st != null) e.Value = Fmt.Range(st.Start, st.End) + "  •  " + db.GetMovie(st.MovieId).Title + "  •  " + db.GetRoom(st.RoomId).Name;
            };
            cbShow.SelectedIndexChanged += (s, e) => RefreshKpi();

            LoadShowtimes();
            LoadInitialLog();
            ShowResult(null, ScanResult.KHONG_TON_TAI, false, true);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.F2) { txtCode.Text = ""; txtCode.Box.Focus(); e.Handled = true; }
            else if (e.KeyCode == Keys.F4) { ConfirmEntry(); e.Handled = true; }
            else if (e.KeyCode == Keys.F8) { ReportProblem(); e.Handled = true; }
        }

        // ---------------- thanh trên ----------------
        private Control BuildTopBar()
        {
            HeaderBar bar = new HeaderBar();
            bar.Padding = new Padding(24, 12, 24, 12);

            Stack l = new Stack();
            l.Dock = DockStyle.None;
            l.Add(Theme.Lbl("CineLuxury POS", FontStyle.Bold, 22, Theme.RUBY));
            l.Add(Theme.Lbl("Hệ thống trực tuyến • Máy quét hoạt động", FontStyle.Regular, 11, Theme.GREEN));

            Stack c = new Stack();
            c.Dock = DockStyle.None;
            c.Add(Theme.Lbl(staff.FullName + "  •  " + (staff.Code.Length == 0 ? "NV" : staff.Code), FontStyle.Bold, 15, Theme.TEXT));
            c.Add(Theme.Lbl("Nhân viên soát vé & Quầy POS  |  " + ShiftName() + "  |  Cổng 02 - " + Fmt.CINEMA, FontStyle.Regular, 12, Theme.MUTED));

            RoundButton handover = Theme.Outline("Bàn giao ca"), outBtn = Theme.Primary("Đăng xuất");
            handover.Click += (s, e) => Theme.Info(this, "Ca làm việc đã soát " + OkCountInShift() + " vé hợp lệ.\nVui lòng bàn giao cho nhân viên ca kế tiếp.");
            outBtn.Click += (s, e) => Logout();

            bar.Controls.Add(Lay.Border(l, Lay.Pad(c, 16, 0, 0, 0), Lay.Row(8, handover, outBtn)));
            return bar;
        }

        private string ShiftName()
        {
            int h = DateTime.Now.Hour;
            return h < 12 ? "Ca Sáng: 07:00 - 12:00" : h < 18 ? "Ca Chiều: 13:00 - 18:00" : "Ca Tối: 18:00 - 23:30";
        }

        private int OkCountInShift()
        {
            int n = 0;
            foreach (DataGridViewRow r in log.Rows)
                if (Convert.ToString(r.Cells[4].Value).StartsWith("Hợp lệ")) n++;
            return n;
        }

        // ---------------- KPI ----------------
        private Control BuildKpi()
        {
            return Lay.Grid(4, 14, 0, new Control[] {
                KpiCard(kTitle, kTotal, "vé tổng suất"),
                KpiCard(Theme.Lbl("ĐÃ SOÁT HỢP LỆ", FontStyle.Bold, 12, Theme.GREEN), kOk, "khách đã vào phòng"),
                KpiCard(Theme.Lbl("KHÁCH CHƯA VÀO", FontStyle.Bold, 12, Theme.AMBER), kWait, "vé còn lại"),
                KpiCard(Theme.Lbl("CẢNH BÁO / VÉ HỦY", FontStyle.Bold, 12, Theme.RUBY), kBad, "vé đã hủy trong suất") });
        }

        private Control KpiCard(Label cap, Label value, string sub)
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(cap); s.Add(value, 2); s.Add(Theme.Lbl(sub, FontStyle.Regular, 12, Theme.MUTED), 2);
            c.Controls.Add(s);
            return c;
        }

        private void RefreshKpi()
        {
            Showtime st = cbShow.SelectedItem as Showtime;
            if (st == null) { kTitle.Text = "CHƯA CÓ SUẤT CHIẾU HÔM NAY"; return; }
            int total = 0, ok = 0, wait = 0, bad = 0;
            foreach (Booking o in db.Orders)
            {
                if (o.ShowtimeId != st.Id) continue;
                int n = o.Seats.Count;
                if (o.Status == BookingStatus.DA_HUY) bad += n;
                else { total += n; if (o.Status == BookingStatus.DA_SOAT) ok += n; else wait += n; }
            }
            Room r = db.GetRoom(st.RoomId);
            kTitle.Text = "SUẤT " + Fmt.Time(st.Start) + " • " + r.Format.ToUpperInvariant();
            kTotal.Text = total.ToString(); kOk.Text = ok.ToString(); kWait.Text = wait.ToString(); kBad.Text = bad.ToString();
        }

        // ---------------- máy quét ----------------
        private class ScanPad : Control
        {
            public ScanPad()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw, true);
                BackColor = Color.Transparent;
                MinimumSize = new Size(0, 150); MaximumSize = new Size(0, 150);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, Width - 1, Height - 1), 16))
                using (SolidBrush b = new SolidBrush(Theme.DARK)) g.FillPath(b, p);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(200, 224, 24, 72))) g.FillRectangle(b, 24, Height / 2, Width - 48, 2);
                int s = 26, x1 = 24, x2 = Width - 24 - s, y1 = 24, y2 = Height - 24 - s;
                using (Pen pen = new Pen(Color.White))
                    foreach (Point pt in new[] { new Point(x1, y1), new Point(x2, y1), new Point(x1, y2), new Point(x2, y2) })
                        g.DrawRectangle(pen, pt.X, pt.Y, s, s);
                using (Font f = Theme.Fnt(FontStyle.Bold, 12))
                    TextRenderer.DrawText(g, "MÁY QUÉT ĐANG ĐỌC TỰ ĐỘNG • 2D / QR", f, new Point(24, Height - 24), Color.White);
            }
        }

        private Control BuildScanner()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Lay.Border(Theme.Lbl("Tra cứu & Quét vé", FontStyle.Bold, 17, Theme.TEXT), null, Theme.Badge("PHÍM F2", Theme.MUTED, Theme.BG)));
            s.Add(Theme.Lbl("Suất đang soát vé", FontStyle.Bold, 12, Theme.MUTED), 10);
            s.Add(cbShow, 4);

            s.Add(Theme.Lbl("Nhập mã vé hoặc mã đặt chỗ (Booking ID)", FontStyle.Bold, 12, Theme.MUTED), 10);
            txtCode.Box.Font = Theme.Fnt(FontStyle.Bold, 16);
            txtCode.Submit += (e, a) => DoCheck();
            RoundButton chk = Theme.Primary("Kiểm tra");
            chk.Click += (e, a) => DoCheck();
            txtCode.Margin = new Padding(0, 0, 8, 0);
            s.Add(Lay.Border(null, txtCode, chk), 4);
            s.Add(Theme.Lbl("Ví dụ: một mã check-in dạng CLX-xxxxxx hoặc mã đơn CNL-xxxxx (xem ở mục Vé của tôi của khách).",
                    FontStyle.Regular, 11, Theme.MUTED, 380), 6);
            s.Add(new ScanPad(), 14);
            s.Add(Theme.Lbl("Phím tắt thao tác nhanh quầy soát", FontStyle.Bold, 12, Theme.TEXT), 12);
            s.Add(Theme.Lbl("F2: Tìm vé tiếp  |  Enter: Kiểm tra  |  F4: Xác nhận vào rạp  |  F8: Báo lỗi / Vé trùng",
                    FontStyle.Regular, 12, Theme.MUTED, 380), 2);
            c.Controls.Add(s);
            return c;
        }

        // ---------------- kết quả kiểm tra ----------------
        private void DoCheck()
        {
            string code = txtCode.Text.Trim();
            if (code.Length == 0) { Theme.Error(this, "Vui lòng nhập hoặc quét mã vé."); return; }
            Booking o = BookingService.FindByCode(code);
            ScanResult r = BookingService.Check(o);
            Showtime sel = cbShow.SelectedItem as Showtime;
            bool wrong = r == ScanResult.HOP_LE && sel != null && o.ShowtimeId != sel.Id;
            current = o; currentResult = r; currentWrongShow = wrong;
            ShowResult(o, r, wrong, false);
            if (o == null) AddLog(code, "-", "-", "-", "Không tồn tại");
            else if (r != ScanResult.HOP_LE || wrong)
                AddLog(o.CheckInCode, RoomName(o), Customer(o), string.Join(", ", o.Seats), wrong ? "Sai suất chiếu" : LabelOf(r));
        }

        private static string LabelOf(ScanResult r)
        {
            switch (r)
            {
                case ScanResult.HOP_LE: return "Hợp lệ";
                case ScanResult.DA_SU_DUNG: return "Đã sử dụng";
                case ScanResult.DA_HUY: return "Vé đã hủy";
                case ScanResult.SAI_NGAY: return "Sai ngày chiếu";
                default: return "Không tồn tại";
            }
        }

        private string RoomName(Booking o) { return db.GetRoom(db.GetShowtime(o.ShowtimeId).RoomId).Name; }
        private string Customer(Booking o) { User u = db.GetUser(o.UserId); return u == null ? "-" : u.FullName; }

        private void ShowResult(Booking o, ScanResult r, bool wrongShow, bool idle)
        {
            resultHolder.SuspendLayout();
            resultHolder.Reset();
            RoundPanel c = Theme.Card();
            c.Padding = new Padding(0, 0, 0, 16);
            Stack s = new Stack();

            Color bg; string big, small;
            if (idle) { bg = Color.FromArgb(107, 114, 128); big = "CHỜ QUÉT VÉ"; small = "Nhập mã hoặc quét QR để xác thực"; }
            else if (r == ScanResult.KHONG_TON_TAI) { bg = Theme.RUBY; big = "KHÔNG TÌM THẤY VÉ"; small = "Mã vé không tồn tại trong hệ thống"; }
            else if (r == ScanResult.DA_HUY) { bg = Theme.RUBY; big = "VÉ ĐÃ HỦY"; small = "Từ chối vào phòng chiếu"; }
            else if (r == ScanResult.DA_SU_DUNG) { bg = Theme.AMBER; big = "VÉ ĐÃ SỬ DỤNG"; small = "Vé đã soát lúc " + (o.ScannedAt.HasValue ? Fmt.DateTimeStr(o.ScannedAt.Value) : "-") + " - nghi vé trùng"; }
            else if (r == ScanResult.SAI_NGAY) { bg = Theme.AMBER; big = "SAI NGÀY CHIẾU"; small = "Vé thuộc suất " + Fmt.DateTimeStr(db.GetShowtime(o.ShowtimeId).Start); }
            else if (wrongShow) { bg = Theme.AMBER; big = "SAI SUẤT CHIẾU"; small = "Vé thuộc suất " + Fmt.Time(db.GetShowtime(o.ShowtimeId).Start) + " - " + RoomName(o); }
            else { bg = Theme.GREEN; big = "VÉ HỢP LỆ • CHẤP THUẬN"; small = "Khách hàng được phép vào phòng chiếu số " + RoomName(o).Replace("Phòng ", ""); }

            Panel banner = new Panel();
            banner.BackColor = bg; banner.Padding = new Padding(20, 14, 20, 14);
            banner.AutoSize = true; banner.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            Stack bt = new Stack();
            bt.Dock = DockStyle.None;
            bt.Add(Theme.Lbl(big, FontStyle.Bold, 22, Color.White));
            Label sm = Theme.Lbl(small, FontStyle.Regular, 12, Theme.Mix(bg, Color.White, 0.88), 420);
            sm.BackColor = bg;
            bt.Add(sm);
            foreach (Control ch in bt.Controls) ch.BackColor = bg;
            TableLayoutPanel bb = Lay.Border(bt, null, Theme.Lbl(DateTime.Now.ToString("HH:mm:ss"), FontStyle.Bold, 16, Color.White));
            bb.Dock = DockStyle.Top;
            foreach (Control ch in bb.Controls) if (ch is Label) ch.BackColor = bg;
            banner.Controls.Add(bb);
            s.Add(banner);

            Stack details = new Stack();
            if (o != null && !idle)
            {
                Showtime st = db.GetShowtime(o.ShowtimeId);
                Movie m = db.GetMovie(st.MovieId);
                User u = db.GetUser(o.UserId);
                details.Add(Theme.Lbl(m.Age + " - " + m.Duration + " phút", FontStyle.Bold, 11, Theme.RUBY));
                details.Add(Theme.Lbl(m.Title, FontStyle.Bold, 22, Theme.TEXT, 480));
                details.Add(Lay.Grid(3, 12, 10, new Control[] {
                    Kv("PHÒNG", RoomName(o)), Kv("SUẤT CHIẾU", Fmt.Time(st.Start) + " • " + Fmt.Day(st.Start)), Kv("MÃ ĐẶT CHỖ", o.Id),
                    Kv("TÊN KHÁCH HÀNG", u == null ? "-" : u.FullName), Kv("HẠNG THÀNH VIÊN", u == null ? "-" : u.Tier()), Kv("MÃ CHECK-IN", o.CheckInCode) }), 10);

                List<Control> seatItems = new List<Control>();
                seatItems.Add(Theme.Lbl("VỊ TRÍ GHẾ (" + o.Seats.Count + " VÉ):", FontStyle.Bold, 11, Theme.MUTED));
                foreach (string seat in o.Seats) seatItems.Add(Theme.Badge(seat, Color.White, Theme.DARK));
                FlowLayoutPanel seatRow = Lay.Row(6, seatItems.ToArray());
                seatRow.WrapContents = true; seatRow.MaximumSize = new Size(500, 0);
                details.Add(seatRow, 10, false);

                StringBuilder cb = new StringBuilder();
                foreach (KeyValuePair<string, int> kv in o.Combos) cb.Append(kv.Value).Append("x ").Append(db.GetCombo(kv.Key).Name).Append("; ");
                details.Add(Theme.Lbl("F&B: " + (cb.Length == 0 ? "không có" : cb.ToString()), FontStyle.Regular, 12, Theme.TEXT, 480), 8);
            }
            else details.Add(Theme.Muted("Chưa có vé nào được kiểm tra."));
            s.Add(Lay.Pad(details, 20, 12, 20, 0));

            RoundButton btnConfirm = Theme.Primary("XÁC NHẬN VÀO RẠP  (F4)");
            btnConfirm.SetColors(Theme.GREEN, Color.White, Color.Empty);
            btnConfirm.MinimumSize = new Size(0, 42);
            btnConfirm.Enabled = o != null && !idle && r == ScanResult.HOP_LE && !wrongShow;
            btnConfirm.Click += (e, a) => ConfirmEntry();
            RoundButton err = Theme.Outline("Báo lỗi / Trùng  (F8)");
            err.MinimumSize = new Size(0, 42);
            err.Enabled = o != null && !idle;
            err.Click += (e, a) => ReportProblem();
            s.Add(Lay.Pad(Lay.Grid(2, 10, 0, new Control[] { btnConfirm, err }), 20, 12, 20, 0));

            c.Controls.Add(s);
            resultHolder.Add(c);
            resultHolder.ResumeLayout(true);
        }

        private Control Kv(string k, string v)
        {
            Stack p = new Stack();
            p.Dock = DockStyle.Fill;
            p.Add(Theme.Lbl(k, FontStyle.Bold, 10, Theme.MUTED));
            p.Add(Theme.Lbl(v, FontStyle.Bold, 14, Theme.TEXT, 150));
            return p;
        }

        private void ConfirmEntry()
        {
            if (current == null || currentResult != ScanResult.HOP_LE || currentWrongShow) return;
            Booking o = current;
            BookingService.ConfirmEntry(o, staff);
            AddLog(o.CheckInCode, RoomName(o), Customer(o), string.Join(", ", o.Seats), "Hợp lệ (Đã vào)");
            Theme.Info(this, "Đã xác nhận cho khách vào rạp: " + Customer(o) + " - ghế " + string.Join(", ", o.Seats));
            current = null; currentResult = ScanResult.KHONG_TON_TAI;
            txtCode.Text = ""; ShowResult(null, ScanResult.KHONG_TON_TAI, false, true); RefreshKpi(); txtCode.Box.Focus();
        }

        private void ReportProblem()
        {
            if (current == null) return;
            string reason = Dialogs.Prompt(this, "Báo lỗi vé " + current.CheckInCode, "Lý do báo lỗi / vé trùng:", "");
            if (reason == null || reason.Trim().Length == 0) return;
            AddLog(current.CheckInCode, RoomName(current), Customer(current), string.Join(", ", current.Seats), "Báo lỗi: " + reason.Trim());
            Theme.Info(this, "Đã ghi nhận sự cố, vui lòng liên hệ quản lý ca.");
        }

        // ---------------- lịch sử ----------------
        private Control BuildHistory()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Lay.Border(Theme.Lbl("Lịch sử lượt quét vé gần nhất trong ca", FontStyle.Bold, 17, Theme.TEXT), null,
                    Theme.Lbl("Cập nhật theo thời gian thực", FontStyle.Regular, 12, Theme.GREEN)));
            log.MinimumSize = new Size(0, 220); log.MaximumSize = new Size(0, 220);
            s.Add(log, 10);
            c.Controls.Add(s);
            return c;
        }

        private void AddLog(string code, string room, string cust, string seats, string status)
        {
            log.Rows.Insert(0, code, room, cust, seats, status, DateTime.Now.ToString("HH:mm:ss"));
            while (log.Rows.Count > 30) log.Rows.RemoveAt(log.Rows.Count - 1);
            Theme.ClearSel(log);
        }

        private void LoadInitialLog()
        {
            List<Booking> done = db.Orders
                    .Where(o => o.Status == BookingStatus.DA_SOAT && o.ScannedAt.HasValue && o.ScannedAt.Value.Date == DateTime.Today)
                    .OrderBy(o => o.ScannedAt.Value).ToList();
            foreach (Booking o in done)
                log.Rows.Insert(0, o.CheckInCode, RoomName(o), Customer(o), string.Join(", ", o.Seats), "Hợp lệ (Đã vào)",
                        o.ScannedAt.Value.ToString("HH:mm:ss"));
            Theme.ClearSel(log);
        }

        private void LoadShowtimes()
        {
            List<Showtime> today = ScheduleService.ForDay(DateTime.Today);
            Showtime pick = null;
            foreach (Showtime st in today) { cbShow.Items.Add(st); if (pick == null && st.End > DateTime.Now) pick = st; }
            if (pick != null) cbShow.SelectedItem = pick;
            else if (cbShow.Items.Count > 0) cbShow.SelectedIndex = 0;
            RefreshKpi();
        }
    }
}
