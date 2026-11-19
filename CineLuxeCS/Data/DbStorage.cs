using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CineLuxe.Models;
using MySqlConnector;

namespace CineLuxe.Data
{
    /// <summary>
    /// Lưu / đọc dữ liệu bằng MySQL (ADO.NET - thư viện MySqlConnector). Cấu hình nằm trong file db.properties.
    /// Dữ liệu vẫn được nạp vào các List của DataStore khi chạy; mỗi lần có thay đổi,
    /// toàn bộ dữ liệu được ghi lại vào MySQL trong một transaction (lỗi giữa chừng sẽ rollback).
    /// Cấu trúc bảng giống hệt bản Java nên dùng chung được cơ sở dữ liệu cũ.
    /// </summary>
    public class DbStorage
    {
        private const string Placeholder = "DIEN_MAT_KHAU_ROOT_VAO_DAY";
        private readonly string connStr, connStrNoDb;

        private DbStorage(string connStr, string connStrNoDb)
        {
            this.connStr = connStr; this.connStrNoDb = connStrNoDb;
        }

        /// <summary>Trả về DbStorage nếu db.properties bật MySQL và kết nối được; ngược lại trả về null (dùng file).</summary>
        public static DbStorage Connect()
        {
            string f = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db.properties");
            if (!File.Exists(f)) return null;
            Dictionary<string, string> p = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(f, Encoding.UTF8))
            {
                string t = line.Trim();
                if (t.Length == 0 || t.StartsWith("#")) continue;
                int i = t.IndexOf('=');
                if (i > 0) p[t.Substring(0, i).Trim()] = t.Substring(i + 1).Trim();
            }
            if (!p.ContainsKey("enabled") || p["enabled"].ToLowerInvariant() != "true") return null;

            string url = p.ContainsKey("url") ? p["url"] : "";
            string user = p.ContainsKey("user") ? p["user"] : "root";
            string pass = p.ContainsKey("password") ? p["password"] : "";

            // jdbc:mysql://host:port/database?... -> host, port, database
            string host = "localhost", port = "3306", db = "cineluxe";
            int s = url.IndexOf("//");
            if (s >= 0)
            {
                string rest = url.Substring(s + 2);
                int q = rest.IndexOf('?');
                if (q >= 0) rest = rest.Substring(0, q);
                int slash = rest.IndexOf('/');
                string hostPort = slash >= 0 ? rest.Substring(0, slash) : rest;
                if (slash >= 0 && slash + 1 < rest.Length) db = rest.Substring(slash + 1);
                int colon = hostPort.IndexOf(':');
                if (colon >= 0) { host = hostPort.Substring(0, colon); port = hostPort.Substring(colon + 1); }
                else if (hostPort.Length > 0) host = hostPort;
            }
            string common = "Server=" + host + ";Port=" + port + ";User ID=" + user + ";Password=" + pass
                    + ";CharSet=utf8mb4;SslMode=None;AllowPublicKeyRetrieval=true;";
            DbStorage storage = new DbStorage(common + "Database=" + db + ";", common);
            try
            {
                using (DbConnection c0 = new MySqlConnection(storage.connStrNoDb))   // tạo CSDL nếu chưa có
                {
                    c0.Open();
                    Exec(c0, null, "CREATE DATABASE IF NOT EXISTS `" + db + "` CHARACTER SET utf8mb4");
                }
                using (DbConnection c = storage.Open())
                {
                    storage.CreateTables(c);
                }
                Console.WriteLine("Đã kết nối MySQL: " + host + ":" + port + "/" + db);
                return storage;
            }
            catch (Exception e)
            {
                string hint = pass == Placeholder ? "\nBạn chưa điền mật khẩu root trong file db.properties." : "";
                MessageBox.Show("Không kết nối được MySQL nên chương trình sẽ lưu dữ liệu bằng file (thư mục data/).\n\nLý do: "
                        + e.Message + hint + "\n\nKiểm tra: MySQL đang chạy và mật khẩu trong db.properties đúng.",
                        "CineLuxury - MySQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private DbConnection Open()
        {
            DbConnection c = new MySqlConnection(connStr);
            c.Open();
            return c;
        }

        // ---------------- tiện ích ADO.NET ----------------
        private static int Exec(DbConnection c, DbTransaction t, string sql, params object[] args)
        {
            using (DbCommand cmd = c.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Transaction = t;
                for (int i = 0; i < args.Length; i++)
                {
                    DbParameter prm = cmd.CreateParameter();
                    prm.ParameterName = "@p" + i;
                    prm.Value = args[i] ?? DBNull.Value;
                    cmd.Parameters.Add(prm);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        private static void Insert(DbConnection c, DbTransaction t, string table, params object[] args)
        {
            string ph = string.Join(",", Enumerable.Range(0, args.Length).Select(i => "@p" + i));
            Exec(c, t, "INSERT INTO " + table + " VALUES (" + ph + ")", args);
        }

        private static string S(DbDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? "" : r.GetString(i);
        }
        private static int I(DbDataReader r, string col) { return Convert.ToInt32(r.GetValue(r.GetOrdinal(col))); }
        private static long L(DbDataReader r, string col) { return Convert.ToInt64(r.GetValue(r.GetOrdinal(col))); }
        private static double Dbl(DbDataReader r, string col) { int i = r.GetOrdinal(col); return r.IsDBNull(i) ? 0 : Convert.ToDouble(r.GetValue(i)); }
        private static bool Bool(DbDataReader r, string col) { return Convert.ToInt32(r.GetValue(r.GetOrdinal(col))) != 0; }
        private static DateTime Dt(DbDataReader r, string col) { return Convert.ToDateTime(r.GetValue(r.GetOrdinal(col))); }
        private static DateTime? DtN(DbDataReader r, string col)
        {
            int i = r.GetOrdinal(col);
            return r.IsDBNull(i) ? (DateTime?)null : Convert.ToDateTime(r.GetValue(i));
        }

        // ============================== TẠO BẢNG ==============================
        private void CreateTables(DbConnection c)
        {
            string[] ddl = {
                "CREATE TABLE IF NOT EXISTS users (id VARCHAR(20) PRIMARY KEY, full_name VARCHAR(100) NOT NULL, "
                    + "phone VARCHAR(15) NOT NULL UNIQUE, email VARCHAR(100) NOT NULL UNIQUE, password VARCHAR(100) NOT NULL, "
                    + "role VARCHAR(20) NOT NULL, dob VARCHAR(12), code VARCHAR(20), points INT NOT NULL DEFAULT 0)",
                "CREATE TABLE IF NOT EXISTS movies (id VARCHAR(20) PRIMARY KEY, title VARCHAR(200) NOT NULL, genre VARCHAR(100), "
                    + "director VARCHAR(100), duration INT NOT NULL, age VARCHAR(5), formats VARCHAR(100), imdb DOUBLE, "
                    + "status VARCHAR(20) NOT NULL, release_date DATE, vietnamese BOOLEAN NOT NULL DEFAULT 0, description TEXT)",
                "CREATE TABLE IF NOT EXISTS rooms (id VARCHAR(10) PRIMARY KEY, name VARCHAR(50) NOT NULL, "
                    + "room_format VARCHAR(50), info VARCHAR(100))",
                "CREATE TABLE IF NOT EXISTS showtimes (id VARCHAR(20) PRIMARY KEY, movie_id VARCHAR(20) NOT NULL, "
                    + "room_id VARCHAR(10) NOT NULL, start_time DATETIME NOT NULL, end_time DATETIME NOT NULL, "
                    + "FOREIGN KEY (movie_id) REFERENCES movies(id), FOREIGN KEY (room_id) REFERENCES rooms(id))",
                "CREATE TABLE IF NOT EXISTS combos (id VARCHAR(10) PRIMARY KEY, name VARCHAR(100) NOT NULL, "
                    + "description VARCHAR(255), price INT NOT NULL)",
                "CREATE TABLE IF NOT EXISTS bookings (id VARCHAR(20) PRIMARY KEY, check_in_code VARCHAR(20) NOT NULL UNIQUE, "
                    + "user_id VARCHAR(20) NOT NULL, showtime_id VARCHAR(20) NOT NULL, payment VARCHAR(50), voucher VARCHAR(30), "
                    + "scanned_by VARCHAR(50), ticket_total BIGINT, combo_total BIGINT, discount BIGINT, total BIGINT, "
                    + "points_used INT, points_earned INT, status VARCHAR(20) NOT NULL, created_at DATETIME NOT NULL, "
                    + "scanned_at DATETIME NULL, FOREIGN KEY (user_id) REFERENCES users(id), "
                    + "FOREIGN KEY (showtime_id) REFERENCES showtimes(id))",
                "CREATE TABLE IF NOT EXISTS booking_seats (booking_id VARCHAR(20) NOT NULL, seat VARCHAR(5) NOT NULL, "
                    + "PRIMARY KEY (booking_id, seat), FOREIGN KEY (booking_id) REFERENCES bookings(id))",
                "CREATE TABLE IF NOT EXISTS booking_combos (booking_id VARCHAR(20) NOT NULL, combo_id VARCHAR(10) NOT NULL, "
                    + "qty INT NOT NULL, PRIMARY KEY (booking_id, combo_id), FOREIGN KEY (booking_id) REFERENCES bookings(id), "
                    + "FOREIGN KEY (combo_id) REFERENCES combos(id))",
                "CREATE TABLE IF NOT EXISTS meta (k VARCHAR(20) PRIMARY KEY, v VARCHAR(50) NOT NULL)"
            };
            foreach (string s in ddl) Exec(c, null, s);
        }

        private static DbDataReader Query(DbConnection c, string sql)
        {
            DbCommand cmd = c.CreateCommand();
            cmd.CommandText = sql;
            return cmd.ExecuteReader();
        }

        // ============================== ĐỌC ==============================
        /// <summary>Nạp dữ liệu từ MySQL vào DataStore. Trả về false nếu CSDL còn trống (cần tạo dữ liệu mẫu).</summary>
        public bool Load(DataStore d)
        {
            try
            {
                using (DbConnection c = Open())
                {
                    using (DbDataReader r = Query(c, "SELECT * FROM users ORDER BY id"))
                        while (r.Read())
                        {
                            User u = new User(S(r, "id"), S(r, "full_name"), S(r, "phone"), S(r, "email"), S(r, "password"),
                                    (Role)Enum.Parse(typeof(Role), S(r, "role")));
                            u.Dob = S(r, "dob"); u.Code = S(r, "code"); u.Points = I(r, "points");
                            d.Users.Add(u);
                        }
                    using (DbDataReader r = Query(c, "SELECT * FROM movies ORDER BY id"))
                        while (r.Read())
                        {
                            DateTime? rel = DtN(r, "release_date");
                            Movie m = new Movie(S(r, "id"), S(r, "title"), S(r, "genre"), S(r, "director"), I(r, "duration"),
                                    S(r, "age"), S(r, "formats"), Dbl(r, "imdb"),
                                    (MovieStatus)Enum.Parse(typeof(MovieStatus), S(r, "status")),
                                    rel ?? DateTime.Today, S(r, "description"));
                            m.Vietnamese = Bool(r, "vietnamese");
                            d.Movies.Add(m);
                        }
                    using (DbDataReader r = Query(c, "SELECT * FROM rooms ORDER BY id"))
                        while (r.Read()) d.Rooms.Add(new Room(S(r, "id"), S(r, "name"), S(r, "room_format"), S(r, "info")));
                    using (DbDataReader r = Query(c, "SELECT * FROM showtimes ORDER BY start_time, id"))
                        while (r.Read())
                            d.Showtimes.Add(new Showtime(S(r, "id"), S(r, "movie_id"), S(r, "room_id"), Dt(r, "start_time"), Dt(r, "end_time")));
                    using (DbDataReader r = Query(c, "SELECT * FROM combos ORDER BY id"))
                        while (r.Read()) d.Combos.Add(new Combo(S(r, "id"), S(r, "name"), S(r, "description"), I(r, "price")));

                    Dictionary<string, Booking> byId = new Dictionary<string, Booking>();
                    List<Booking> ordered = new List<Booking>();
                    using (DbDataReader r = Query(c, "SELECT * FROM bookings ORDER BY created_at, id"))
                        while (r.Read())
                        {
                            Booking b = new Booking();
                            b.Id = S(r, "id"); b.CheckInCode = S(r, "check_in_code"); b.UserId = S(r, "user_id");
                            b.ShowtimeId = S(r, "showtime_id"); b.Payment = S(r, "payment"); b.Voucher = S(r, "voucher");
                            b.ScannedBy = S(r, "scanned_by"); b.TicketTotal = L(r, "ticket_total"); b.ComboTotal = L(r, "combo_total");
                            b.Discount = L(r, "discount"); b.Total = L(r, "total"); b.PointsUsed = I(r, "points_used");
                            b.PointsEarned = I(r, "points_earned");
                            b.Status = (BookingStatus)Enum.Parse(typeof(BookingStatus), S(r, "status"));
                            b.CreatedAt = Dt(r, "created_at");
                            b.ScannedAt = DtN(r, "scanned_at");
                            byId[b.Id] = b; ordered.Add(b);
                        }
                    using (DbDataReader r = Query(c, "SELECT booking_id, seat FROM booking_seats ORDER BY booking_id, seat"))
                        while (r.Read())
                        {
                            Booking b;
                            if (byId.TryGetValue(r.GetString(0), out b)) b.Seats.Add(r.GetString(1));
                        }
                    using (DbDataReader r = Query(c, "SELECT booking_id, combo_id, qty FROM booking_combos ORDER BY booking_id, combo_id"))
                        while (r.Read())
                        {
                            Booking b;
                            if (byId.TryGetValue(r.GetString(0), out b)) b.Combos[r.GetString(1)] = Convert.ToInt32(r.GetValue(2));
                        }
                    d.Orders.AddRange(ordered);
                    using (DbDataReader r = Query(c, "SELECT v FROM meta WHERE k = 'seq'"))
                        if (r.Read()) d.Seq = int.Parse(r.GetString(0));
                }
                d.SyncSeats();
                return d.Movies.Count > 0 && d.Users.Count > 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Không đọc được dữ liệu từ MySQL: " + e);
                d.Users.Clear(); d.Movies.Clear(); d.Rooms.Clear(); d.Showtimes.Clear(); d.Combos.Clear(); d.Orders.Clear();
                return false;
            }
        }

        // ============================== GHI ==============================
        /// <summary>Ghi toàn bộ dữ liệu vào MySQL (xóa cũ - ghi mới trong một transaction).</summary>
        public void Save(DataStore d)
        {
            try
            {
                using (DbConnection c = Open())
                using (DbTransaction tx = c.BeginTransaction())
                {
                    try
                    {
                        // xóa bảng con trước, bảng cha sau
                        foreach (string t in new[] { "booking_combos", "booking_seats", "bookings", "showtimes", "movies", "rooms", "combos", "users", "meta" })
                            Exec(c, tx, "DELETE FROM " + t);

                        foreach (User u in d.Users)
                            Insert(c, tx, "users", u.Id, u.FullName, u.Phone, u.Email, u.Password, u.Role.ToString(), u.Dob, u.Code, u.Points);
                        foreach (Movie m in d.Movies)
                            Insert(c, tx, "movies", m.Id, m.Title, m.Genre, m.Director, m.Duration, m.Age, m.Formats, m.Imdb,
                                    m.Status.ToString(), m.ReleaseDate.Date, m.Vietnamese ? 1 : 0, m.Description);
                        foreach (Room r in d.Rooms) Insert(c, tx, "rooms", r.Id, r.Name, r.Format, r.Info);
                        foreach (Combo cb in d.Combos) Insert(c, tx, "combos", cb.Id, cb.Name, cb.Desc, cb.Price);
                        foreach (Showtime s in d.Showtimes) Insert(c, tx, "showtimes", s.Id, s.MovieId, s.RoomId, s.Start, s.End);
                        foreach (Booking b in d.Orders)
                        {
                            Insert(c, tx, "bookings", b.Id, b.CheckInCode, b.UserId, b.ShowtimeId, b.Payment, b.Voucher, b.ScannedBy,
                                    b.TicketTotal, b.ComboTotal, b.Discount, b.Total, b.PointsUsed, b.PointsEarned,
                                    b.Status.ToString(), b.CreatedAt, b.ScannedAt.HasValue ? (object)b.ScannedAt.Value : null);
                            foreach (string seat in b.Seats) Insert(c, tx, "booking_seats", b.Id, seat);
                            foreach (KeyValuePair<string, int> e in b.Combos) Insert(c, tx, "booking_combos", b.Id, e.Key, e.Value);
                        }
                        Insert(c, tx, "meta", "seq", d.Seq.ToString());
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Không ghi được dữ liệu vào MySQL: " + e.Message);
            }
        }
    }
}
