using System;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace HRM.DAL
{
    public static class DBHelper
    {
        private const string Database = "HRMDB";

        // Các server thường gặp, chương trình tự thử lần lượt
        private static readonly string[] Servers =
        {
            ".", @".\SQLEXPRESS", @"(localdb)\MSSQLLocalDB", @"localhost\SQLEXPRESS", "localhost"
        };

        private static string resolved;

        private static string Build(string server, string db)
        {
            return "Server=" + server + ";Database=" + db +
                   ";Integrated Security=True;TrustServerCertificate=True;Connect Timeout=2;";
        }

        public static string ConnStr
        {
            get
            {
                if (resolved != null) return resolved;

                // 1) Biến môi trường HRM_CONN
                string env = Environment.GetEnvironmentVariable("HRM_CONN");
                if (!string.IsNullOrWhiteSpace(env)) return resolved = env;

                // 2) File connection.txt cạnh file .exe (bỏ qua dòng bắt đầu bằng #)
                string file = Path.Combine(AppContext.BaseDirectory, "connection.txt");
                if (File.Exists(file))
                {
                    string line = File.ReadAllLines(file)
                        .Select(l => l.Trim())
                        .FirstOrDefault(l => l.Length > 0 && !l.StartsWith("#"));
                    if (line != null) return resolved = line;
                }

                // 3) Tự dò: lượt 1 thử kết nối thẳng HRMDB, lượt 2 thử master (để báo "chưa có CSDL")
                foreach (string db in new[] { Database, "master" })
                {
                    foreach (string s in Servers)
                    {
                        try
                        {
                            using (var c = new SqlConnection(Build(s, db)))
                            {
                                c.Open();
                            }
                            return resolved = Build(s, Database);
                        }
                        catch (SqlException) { }
                    }
                }

                throw new Exception(
                    "Không tìm thấy SQL Server trên máy này.\n" +
                    "Hãy mở file connection.txt (cạnh file HRM.exe) và nhập chuỗi kết nối với đúng tên server trong SSMS.");
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnStr);
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] ps)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                var dt = new DataTable();
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
                return dt;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] ps)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (ps != null) cmd.Parameters.AddRange(ps);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();
        }

        public static string FriendlyError(Exception ex)
        {
            var sql = ex as SqlException;
            if (sql != null)
            {
                switch (sql.Number)
                {
                    case 2627:
                    case 2601: return "Dữ liệu bị trùng (CCCD hoặc bản ghi đã tồn tại).";
                    case 547: return "Dữ liệu đang được sử dụng ở nơi khác hoặc vi phạm ràng buộc, không thể thực hiện.";
                    case 4060: return "Chưa có CSDL HRMDB. Hãy chạy file Database/HRMDB.sql trong SSMS.";
                    case -1:
                    case 2:
                    case 53:
                    case 18456: return "Không kết nối được SQL Server. Kiểm tra file connection.txt.\n\n" + sql.Message;
                }
            }
            return ex.Message;
        }
    }
}
