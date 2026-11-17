using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace RetailSales.DAL
{
    public static class DBHelper
    {
        // Sửa chuỗi này nếu SQL Server của bạn là .\SQLEXPRESS hoặc LocalDB
        private const string DefaultConnStr =
            "Server=.;Database=SalesDB;Integrated Security=True;TrustServerCertificate=True;";

        public static string ConnStr
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("RETAIL_CONN");
                return string.IsNullOrWhiteSpace(env) ? DefaultConnStr : env;
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

        // Đổi lỗi SQL thành thông báo tiếng Việt dễ hiểu
        public static string FriendlyError(Exception ex)
        {
            var sql = ex as SqlException;
            if (sql != null)
            {
                switch (sql.Number)
                {
                    case 2627:
                    case 2601: return "Dữ liệu bị trùng (mã hoặc số điện thoại đã tồn tại).";
                    case 547: return "Dữ liệu đang được sử dụng ở nơi khác hoặc vi phạm ràng buộc, không thể thực hiện.";
                    case -1:
                    case 2:
                    case 53:
                    case 18456: return "Không kết nối được SQL Server. Kiểm tra chuỗi kết nối trong DAL/DBHelper.cs.\n\n" + sql.Message;
                    case 4060: return "Chưa có CSDL SalesDB. Hãy chạy file Database/SalesDB.sql trong SSMS.\n\n" + sql.Message;
                }
            }
            return ex.Message;
        }
    }
}
