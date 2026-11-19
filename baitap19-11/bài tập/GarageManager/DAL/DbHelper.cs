using Microsoft.Data.SqlClient;

namespace GarageManager.DAL
{
    public static class DbHelper
    {
        // Sửa Server cho đúng máy của bạn (vd: .\SQLEXPRESS, (localdb)\MSSQLLocalDB, localhost)
        public static string ConnectionString =
            @"Server=.\SQLEXPRESS;Database=GarageDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);

        public static int ExecuteNonQuery(string sql, params SqlParameter[] ps)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddRange(ps);
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] ps)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddRange(ps);
            cn.Open();
            return cmd.ExecuteScalar();
        }

        public static SqlParameter P(string name, object value) =>
            new SqlParameter(name, value ?? System.DBNull.Value);

        // Mã lỗi SQL Server: 2627/2601 = trùng khóa/unique, 547 = vi phạm khóa ngoại
        public static bool IsDuplicate(SqlException ex) => ex.Number == 2627 || ex.Number == 2601;
        public static bool IsForeignKey(SqlException ex) => ex.Number == 547;
    }
}
