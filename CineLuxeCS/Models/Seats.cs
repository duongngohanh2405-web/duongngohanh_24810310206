using System;
using System.Collections.Generic;

namespace CineLuxe.Models
{
    /// <summary>Lớp cha trừu tượng của mọi loại ghế. Mỗi loại ghế tự quy định giá và tên loại (đa hình).</summary>
    public abstract class Seat
    {
        private readonly string name;      // ví dụ F07
        private bool booked;

        protected Seat(string name) { this.name = name; }

        public string Name { get { return name; } }
        public bool IsBooked { get { return booked; } set { booked = value; } }

        /// <summary>Giá vé của ghế (mỗi lớp con có giá riêng).</summary>
        public abstract int Price { get; }

        /// <summary>Tên loại ghế để hiển thị.</summary>
        public abstract string Type { get; }

        public override string ToString() { return name + " (" + Type + ", " + Price + "đ)"; }
    }

    /// <summary>Ghế thường (hàng A-C).</summary>
    public class StandardSeat : Seat
    {
        public StandardSeat(string name) : base(name) { }
        public override int Price { get { return 95000; } }
        public override string Type { get { return "Thường"; } }
    }

    /// <summary>Ghế VIP (hàng D-G).</summary>
    public class VipSeat : Seat
    {
        public VipSeat(string name) : base(name) { }
        public override int Price { get { return 120000; } }
        public override string Type { get { return "VIP"; } }
    }

    /// <summary>Ghế đôi Sweetbox (hàng H), giá cho cả ghế đôi.</summary>
    public class SweetboxSeat : Seat
    {
        public SweetboxSeat(string name) : base(name) { }
        public override int Price { get { return 240000; } }
        public override string Type { get { return "Sweetbox"; } }
    }

    /// <summary>Bố cục ghế dùng chung cho mọi phòng: A-C thường, D-G VIP, H Sweetbox đôi.</summary>
    public static class SeatMap
    {
        public const string Rows = "ABCDEFGH";
        public const int Cols = 10, SweetboxCols = 5;
        public const int PriceStandard = 95000, PriceVip = 120000, PriceSweetbox = 240000;

        public static int ColsOf(char row) { return row == 'H' ? SweetboxCols : Cols; }

        public static string NameOf(char row, int col) { return row.ToString() + col.ToString("00"); }

        /// <summary>Tạo đúng lớp ghế theo hàng (đa hình).</summary>
        public static Seat Of(string name)
        {
            char r = name[0];
            if (r <= 'C') return new StandardSeat(name);
            if (r <= 'G') return new VipSeat(name);
            return new SweetboxSeat(name);
        }

        /// <summary>Tạo mảng hai chiều ghế: seats[hàng][cột]. Hàng H chỉ có 5 ghế đôi nên là mảng so le.</summary>
        public static Seat[][] Build()
        {
            Seat[][] seats = new Seat[Rows.Length][];
            for (int i = 0; i < Rows.Length; i++)
            {
                char row = Rows[i];
                seats[i] = new Seat[ColsOf(row)];
                for (int c = 0; c < seats[i].Length; c++) seats[i][c] = Of(NameOf(row, c + 1));
            }
            return seats;
        }

        public static int PriceOf(string seat) { return Of(seat).Price; }
        public static string TypeOf(string seat) { return Of(seat).Type; }

        public static List<string> All()
        {
            List<string> l = new List<string>();
            foreach (char r in Rows)
                for (int c = 1; c <= ColsOf(r); c++) l.Add(NameOf(r, c));
            return l;
        }

        public static int Total() { return All().Count; }
    }

    /// <summary>Suất chiếu: chứa mảng hai chiều Seat[][] biểu diễn sơ đồ ghế của suất đó.</summary>
    public class Showtime
    {
        public string Id, MovieId, RoomId;
        public DateTime Start, End;
        public readonly Seat[][] Seats = SeatMap.Build();      // mảng hai chiều

        public Showtime(string id, string movieId, string roomId, DateTime start, DateTime end)
        {
            Id = id; MovieId = movieId; RoomId = roomId; Start = start; End = end;
        }

        /// <summary>Tìm ghế theo tên (ví dụ "F07"); trả về null nếu không có.</summary>
        public Seat FindSeat(string name)
        {
            if (name == null || name.Length < 2) return null;
            int r = SeatMap.Rows.IndexOf(name[0]);
            int c;
            if (!int.TryParse(name.Substring(1), out c)) return null;
            c -= 1;
            if (r < 0 || c < 0 || c >= Seats[r].Length) return null;
            return Seats[r][c];
        }

        public bool IsBooked(string name) { Seat s = FindSeat(name); return s != null && s.IsBooked; }

        public void Book(string name)
        {
            Seat s = FindSeat(name);
            if (s == null) throw new ArgumentException("Ghế không tồn tại: " + name);
            if (s.IsBooked) throw new InvalidOperationException("Ghế " + name + " đã có người đặt.");
            s.IsBooked = true;
        }

        public void Release(string name) { Seat s = FindSeat(name); if (s != null) s.IsBooked = false; }

        public void ClearAll() { foreach (Seat[] row in Seats) foreach (Seat s in row) s.IsBooked = false; }

        public int TotalSeats() { int n = 0; foreach (Seat[] row in Seats) n += row.Length; return n; }

        public int BookedCount()
        {
            int n = 0;
            foreach (Seat[] row in Seats) foreach (Seat s in row) if (s.IsBooked) n++;
            return n;
        }

        public int FreeCount() { return TotalSeats() - BookedCount(); }

        public List<string> BookedSeats()
        {
            List<string> l = new List<string>();
            foreach (Seat[] row in Seats) foreach (Seat s in row) if (s.IsBooked) l.Add(s.Name);
            return l;
        }
    }
}
