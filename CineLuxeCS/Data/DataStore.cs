using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CineLuxe.Models;

namespace CineLuxe.Data
{
    /// <summary>
    /// Kho dữ liệu của ứng dụng (List trong bộ nhớ) + lưu/đọc file văn bản trong thư mục data/:
    /// movies.txt, rooms.txt, showtimes.txt, users.txt, combos.txt, bookings.txt, meta.txt.
    /// Mỗi dòng là một bản ghi, các trường cách nhau bằng dấu Tab. Xóa thư mục data/ để tạo lại dữ liệu mẫu.
    /// Nếu db.properties bật MySQL thì dữ liệu được lưu vào MySQL thay cho file.
    /// </summary>
    public class DataStore
    {
        private static readonly string Dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        private static readonly object Gate = new object();
        private static DataStore instance;

        public List<User> Users = new List<User>();
        public List<Movie> Movies = new List<Movie>();
        public List<Room> Rooms = new List<Room>();
        public List<Showtime> Showtimes = new List<Showtime>();
        public List<Combo> Combos = new List<Combo>();
        public List<Booking> Orders = new List<Booking>();
        public int Seq = 10000;
        private DbStorage db;

        public static DataStore Get()
        {
            lock (Gate)
            {
                if (instance == null)
                {
                    DbStorage storage = DbStorage.Connect();
                    instance = new DataStore();
                    instance.db = storage;
                    if (!instance.Load())
                    {
                        instance = new DataStore();
                        instance.db = storage;
                        Seeder.Seed(instance);
                        instance.SyncSeats();
                        instance.Save();
                    }
                }
                return instance;
            }
        }

        /// <summary>Đánh dấu lại ghế đã đặt của mọi suất chiếu từ danh sách đặt vé chưa hủy.</summary>
        public void SyncSeats()
        {
            foreach (Showtime s in Showtimes) s.ClearAll();
            foreach (Booking b in Orders)
            {
                if (b.Status == BookingStatus.DA_HUY) continue;
                Showtime s = GetShowtime(b.ShowtimeId);
                if (s != null) foreach (string n in b.Seats) s.Book(n);
            }
        }

        // ===================== FILE I/O (văn bản, phân cách bằng Tab) =====================
        private static string Esc(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\n", "\\n").Replace("\r", "");
        }

        private static string Unesc(string s)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[++i];
                    sb.Append(n == 't' ? '\t' : n == 'n' ? '\n' : n);
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static string Line(params object[] fields)
        {
            return string.Join("\t", fields.Select(f => Esc(f == null ? "" : Convert.ToString(f, Inv))));
        }

        private static string D(DateTime d) { return d.ToString("yyyy-MM-dd", Inv); }
        private static string DT(DateTime d) { return d.ToString("yyyy-MM-dd'T'HH:mm:ss", Inv); }
        private static string B(bool b) { return b ? "true" : "false"; }

        private static List<string[]> Read(string file)
        {
            List<string[]> rows = new List<string[]>();
            string p = Path.Combine(Dir, file);
            if (!File.Exists(p)) return rows;
            foreach (string l in File.ReadAllLines(p, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(l) || l.StartsWith("#")) continue;
                string[] parts = l.Split('\t');
                for (int i = 0; i < parts.Length; i++) parts[i] = Unesc(parts[i]);
                rows.Add(parts);
            }
            return rows;
        }

        private static void Write(string file, string header, List<string> lines)
        {
            List<string> all = new List<string>();
            all.Add("# " + header);
            all.AddRange(lines);
            File.WriteAllLines(Path.Combine(Dir, file), all, new UTF8Encoding(false));
        }

        public void Save()
        {
            lock (Gate)
            {
                if (db != null) { db.Save(this); return; }
                try
                {
                    Directory.CreateDirectory(Dir);
                    List<string> l = new List<string>();
                    foreach (Movie m in Movies)
                        l.Add(Line(m.Id, m.Title, m.Genre, m.Director, m.Duration, m.Age, m.Formats, m.Imdb.ToString("0.0", Inv),
                                m.Status.ToString(), D(m.ReleaseDate), B(m.Vietnamese), m.Description));
                    Write("movies.txt", "id, tên, thể loại, đạo diễn, phút, tuổi, định dạng, imdb, trạng thái, khởi chiếu, phim VN, mô tả", l);

                    l = new List<string>();
                    foreach (Room r in Rooms) l.Add(Line(r.Id, r.Name, r.Format, r.Info));
                    Write("rooms.txt", "id, tên phòng, định dạng, ghi chú", l);

                    l = new List<string>();
                    foreach (Showtime s in Showtimes) l.Add(Line(s.Id, s.MovieId, s.RoomId, DT(s.Start), DT(s.End)));
                    Write("showtimes.txt", "id, mã phim, mã phòng, bắt đầu, kết thúc", l);

                    l = new List<string>();
                    foreach (User u in Users)
                        l.Add(Line(u.Id, u.FullName, u.Phone, u.Email, u.Password, u.Role.ToString(), u.Dob, u.Code, u.Points));
                    Write("users.txt", "id, họ tên, sđt, email, mật khẩu, vai trò, ngày sinh, mã nv, điểm", l);

                    l = new List<string>();
                    foreach (Combo c in Combos) l.Add(Line(c.Id, c.Name, c.Desc, c.Price));
                    Write("combos.txt", "id, tên, mô tả, giá", l);

                    l = new List<string>();
                    foreach (Booking b in Orders)
                    {
                        string cb = string.Join(",", b.Combos.Select(kv => kv.Key + ":" + kv.Value));
                        l.Add(Line(b.Id, b.CheckInCode, b.UserId, b.ShowtimeId, b.Payment, b.Voucher, b.ScannedBy,
                                string.Join(",", b.Seats), cb, b.TicketTotal, b.ComboTotal, b.Discount, b.Total,
                                b.PointsUsed, b.PointsEarned, b.Status.ToString(), DT(b.CreatedAt),
                                b.ScannedAt.HasValue ? DT(b.ScannedAt.Value) : ""));
                    }
                    Write("bookings.txt", "id, mã check-in, user, suất chiếu, thanh toán, voucher, người soát, ghế, combo, tiền vé, tiền combo, giảm, tổng, điểm dùng, điểm tích, trạng thái, tạo lúc, soát lúc", l);

                    File.WriteAllText(Path.Combine(Dir, "meta.txt"), Seq.ToString(Inv) + Environment.NewLine, new UTF8Encoding(false));
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine("Không lưu được dữ liệu: " + e.Message);
                }
            }
        }

        /// <summary>Đọc dữ liệu từ thư mục data/. Trả về false nếu chưa có dữ liệu hoặc file lỗi.</summary>
        private bool Load()
        {
            if (db != null) return db.Load(this);
            try
            {
                if (!File.Exists(Path.Combine(Dir, "movies.txt"))) return false;
                foreach (string[] f in Read("movies.txt"))
                {
                    Movie m = new Movie(f[0], f[1], f[2], f[3], int.Parse(f[4], Inv), f[5], f[6], double.Parse(f[7], Inv),
                            (MovieStatus)Enum.Parse(typeof(MovieStatus), f[8]),
                            DateTime.ParseExact(f[9], "yyyy-MM-dd", Inv), f[11]);
                    m.Vietnamese = bool.Parse(f[10]);
                    Movies.Add(m);
                }
                foreach (string[] f in Read("rooms.txt")) Rooms.Add(new Room(f[0], f[1], f[2], f[3]));
                foreach (string[] f in Read("showtimes.txt"))
                    Showtimes.Add(new Showtime(f[0], f[1], f[2], DateTime.Parse(f[3], Inv), DateTime.Parse(f[4], Inv)));
                foreach (string[] f in Read("users.txt"))
                {
                    User u = new User(f[0], f[1], f[2], f[3], f[4], (Role)Enum.Parse(typeof(Role), f[5]));
                    u.Dob = f[6]; u.Code = f[7]; u.Points = int.Parse(f[8], Inv);
                    Users.Add(u);
                }
                foreach (string[] f in Read("combos.txt")) Combos.Add(new Combo(f[0], f[1], f[2], int.Parse(f[3], Inv)));
                foreach (string[] f in Read("bookings.txt"))
                {
                    Booking b = new Booking();
                    b.Id = f[0]; b.CheckInCode = f[1]; b.UserId = f[2]; b.ShowtimeId = f[3]; b.Payment = f[4];
                    b.Voucher = f[5]; b.ScannedBy = f[6];
                    if (f[7].Length > 0) b.Seats.AddRange(f[7].Split(','));
                    if (f[8].Length > 0)
                        foreach (string c in f[8].Split(','))
                        {
                            string[] kv = c.Split(':');
                            b.Combos[kv[0]] = int.Parse(kv[1], Inv);
                        }
                    b.TicketTotal = long.Parse(f[9], Inv); b.ComboTotal = long.Parse(f[10], Inv);
                    b.Discount = long.Parse(f[11], Inv); b.Total = long.Parse(f[12], Inv);
                    b.PointsUsed = int.Parse(f[13], Inv); b.PointsEarned = int.Parse(f[14], Inv);
                    b.Status = (BookingStatus)Enum.Parse(typeof(BookingStatus), f[15]);
                    b.CreatedAt = DateTime.Parse(f[16], Inv);
                    if (f[17].Length > 0) b.ScannedAt = DateTime.Parse(f[17], Inv);
                    Orders.Add(b);
                }
                List<string[]> meta = Read("meta.txt");
                if (meta.Count > 0) Seq = int.Parse(meta[0][0].Trim(), Inv);
                SyncSeats();
                return Movies.Count > 0 && Users.Count > 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("File dữ liệu lỗi, tạo lại dữ liệu mẫu: " + e);
                return false;
            }
        }

        public int NextSeq() { return ++Seq; }

        // ---------- tra cứu theo id ----------
        public User GetUser(string id) { return Users.FirstOrDefault(u => u.Id == id); }
        public Movie GetMovie(string id) { return Movies.FirstOrDefault(m => m.Id == id); }
        public Room GetRoom(string id) { return Rooms.FirstOrDefault(r => r.Id == id); }
        public Showtime GetShowtime(string id) { return Showtimes.FirstOrDefault(s => s.Id == id); }
        public Combo GetCombo(string id) { return Combos.FirstOrDefault(c => c.Id == id); }

        // ====================== DỮ LIỆU MẪU ======================
        private static class Seeder
        {
            public static void Seed(DataStore d)
            {
                d.Users.Add(new User("U1", "Admin Cinema", "0900000001", "quanly@cineluxe.vn", "123456", Role.QUAN_LY));
                User nv = new User("U2", "Trần Minh Quân", "0900000002", "nhanvien@cineluxe.vn", "123456", Role.NHAN_VIEN);
                nv.Code = "NV-8842";
                d.Users.Add(nv);
                User my = new User("U3", "Nguyễn Thảo My", "0908345678", "thaomy@gmail.com", "123456", Role.KHACH_HANG);
                my.Dob = "18/09/1998"; my.Points = 1250;
                d.Users.Add(my);
                d.Users.Add(new User("U4", "Lê Hoàng Nam", "0911222333", "hoangnam@gmail.com", "123456", Role.KHACH_HANG));
                d.Users.Add(new User("U5", "Phạm Đăng Khoa", "0922333444", "dangkhoa@gmail.com", "123456", Role.KHACH_HANG));

                DateTime today = DateTime.Today;
                d.Movies.Add(new Movie("MOV-8821", "Dune: Hành Tinh Cát 2", "Hành động, Khoa học viễn tưởng", "Denis Villeneuve",
                        166, "T16", "2D, IMAX Laser", 8.8, MovieStatus.DANG_CHIEU, today.AddDays(-60),
                        "Paul Atreides liên minh cùng Chani và tộc người Fremen khi anh tìm kiếm sự báo thù chống lại những kẻ đã hủy diệt gia tộc mình."));
                d.Movies.Add(new Movie("MOV-8910", "Godzilla x Kong: Đế Chế Mới", "Hành động, Phiêu lưu", "Adam Wingard",
                        115, "T13", "2D, 3D, Atmos", 6.4, MovieStatus.DANG_CHIEU, today.AddDays(-40),
                        "Hai quái thú huyền thoại cùng đối mặt một mối đe dọa khổng lồ ẩn sâu trong lòng Trái Đất."));
                d.Movies.Add(new Movie("MOV-8742", "Mai", "Tâm lý, Tình cảm", "Trấn Thành",
                        131, "T18", "2D, Dolby Atmos", 7.2, MovieStatus.DANG_CHIEU, today.AddDays(-80),
                        "Câu chuyện về người phụ nữ tên Mai và cuộc tình đầy giằng xé giữa quá khứ và hiện tại."));
                d.Movies.Add(new Movie("MOV-8755", "Kung Fu Panda 4", "Hoạt hình, Hài hước", "Mike Mitchell",
                        94, "P", "2D, Lồng tiếng", 6.9, MovieStatus.DANG_CHIEU, today.AddDays(-30),
                        "Po phải chọn người kế vị Thần Long Đại Hiệp và đối đầu với Tắc Kè Hoa."));
                d.Movies.Add(new Movie("MOV-8933", "Exhuma: Quật Mộ Trùng Ma", "Kinh dị, Bí ẩn", "Jang Jae-hyun",
                        134, "T18", "2D", 7.5, MovieStatus.DANG_CHIEU, today.AddDays(-20),
                        "Một nhóm thầy phong thủy và pháp sư được thuê để dời một ngôi mộ bị nguyền rủa."));
                d.Movies.Add(new Movie("MOV-9001", "Furiosa: Câu Chuyện Từ Mad Max", "Hành động, Hậu tận thế", "George Miller",
                        148, "T16", "2D, IMAX Laser", 7.6, MovieStatus.SAP_CHIEU, today.AddDays(12),
                        "Hành trình nguồn gốc của nữ chiến binh quả cảm Furiosa."));
                d.Movies.Add(new Movie("MOV-9002", "Deadpool & Wolverine", "Siêu anh hùng, Hài hước", "Shawn Levy",
                        128, "T18", "2D, 3D, IMAX Laser", 8.0, MovieStatus.SAP_CHIEU, today.AddDays(26),
                        "Sự kết hợp bùng nổ giữa hai nhân vật huyền thoại của vũ trụ Marvel."));

                d.GetMovie("MOV-8742").Vietnamese = true;

                d.Rooms.Add(new Room("R1", "Phòng 01", "IMAX Laser", "IMAX Laser • 75 ghế"));
                d.Rooms.Add(new Room("R2", "Phòng 02", "Dolby Atmos", "Dolby Atmos • Âm thanh vòm"));
                d.Rooms.Add(new Room("R3", "Phòng 03", "2D Digital", "2D Barco • Ghế tiêu chuẩn"));
                d.Rooms.Add(new Room("R4", "Phòng 04", "VIP Gold", "VIP Gold Class • Recliner"));

                d.Combos.Add(new Combo("C1", "Combo Solo", "1 Bắp lớn 60oz + 1 Coke lớn 32oz", 89000));
                d.Combos.Add(new Combo("C2", "Combo Couple", "1 Bắp khổng lồ caramel + 2 Nước ngọt", 129000));
                d.Combos.Add(new Combo("C3", "Combo Family", "2 Bắp lớn + 4 Nước ngọt + 1 Snack", 199000));

                // Suất chiếu: từ 7 ngày trước đến 6 ngày sau
                string[][] tpl = {
                    new[] {"R1", "MOV-8821", "09:00"}, new[] {"R1", "MOV-8821", "14:15"}, new[] {"R1", "MOV-8821", "18:30"}, new[] {"R1", "MOV-8821", "22:00"},
                    new[] {"R2", "MOV-8910", "09:15"}, new[] {"R2", "MOV-8742", "11:45"}, new[] {"R2", "MOV-8910", "14:30"},
                    new[] {"R2", "MOV-8910", "17:30"}, new[] {"R2", "MOV-8742", "20:00"},
                    new[] {"R3", "MOV-8742", "09:30"}, new[] {"R3", "MOV-8742", "14:00"}, new[] {"R3", "MOV-8933", "17:00"}, new[] {"R3", "MOV-8755", "20:00"},
                    new[] {"R4", "MOV-8821", "10:30"}, new[] {"R4", "MOV-8910", "15:00"}, new[] {"R4", "MOV-8821", "19:30"}};
                for (int off = -7; off <= 6; off++)
                {
                    foreach (string[] t in tpl)
                    {
                        Movie m = d.GetMovie(t[1]);
                        DateTime st = today.AddDays(off).Add(TimeSpan.Parse(t[2]));
                        d.Showtimes.Add(new Showtime("ST" + d.NextSeq(), m.Id, t[0], st, st.AddMinutes(m.Duration)));
                    }
                }
                SeedOrders(d);
            }

            private static void SeedOrders(DataStore d)
            {
                Random rnd = new Random(7);
                string[] pay = { "Ví MoMo", "Ví MoMo", "Ví MoMo", "Thẻ Visa/Master", "Thẻ Visa/Master", "ZaloPay" };
                List<User> customers = d.Users.Where(u => u.Role == Role.KHACH_HANG).ToList();
                DateTime now = DateTime.Now;
                List<string> allSeats = SeatMap.All();

                foreach (Showtime st in d.Showtimes.ToList())
                {
                    if (st.Start > now.AddDays(1) || rnd.Next(100) > 55) continue;
                    HashSet<string> used = new HashSet<string>();
                    int n = 1 + rnd.Next(3);
                    for (int i = 0; i < n; i++)
                    {
                        Booking o = new Booking();
                        int cnt = 1 + rnd.Next(3);
                        for (int k = 0; k < cnt; k++)
                        {
                            string s = allSeats[rnd.Next(allSeats.Count)];
                            if (used.Add(s)) { o.Seats.Add(s); o.TicketTotal += SeatMap.PriceOf(s); }
                        }
                        if (o.Seats.Count == 0) continue;
                        if (rnd.Next(100) < 60)
                        {
                            Combo c = d.Combos[rnd.Next(2)];
                            o.Combos[c.Id] = 1; o.ComboTotal = c.Price;
                        }
                        Fill(d, o, customers[rnd.Next(customers.Count)], st, pay[rnd.Next(pay.Length)]);
                        o.CreatedAt = st.Start.AddHours(-(2 + rnd.Next(20)));
                        if (st.Start < now)
                        {
                            int r = rnd.Next(100);
                            o.Status = r < 82 ? BookingStatus.DA_SOAT : r < 93 ? BookingStatus.DA_HUY : BookingStatus.DA_THANH_TOAN;
                            if (o.Status == BookingStatus.DA_SOAT) { o.ScannedAt = st.Start.AddMinutes(-10); o.ScannedBy = "NV-8842"; }
                        }
                        else if (rnd.Next(100) < 8) o.Status = BookingStatus.DA_HUY;
                        d.Orders.Add(o);
                    }
                }
                // Vé sắp chiếu của khách hàng mẫu (Nguyễn Thảo My): Dune - F07, F08
                User my = d.GetUser("U3");
                Showtime next = d.Showtimes
                        .Where(s => s.MovieId == "MOV-8821" && s.RoomId == "R1" && s.Start > now)
                        .OrderBy(s => s.Start).FirstOrDefault();
                if (next != null)
                {
                    d.Orders.RemoveAll(x => x.ShowtimeId == next.Id && (x.Seats.Contains("F07") || x.Seats.Contains("F08")));
                    Booking o = new Booking();
                    o.Seats.AddRange(new[] { "F07", "F08" }); o.TicketTotal = 240000;
                    o.Combos["C1"] = 1; o.ComboTotal = 89000;
                    Fill(d, o, my, next, "Ví MoMo");
                    o.CreatedAt = now.AddHours(-3);
                    d.Orders.Add(o);
                }
            }

            private static void Fill(DataStore d, Booking o, User u, Showtime st, string payment)
            {
                int seq = d.NextSeq();
                o.Id = "CNL-" + (50000 + seq);
                o.CheckInCode = "CLX-" + (900000 + seq);
                o.UserId = u.Id; o.ShowtimeId = st.Id; o.Payment = payment;
                o.Total = o.TicketTotal + o.ComboTotal;
                o.PointsEarned = (int)(o.Total / 10000);
            }
        }
    }
}
