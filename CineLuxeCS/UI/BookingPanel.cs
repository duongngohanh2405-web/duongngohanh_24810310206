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
    /// <summary>Màn hình "Đặt vé, Chọn ghế &amp; Bắp nước" + thanh toán (use case Quản lý đặt vé).</summary>
    public class BookingPanel : Panel
    {
        private readonly CustomerForm frame;
        private readonly User user;
        private readonly DataStore db = DataStore.Get();

        private readonly ComboBox cbMovie = new ComboBox(), cbShow = new ComboBox();
        private readonly Label lblInfo = Theme.Lbl("", FontStyle.Regular, 13, Theme.MUTED, 740);
        private readonly Stack seatArea = new Stack();
        private readonly SortedSet<string> selected = new SortedSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, NumericUpDown> comboSpin = new Dictionary<string, NumericUpDown>();
        private readonly ToolTip tip = new ToolTip();

        // tóm tắt đơn hàng
        private readonly Label lblSeats = Theme.Lbl("Chưa chọn ghế", FontStyle.Bold, 14, Theme.TEXT, 380);
        private readonly Label lblTicket = Theme.Lbl("0đ", FontStyle.Bold, 13, Theme.TEXT);
        private readonly Label lblCombo = Theme.Lbl("0đ", FontStyle.Bold, 13, Theme.TEXT);
        private readonly Label lblDiscount = Theme.Lbl("0đ", FontStyle.Bold, 13, Theme.TEXT);
        private readonly Label lblTotal = Theme.Lbl("0đ", FontStyle.Bold, 22, Theme.RUBY);
        private readonly Label lblEarn = Theme.Lbl("", FontStyle.Regular, 12, Theme.GREEN);
        private readonly Input txtVoucher = Theme.Field();
        private readonly CheckBox chkPoints = new CheckBox();
        private readonly RadioButton[] pay = { new RadioButton(), new RadioButton(), new RadioButton() };
        private string appliedVoucher = "";
        private bool loading;

        public BookingPanel(CustomerForm frame, User user)
        {
            this.frame = frame; this.user = user;
            Dock = DockStyle.Fill; BackColor = Theme.BG;

            // cột trái (cuộn) - thêm trước để dock Fill sau cùng
            Stack left = new Stack();
            left.Add(BuildSummaryCard());
            left.Add(BuildSeatCard(), 14);
            left.Add(BuildComboCard(), 14);
            Panel lsp = Lay.Scroll(Lay.Pad(left, 24, 16, 12, 24));
            Controls.Add(lsp);

            // cột phải (cố định 430px)
            Panel rsp = Lay.Scroll(Lay.Pad(BuildOrderCard(), 12, 16, 24, 24));
            rsp.Dock = DockStyle.Right; rsp.Width = 430;
            Controls.Add(rsp);

            Controls.Add(BuildStepper());

            cbMovie.SelectedIndexChanged += (s, e) => { if (!loading) ReloadShowtimes(); };
            cbShow.SelectedIndexChanged += (s, e) => { if (!loading) ReloadSeats(); };
            cbShow.FormattingEnabled = true;
            cbShow.Format += (s, e) =>
            {
                Showtime st = e.ListItem as Showtime;
                if (st != null)
                {
                    Room r = db.GetRoom(st.RoomId);
                    e.Value = Fmt.Day(st.Start) + "   " + Fmt.Range(st.Start, st.End) + "   •   " + r.Name + " (" + r.Format + ")";
                }
            };
        }

        // ---------------- stepper ----------------
        private Control BuildStepper()
        {
            HeaderBar p = new HeaderBar();
            p.Padding = new Padding(24, 10, 24, 10);
            Button back = Theme.NavButton("←  Quay lại chọn phim");
            back.Click += (s, e) => frame.ShowHome();
            FlowLayoutPanel steps = Lay.Row(16,
                    Theme.Lbl("① Chọn phim & Rạp", FontStyle.Bold, 13, Theme.GREEN),
                    Theme.Lbl("—", FontStyle.Regular, 13, Theme.BORDER),
                    Theme.Lbl("② Chọn ghế & Combo", FontStyle.Bold, 13, Theme.RUBY),
                    Theme.Lbl("—", FontStyle.Regular, 13, Theme.BORDER),
                    Theme.Lbl("③ Thanh toán vé", FontStyle.Bold, 13, Theme.MUTED));
            TableLayoutPanel b = Lay.Border(back, steps, null);
            steps.Dock = DockStyle.None; steps.Anchor = AnchorStyles.None;
            p.Controls.Add(b);
            return p;
        }

        private Control BuildSummaryCard()
        {
            RoundPanel c = Theme.Card();
            cbMovie.DropDownStyle = ComboBoxStyle.DropDownList; cbMovie.Font = Theme.Fnt(FontStyle.Regular, 13);
            cbShow.DropDownStyle = ComboBoxStyle.DropDownList; cbShow.Font = Theme.Fnt(FontStyle.Regular, 13);
            Stack s = new Stack();
            s.Add(Lay.Grid(2, 12, 0, new Control[] {
                Theme.Lbl("Phim", FontStyle.Bold, 12, Theme.MUTED), Theme.Lbl("Suất chiếu", FontStyle.Bold, 12, Theme.MUTED),
                cbMovie, cbShow }));
            s.Add(lblInfo, 8);
            c.Controls.Add(s);
            return c;
        }

        private class Swatch : Control
        {
            private readonly Color fill, line;
            public Swatch(Color fill, Color line) { this.fill = fill; this.line = line; Size = new Size(16, 16); Margin = Padding.Empty; }
            protected override void OnPaint(PaintEventArgs e)
            {
                using (SolidBrush b = new SolidBrush(fill)) e.Graphics.FillRectangle(b, 0, 0, Width, Height);
                using (Pen p = new Pen(line)) e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
        }

        private Control LegendItem(string text, Color fill, Color line)
        {
            return Lay.Row(5, new Swatch(fill, line), Theme.Lbl(text, FontStyle.Regular, 12, Theme.MUTED));
        }

        private Control BuildSeatCard()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            Panel bar = new Panel(); bar.Height = 4; bar.BackColor = Theme.RUBY; bar.MinimumSize = new Size(0, 4);
            s.Add(bar);
            Label screen = Theme.Lbl("MÀN HÌNH CHIẾU / SCREEN", FontStyle.Bold, 12, Theme.MUTED);
            screen.TextAlign = ContentAlignment.MiddleCenter; screen.Padding = new Padding(0, 8, 0, 8);
            s.Add(screen);
            s.Add(seatArea, 4);

            FlowLayoutPanel legend = Lay.Row(14,
                    LegendItem("Ghế Thường (" + Theme.Money(SeatMap.PriceStandard) + ")", Color.White, Theme.BORDER),
                    LegendItem("Ghế VIP (" + Theme.Money(SeatMap.PriceVip) + ")", Theme.AMBER_SOFT, Theme.AMBER),
                    LegendItem("Sweetbox Đôi (" + Theme.Money(SeatMap.PriceSweetbox) + ")", Theme.RUBY_SOFT, Theme.RUBY),
                    LegendItem("Đang chọn", Theme.RUBY, Theme.RUBY),
                    LegendItem("Đã bán", Color.FromArgb(209, 213, 219), Color.FromArgb(209, 213, 219)));
            legend.WrapContents = true; legend.MaximumSize = new Size(760, 0);
            s.Add(legend, 10, false);
            legend.Anchor = AnchorStyles.Top;
            c.Controls.Add(s);
            return c;
        }

        private Control BuildComboCard()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Lay.Border(Theme.Lbl("Bắp & Nước Thơm Ngon", FontStyle.Bold, 16, Theme.TEXT), null,
                    Theme.Lbl("Tối ưu tiết kiệm tới 25%", FontStyle.Regular, 12, Theme.GREEN)));
            foreach (Combo cb in db.Combos)
            {
                Panel row = new Panel();
                row.BackColor = Theme.BG; row.Padding = new Padding(14, 10, 14, 10);
                row.AutoSize = true; row.AutoSizeMode = AutoSizeMode.GrowAndShrink;

                Stack txt = new Stack();
                txt.Dock = DockStyle.None;
                txt.Add(Theme.Lbl(cb.Name, FontStyle.Bold, 14, Theme.TEXT));
                txt.Add(Theme.Lbl(cb.Desc, FontStyle.Regular, 12, Theme.MUTED));

                NumericUpDown sp = new NumericUpDown();
                sp.Minimum = 0; sp.Maximum = 10; sp.Value = 0; sp.Width = 60; sp.Font = Theme.Fnt(FontStyle.Regular, 14);
                sp.ValueChanged += (sender, e) => UpdateSummary();
                comboSpin[cb.Id] = sp;
                FlowLayoutPanel right = Lay.Row(10, Theme.Lbl(Theme.Money(cb.Price), FontStyle.Bold, 14, Theme.RUBY), sp);

                TableLayoutPanel line = Lay.Border(txt, null, right);
                line.Dock = DockStyle.Top;
                row.Controls.Add(line);
                s.Add(row, 8);
            }
            c.Controls.Add(s);
            return c;
        }

        private Control Sep()
        {
            Panel p = new Panel(); p.Height = 1; p.MinimumSize = new Size(0, 1); p.MaximumSize = new Size(0, 1);
            p.BackColor = Theme.BORDER;
            return p;
        }

        private Control SumRow(string l, Label v)
        {
            return Lay.Border(Theme.Lbl(l, FontStyle.Regular, 13, Theme.MUTED), null, v);
        }

        private Control BuildOrderCard()
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Theme.Lbl("Tóm tắt đơn hàng", FontStyle.Bold, 18, Theme.TEXT));
            s.Add(lblSeats, 10);

            s.Add(Theme.Lbl("Mã khuyến mãi / Voucher", FontStyle.Bold, 12, Theme.MUTED), 12);
            RoundButton ap = Theme.Outline("Áp dụng");
            ap.Click += (e, a) => ApplyVoucher();
            txtVoucher.Submit += (e, a) => ApplyVoucher();
            s.Add(Lay.Border(null, txtVoucher, ap), 4);
            txtVoucher.Margin = new Padding(0, 0, 8, 0);
            s.Add(Theme.Lbl("Thử mã: CINELUXE20 hoặc WELCOME50", FontStyle.Regular, 11, Theme.MUTED), 4);

            chkPoints.AutoSize = true; chkPoints.MaximumSize = new Size(380, 0);
            chkPoints.BackColor = Color.Transparent; chkPoints.Font = Theme.Fnt(FontStyle.Regular, 13); chkPoints.ForeColor = Theme.TEXT;
            chkPoints.CheckedChanged += (e, a) => UpdateSummary();
            s.Add(chkPoints, 10, false);

            s.Add(Sep(), 10);
            s.Add(SumRow("Tạm tính vé xem phim", lblTicket), 8);
            s.Add(SumRow("Bắp nước & Combo", lblCombo), 4);
            s.Add(SumRow("Giảm giá (voucher / điểm)", lblDiscount), 4);
            s.Add(Sep(), 8);
            s.Add(Lay.Border(Theme.Lbl("TỔNG THANH TOÁN", FontStyle.Bold, 13, Theme.TEXT), null, lblTotal), 8);
            s.Add(lblEarn, 2);

            s.Add(Theme.Lbl("Phương thức thanh toán", FontStyle.Bold, 13, Theme.TEXT), 12);
            string[] methods = { "Ví MoMo", "Thẻ Visa/Master", "ZaloPay" };
            for (int i = 0; i < 3; i++)
            {
                pay[i].Text = methods[i]; pay[i].AutoSize = true; pay[i].Checked = i == 0;
                pay[i].BackColor = Color.Transparent; pay[i].Font = Theme.Fnt(FontStyle.Regular, 13); pay[i].ForeColor = Theme.TEXT;
                s.Add(pay[i], 2, false);
            }

            RoundButton go = Theme.Primary("Tiến hành Thanh toán & Nhận vé");
            go.MinimumSize = new Size(0, 46);
            go.Click += (e, a) => Pay();
            s.Add(go, 12);
            s.Add(Theme.Lbl("Vé điện tử có QR code vào thẳng phòng chiếu.\nĐược hủy vé trước giờ chiếu " + BookingService.CancelBeforeHours + " tiếng.",
                    FontStyle.Regular, 11, Theme.MUTED, 380), 6);
            c.Controls.Add(s);
            return c;
        }

        // ---------------- dữ liệu ----------------
        /// <summary>Mở trang đặt vé; movie == null thì chọn phim đang chiếu đầu tiên.</summary>
        public void SetMovie(Movie movie)
        {
            loading = true;
            cbMovie.Items.Clear();
            foreach (Movie m in db.Movies) if (m.Status == MovieStatus.DANG_CHIEU) cbMovie.Items.Add(m);
            if (movie != null && cbMovie.Items.Contains(movie)) cbMovie.SelectedItem = movie;
            else if (cbMovie.Items.Count > 0) cbMovie.SelectedIndex = 0;
            loading = false;
            appliedVoucher = ""; txtVoucher.Text = ""; chkPoints.Checked = false;
            ReloadShowtimes();
        }

        private void ReloadShowtimes()
        {
            loading = true;
            cbShow.Items.Clear();
            Movie m = cbMovie.SelectedItem as Movie;
            if (m != null)
            {
                DateTime now = DateTime.Now;
                foreach (Showtime s in ScheduleService.UpcomingOf(m.Id)) if (s.Start > now) cbShow.Items.Add(s);
                if (cbShow.Items.Count > 0) cbShow.SelectedIndex = 0;
            }
            loading = false;
            ReloadSeats();
        }

        private void ReloadSeats()
        {
            selected.Clear();
            seatArea.Reset();
            Showtime st = cbShow.SelectedItem as Showtime;
            Movie m = cbMovie.SelectedItem as Movie;
            if (st == null || m == null)
            {
                lblInfo.Text = "Phim này hiện chưa có suất chiếu sắp tới.";
                UpdateSummary();
                return;
            }
            Room room = db.GetRoom(st.RoomId);
            lblInfo.Text = m.Genre + " • " + m.Duration + " phút • " + m.Age + "   |   " + room.Name + " - " + room.Info
                    + "   |   Còn " + st.FreeCount() + " ghế trống / " + st.BookedCount() + " đã đặt";
            HashSet<string> sold = BookingService.SoldSeats(st.Id);
            seatArea.SuspendLayout();
            foreach (char r in SeatMap.Rows)
            {
                List<Control> items = new List<Control>();
                Label rl = Theme.Lbl(r.ToString(), FontStyle.Bold, 12, Theme.MUTED);
                rl.AutoSize = false; rl.Size = new Size(16, 34); rl.TextAlign = ContentAlignment.MiddleCenter;
                items.Add(rl);
                for (int col = 1; col <= SeatMap.ColsOf(r); col++)
                {
                    string name = SeatMap.NameOf(r, col);
                    SeatButton b = new SeatButton(name, sold.Contains(name));
                    b.Toggled += (s, e) => ToggleSeat((SeatButton)s);
                    tip.SetToolTip(b, name + " • " + SeatMap.TypeOf(name) + " • " + Theme.Money(SeatMap.PriceOf(name)));
                    items.Add(b);
                }
                FlowLayoutPanel row = Lay.Row(6, items.ToArray());
                seatArea.Add(row, 6, false);
                row.Anchor = AnchorStyles.Top;               // canh giữa
            }
            seatArea.ResumeLayout(true);
            UpdateSummary();
        }

        private void ToggleSeat(SeatButton b)
        {
            if (b.Checked)
            {
                if (selected.Count >= 8) { b.Checked = false; Theme.Error(this, "Mỗi đơn đặt tối đa 8 ghế."); return; }
                selected.Add(b.Seat);
            }
            else selected.Remove(b.Seat);
            UpdateSummary();
        }

        private void ApplyVoucher()
        {
            string code = txtVoucher.Text.Trim().ToUpperInvariant();
            if (code.Length == 0) { appliedVoucher = ""; UpdateSummary(); return; }
            if (BookingService.VoucherValue(code) == 0) { Theme.Error(this, "Mã khuyến mãi không hợp lệ hoặc đã hết hạn."); return; }
            appliedVoucher = code;
            UpdateSummary();
        }

        private Dictionary<string, int> CombosChosen()
        {
            Dictionary<string, int> m = new Dictionary<string, int>();
            foreach (KeyValuePair<string, NumericUpDown> kv in comboSpin)
            {
                int q = (int)kv.Value.Value;
                if (q > 0) m[kv.Key] = q;
            }
            return m;
        }

        private void UpdateSummary()
        {
            long ticket = 0;
            foreach (string s in selected) ticket += SeatMap.PriceOf(s);
            long combo = BookingService.ComboTotal(CombosChosen());
            long sub = ticket + combo;
            long voucher = Math.Min(BookingService.VoucherValue(appliedVoucher), sub);
            int maxPts = (int)Math.Min(user.Points, (sub - voucher) / BookingService.PointValue);
            chkPoints.Text = "Dùng điểm CineRewards\n(" + user.Points + " điểm = " + Theme.Money((long)user.Points * BookingService.PointValue) + ")";
            long pts = chkPoints.Checked ? (long)maxPts * BookingService.PointValue : 0;
            long total = sub - voucher - pts;
            lblSeats.Text = selected.Count == 0 ? "Chưa chọn ghế" : selected.Count + " vé giữ chỗ: " + string.Join(", ", selected);
            lblTicket.Text = Theme.Money(ticket);
            lblCombo.Text = Theme.Money(combo);
            lblDiscount.Text = "-" + Theme.Money(voucher + pts);
            lblTotal.Text = Theme.Money(total);
            lblEarn.Text = "+" + (total / 10000) + " CineRewards Points";
        }

        private void Pay()
        {
            Showtime st = cbShow.SelectedItem as Showtime;
            if (st == null) { Theme.Error(this, "Vui lòng chọn suất chiếu."); return; }
            if (selected.Count == 0) { Theme.Error(this, "Vui lòng chọn ít nhất 1 ghế."); return; }
            string method = "Ví MoMo";
            foreach (RadioButton r in pay) if (r.Checked) method = r.Text;
            if (!Theme.Confirm(this, "Xác nhận thanh toán " + lblTotal.Text + " bằng " + method + "?")) return;
            try
            {
                Booking o = BookingService.Book(user, st, selected.ToList(), CombosChosen(), appliedVoucher, chkPoints.Checked, method);
                frame.RefreshUser();
                Theme.Info(this, "Thanh toán thành công!\n\nMã đặt vé: " + o.Id + "\nMã check-in (đưa cho nhân viên soát vé): " + o.CheckInCode
                        + "\nGhế: " + string.Join(", ", o.Seats) + "\nTổng thanh toán: " + Theme.Money(o.Total) + "\nĐiểm tích lũy: +" + o.PointsEarned);
                appliedVoucher = ""; txtVoucher.Text = ""; chkPoints.Checked = false;
                foreach (NumericUpDown n in comboSpin.Values) n.Value = 0;
                frame.ShowTickets();
            }
            catch (InvalidOperationException ex)
            {
                Theme.Error(this, ex.Message);
                ReloadSeats();
            }
        }

        // ---------------- nút ghế ----------------
        private class SeatButton : Control
        {
            public readonly string Seat;
            private readonly bool sold;
            private bool isChecked;
            public event EventHandler Toggled;

            public SeatButton(string seat, bool sold)
            {
                Seat = seat; this.sold = sold;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
                BackColor = Color.Transparent;
                bool dbl = seat[0] == 'H';
                Size = new Size(dbl ? 86 : 42, 34); MinimumSize = Size; MaximumSize = Size;
                Font = Theme.Fnt(FontStyle.Bold, 11);
                Cursor = sold ? Cursors.Default : Cursors.Hand;
            }

            public bool Checked
            {
                get { return isChecked; }
                set { isChecked = value; Invalidate(); }
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                if (sold) return;
                isChecked = !isChecked;
                Invalidate();
                if (Toggled != null) Toggled(this, EventArgs.Empty);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color fill, line, txt;
                char r = Seat[0];
                if (sold) { fill = Color.FromArgb(209, 213, 219); line = fill; txt = Color.FromArgb(107, 114, 128); }
                else if (isChecked) { fill = Theme.RUBY; line = Theme.RUBY; txt = Color.White; }
                else if (r <= 'C') { fill = Color.White; line = Theme.BORDER; txt = Theme.TEXT; }
                else if (r <= 'G') { fill = Theme.AMBER_SOFT; line = Theme.AMBER; txt = Theme.TEXT; }
                else { fill = Theme.RUBY_SOFT; line = Theme.RUBY; txt = Theme.TEXT; }
                using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, Width - 1, Height - 1), 10))
                {
                    using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, p);
                    using (Pen pen = new Pen(line)) g.DrawPath(pen, p);
                }
                TextRenderer.DrawText(g, Seat, Font, ClientRectangle, txt,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            }
        }
    }
}
