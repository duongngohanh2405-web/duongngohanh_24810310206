using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;

namespace CineLuxe.UI
{
    /// <summary>Xem thông tin chi tiết một suất chiếu: số ghế trống / đã đặt, thống kê theo loại ghế và sơ đồ ghế.</summary>
    public static class ShowtimeDetailDialog
    {
        /// <summary>Ô ghế nhỏ trong sơ đồ (trắng = trống, đỏ = đã đặt).</summary>
        private class Chip : Control
        {
            private readonly bool booked;
            public Chip(string name, bool booked, bool wide)
            {
                this.booked = booked; Text = name;
                Font = Theme.Fnt(FontStyle.Bold, 10);
                Size = new Size(wide ? 66 : 36, 24); MinimumSize = Size; MaximumSize = Size;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Color fill = booked ? Theme.RUBY : Color.White, line = booked ? Theme.RUBY : Theme.BORDER;
                using (SolidBrush b = new SolidBrush(fill)) e.Graphics.FillRectangle(b, 0, 0, Width, Height);
                using (Pen p = new Pen(line)) e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, booked ? Color.White : Theme.TEXT,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            }
        }

        public static void Show(IWin32Window owner, Showtime st)
        {
            DataStore db = DataStore.Get();
            Movie m = db.GetMovie(st.MovieId);
            Room r = db.GetRoom(st.RoomId);

            Stack p = new Stack();
            p.Add(Theme.Lbl(m.Title, FontStyle.Bold, 20, Theme.TEXT, 600));
            p.Add(Theme.Lbl(r.Name + " • " + r.Format + "   |   " + Fmt.Day(st.Start) + "   " + Fmt.Range(st.Start, st.End)
                    + "   |   " + m.Duration + " phút", FontStyle.Regular, 13, Theme.MUTED, 600));

            p.Add(Lay.Grid(3, 10, 0, new Control[] {
                Stat("TỔNG SỐ GHẾ", st.TotalSeats(), Theme.TEXT),
                Stat("GHẾ TRỐNG", st.FreeCount(), Theme.GREEN),
                Stat("GHẾ ĐÃ ĐẶT", st.BookedCount(), Theme.RUBY) }), 10);

            // thống kê theo loại ghế (duyệt mảng 2 chiều, dùng đa hình Type)
            List<string> order = new List<string>();
            Dictionary<string, int[]> byType = new Dictionary<string, int[]>();
            foreach (Seat[] row in st.Seats)
                foreach (Seat s in row)
                {
                    int[] a;
                    if (!byType.TryGetValue(s.Type, out a)) { a = new int[2]; byType[s.Type] = a; order.Add(s.Type); }
                    a[s.IsBooked ? 1 : 0]++;
                }
            List<Control> parts = new List<Control>();
            foreach (string k in order)
                parts.Add(Theme.Lbl("Ghế " + k + ": trống " + byType[k][0] + ", đã đặt " + byType[k][1], FontStyle.Regular, 13, Theme.TEXT));
            p.Add(Lay.Row(18, parts.ToArray()), 10, false);

            // sơ đồ ghế
            Panel bar = new Panel(); bar.Height = 3; bar.BackColor = Theme.RUBY; bar.MinimumSize = new Size(0, 3);
            p.Add(bar, 10);
            Label screen = Theme.Lbl("MÀN HÌNH", FontStyle.Bold, 11, Theme.MUTED);
            screen.TextAlign = ContentAlignment.MiddleCenter; screen.Padding = new Padding(0, 4, 0, 4);
            p.Add(screen);
            ToolTip tt = new ToolTip();
            for (int i = 0; i < st.Seats.Length; i++)
            {
                List<Control> items = new List<Control>();
                Label rl = Theme.Lbl(SeatMap.Rows[i].ToString(), FontStyle.Bold, 11, Theme.MUTED);
                rl.AutoSize = false; rl.Size = new Size(16, 24); rl.TextAlign = ContentAlignment.MiddleCenter;
                items.Add(rl);
                foreach (Seat s in st.Seats[i])
                {
                    Chip c = new Chip(s.Name, s.IsBooked, s is SweetboxSeat);
                    tt.SetToolTip(c, s.ToString() + (s.IsBooked ? " - ĐÃ ĐẶT" : " - trống"));
                    items.Add(c);
                }
                FlowLayoutPanel row = Lay.Row(4, items.ToArray());
                p.Add(row, 4, false);
                row.Anchor = AnchorStyles.Top;
            }
            p.Add(Theme.Lbl("Ô trắng: ghế trống   •   Ô đỏ: ghế đã đặt", FontStyle.Regular, 12, Theme.MUTED), 8);
            string booked = st.BookedSeats().Count == 0 ? "chưa có" : string.Join(", ", st.BookedSeats());
            p.Add(Theme.Lbl("Danh sách ghế đã đặt: " + booked, FontStyle.Regular, 12, Theme.TEXT, 560), 6);

            p.MinimumSize = new Size(580, 0);
            Dialogs.Message(owner, "Thông tin suất chiếu", p);
            tt.Dispose();
        }

        private static Control Stat(string cap, int value, Color color)
        {
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Theme.Lbl(cap, FontStyle.Bold, 11, Theme.MUTED));
            s.Add(Theme.Lbl(value.ToString(), FontStyle.Bold, 26, color));
            c.Controls.Add(s);
            return c;
        }
    }
}
