using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Suất chiếu &amp; Phòng: lịch Gantt theo phòng, xếp lịch chiếu mới (kiểm tra xung đột), xóa suất.</summary>
    public class AdminSchedulePanel : Panel
    {
        private readonly DataStore db = DataStore.Get();
        private DateTime day = DateTime.Today;
        private readonly Label lblDay = Theme.Lbl("", FontStyle.Bold, 16, Theme.TEXT);
        private readonly GanttView gantt;
        private readonly DataGridView table = Theme.Table("PHÒNG", "PHIM", "BẮT ĐẦU", "KẾT THÚC", "GHẾ ĐÃ ĐẶT / TRỐNG", "LẤP ĐẦY");
        private readonly List<Showtime> shown = new List<Showtime>();

        public AdminSchedulePanel()
        {
            Dock = DockStyle.Fill; BackColor = Theme.BG;
            Padding = new Padding(28, 22, 28, 24);
            gantt = new GanttView(db);

            // vùng giữa (thêm trước để dock Fill sau cùng)
            RoundPanel gc = Theme.Card();
            gc.Dock = DockStyle.Fill; gc.AutoSize = false;
            Stack gs = new Stack();
            gs.Add(Theme.Lbl("Cinema Hall Gantt Schedule • " + Fmt.CINEMA, FontStyle.Bold, 15, Theme.TEXT));
            gs.Add(gantt, 6);
            gc.Controls.Add(gs);
            Controls.Add(gc);

            Panel gap = new Panel();             // khoảng cách giữa Gantt và bảng
            gap.Dock = DockStyle.Bottom; gap.Height = 12; gap.BackColor = Theme.BG;
            Controls.Add(gap);

            table.Dock = DockStyle.Bottom; table.Height = 230;
            table.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) ShowDetail(); };
            Controls.Add(table);

            Stack north = new Stack();
            Stack t = new Stack();
            t.Dock = DockStyle.None;
            t.Add(Theme.Lbl("Quản Lý Phim & Xếp Lịch Chiếu", FontStyle.Bold, 26, Theme.TEXT));
            t.Add(Theme.Lbl("Sắp xếp suất chiếu theo từng phòng • tự động giãn cách dọn phòng " + ScheduleService.CleaningMinutes + " phút",
                    FontStyle.Regular, 12, Theme.MUTED));
            RoundButton add = Theme.Primary("+ Xếp lịch chiếu mới"), del = Theme.Outline("Xóa suất đã chọn"), info = Theme.Outline("Xem chi tiết suất");
            info.Click += (s, e) => ShowDetail();
            add.Click += (s, e) => OpenAddDialog();
            del.Click += (s, e) => DeleteSelected();
            north.Add(Lay.Border(t, null, Lay.Row(8, add, info, del)));

            RoundButton prev = Theme.Outline("◀"), next = Theme.Outline("▶"), today = Theme.Outline("Hôm nay");
            prev.Click += (s, e) => { day = day.AddDays(-1); RefreshList(); };
            next.Click += (s, e) => { day = day.AddDays(1); RefreshList(); };
            today.Click += (s, e) => { day = DateTime.Today; RefreshList(); };
            north.Add(Lay.Row(8, prev, lblDay, next, today), 10, false);
            Panel np = Lay.Pad(north, 0, 0, 0, 12);
            np.Dock = DockStyle.Top;
            Controls.Add(np);
        }

        public void RefreshList()
        {
            lblDay.Text = "  " + Fmt.Day(day) + "  ";
            shown.Clear(); table.Rows.Clear();
            foreach (Showtime s in ScheduleService.ForDay(day))
            {
                shown.Add(s);
                int sold = BookingService.SoldSeats(s.Id).Count;
                table.Rows.Add(db.GetRoom(s.RoomId).Name, db.GetMovie(s.MovieId).Title, Fmt.Time(s.Start), Fmt.Time(s.End),
                        sold + " đã đặt / " + (s.TotalSeats() - sold) + " trống",
                        (sold * 100.0 / s.TotalSeats()).ToString("0", Theme.Inv) + "%");
            }
            gantt.SetData(new List<Showtime>(shown));
            Theme.ClearSel(table);
        }

        /// <summary>Xem thông tin chi tiết suất chiếu đang chọn (số ghế trống, đã đặt, sơ đồ ghế).</summary>
        private void ShowDetail()
        {
            int r = Theme.SelectedIndex(table);
            if (r < 0) { Theme.Error(this, "Vui lòng chọn một suất chiếu trong bảng (hoặc nhấp đúp vào dòng)."); return; }
            ShowtimeDetailDialog.Show(this, shown[r]);
        }

        private void DeleteSelected()
        {
            int r = Theme.SelectedIndex(table);
            if (r < 0) { Theme.Error(this, "Vui lòng chọn một suất chiếu trong bảng bên dưới."); return; }
            Showtime s = shown[r];
            if (!Theme.Confirm(this, "Xóa suất " + Fmt.Time(s.Start) + " - " + db.GetMovie(s.MovieId).Title + "?")) return;
            try { ScheduleService.RemoveShowtime(s); RefreshList(); }
            catch (InvalidOperationException ex) { Theme.Error(this, ex.Message); }
        }

        public void OpenAddDialog()
        {
            ComboBox cbMovie = new ComboBox(), cbRoom = new ComboBox();
            cbMovie.DropDownStyle = ComboBoxStyle.DropDownList; cbRoom.DropDownStyle = ComboBoxStyle.DropDownList;
            cbMovie.Font = Theme.Fnt(FontStyle.Regular, 14); cbRoom.Font = Theme.Fnt(FontStyle.Regular, 14);
            foreach (Movie m in db.Movies) if (m.Status != MovieStatus.NGUNG) cbMovie.Items.Add(m);
            foreach (Room r in db.Rooms) cbRoom.Items.Add(r);
            if (cbMovie.Items.Count > 0) cbMovie.SelectedIndex = 0;
            if (cbRoom.Items.Count > 0) cbRoom.SelectedIndex = 0;
            Input date = Theme.Field(), time = Theme.Field();
            date.Text = Fmt.Date(day); time.Text = "20:00";
            cbMovie.MinimumSize = new Size(340, 0); cbRoom.MinimumSize = new Size(340, 0);
            date.MinimumSize = new Size(340, 40); time.MinimumSize = new Size(340, 40);

            Stack f = new Stack();
            f.Add(Theme.Lbl("Phim", FontStyle.Bold, 12, Theme.MUTED));
            f.Add(cbMovie, 4);
            f.Add(Theme.Lbl("Phòng chiếu", FontStyle.Bold, 12, Theme.MUTED), 10);
            f.Add(cbRoom, 4);
            f.Add(Theme.Lbl("Ngày chiếu (dd/MM/yyyy)", FontStyle.Bold, 12, Theme.MUTED), 10);
            f.Add(date, 4);
            f.Add(Theme.Lbl("Giờ bắt đầu (HH:mm)", FontStyle.Bold, 12, Theme.MUTED), 10);
            f.Add(time, 4);
            f.Add(Theme.Lbl("Giờ kết thúc = giờ bắt đầu + thời lượng phim; cách suất khác tối thiểu " + ScheduleService.CleaningMinutes + " phút.",
                    FontStyle.Regular, 12, Theme.MUTED, 340), 10);
            if (!Dialogs.OkCancel(this, "Xếp lịch chiếu mới", f, "OK")) return;
            try
            {
                if (cbMovie.SelectedItem == null || cbRoom.SelectedItem == null) throw new ArgumentException("Vui lòng chọn phim và phòng chiếu.");
                DateTime start = Fmt.ParseDate(date.Text).Add(Fmt.ParseTime(time.Text));
                if (start < DateTime.Now.AddDays(-1)) throw new ArgumentException("Không thể xếp lịch trong quá khứ.");
                Showtime st = ScheduleService.Add(((Movie)cbMovie.SelectedItem).Id, ((Room)cbRoom.SelectedItem).Id, start);
                day = st.Start.Date;
                RefreshList();
                Theme.Info(this, "Đã xếp lịch: " + Fmt.DateTimeStr(st.Start) + " - " + db.GetMovie(st.MovieId).Title);
            }
            catch (ArgumentException ex) { Theme.Error(this, ex.Message); }
            catch (InvalidOperationException ex) { Theme.Error(this, ex.Message); }
        }

        /// <summary>Biểu đồ Gantt: mỗi phòng một hàng, trục giờ 08:00 → 24:00.</summary>
        private class GanttView : Control
        {
            private readonly DataStore db;
            private List<Showtime> data = new List<Showtime>();
            private const int START_MIN = 8 * 60, END_MIN = 24 * 60, LEFT = 150, ROW_H = 62;

            public GanttView(DataStore db)
            {
                this.db = db;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
                int h = 40 + ROW_H * Math.Max(1, db.Rooms.Count) + 10;
                MinimumSize = new Size(300, h); MaximumSize = new Size(0, h); Size = new Size(800, h);
            }

            public void SetData(List<Showtime> d) { data = d; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                g.Clear(Color.White);
                int w = Width - LEFT - 10;
                int span = END_MIN - START_MIN;
                using (Font f11 = Theme.Fnt(FontStyle.Regular, 11), f13b = Theme.Fnt(FontStyle.Bold, 13), f11b = Theme.Fnt(FontStyle.Bold, 11), f10 = Theme.Fnt(FontStyle.Regular, 10))
                using (SolidBrush muted = new SolidBrush(Theme.MUTED), text = new SolidBrush(Theme.TEXT), white = new SolidBrush(Color.White))
                using (Pen border = new Pen(Theme.BORDER))
                {
                    for (int h = 8; h <= 24; h += 2)
                    {
                        int x = LEFT + (h * 60 - START_MIN) * w / span;
                        g.DrawLine(border, x, 24, x, 24 + ROW_H * db.Rooms.Count);
                        g.DrawString((h % 24).ToString("00") + ":00", f11, muted, x - 16, 4);
                    }
                    for (int i = 0; i < db.Rooms.Count; i++)
                    {
                        Room r = db.Rooms[i];
                        int y = 24 + i * ROW_H;
                        using (SolidBrush bg = new SolidBrush(i % 2 == 0 ? Color.FromArgb(249, 250, 251) : Color.White))
                            g.FillRectangle(bg, 0, y, Width, ROW_H);
                        g.DrawString(r.Name, f13b, text, 8, y + 12);
                        g.DrawString(r.Format, f11, muted, 8, y + 32);
                        foreach (Showtime s in data)
                        {
                            if (s.RoomId != r.Id) continue;
                            int a = Math.Max(START_MIN, s.Start.Hour * 60 + s.Start.Minute);
                            int endMin = s.End.Date > s.Start.Date ? END_MIN : s.End.Hour * 60 + s.End.Minute;
                            int x1 = LEFT + (a - START_MIN) * w / span;
                            int x2 = LEFT + (Math.Min(endMin, END_MIN) - START_MIN) * w / span;
                            float hue = (Math.Abs(Theme.StableHash(s.MovieId) % 360)) / 360f;
                            using (SolidBrush b = new SolidBrush(Theme.FromHsb(hue, 0.55f, 0.85f)))
                            using (GraphicsPath p = Theme.Rounded(new RectangleF(x1, y + 8, Math.Max(8, x2 - x1), ROW_H - 16), 10))
                                g.FillPath(b, p);
                            // vùng dọn phòng
                            int cx2 = Math.Min(LEFT + w, x2 + ScheduleService.CleaningMinutes * w / span);
                            using (SolidBrush b = new SolidBrush(Color.FromArgb(209, 213, 219)))
                                g.FillRectangle(b, x2, y + 22, Math.Max(0, cx2 - x2), ROW_H - 44);
                            Rectangle clip = new Rectangle(x1 + 4, y + 8, Math.Max(1, x2 - x1 - 8), ROW_H - 16);
                            g.SetClip(clip);
                            g.DrawString(db.GetMovie(s.MovieId).Title, f11b, white, x1 + 6, y + 14);
                            g.DrawString(Fmt.Range(s.Start, s.End) + "  " + BookingService.SoldSeats(s.Id).Count + "/" + SeatMap.Total(), f10, white, x1 + 6, y + 33);
                            g.ResetClip();
                        }
                    }
                }
            }
        }
    }
}
