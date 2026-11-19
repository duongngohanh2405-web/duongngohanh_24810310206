using Microsoft.Data.SqlClient;

namespace ProjectTaskManager.DAL
{
    public static class DbHelper
    {
        // Sửa lại Server cho đúng máy của bạn (vd: .\SQLEXPRESS, (localdb)\MSSQLLocalDB, localhost)
        public static string ConnectionString =
            @"Server=.\SQLEXPRESS;Database=ProjectTaskDB;Trusted_Connection=True;TrustServerCertificate=True;";

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

        // Tham số cho phép null -> DBNull
        public static SqlParameter P(string name, object value) =>
            new SqlParameter(name, value ?? System.DBNull.Value);
    }
}
