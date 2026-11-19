using System;
using Microsoft.Data.SqlClient;

namespace KaraokeManager.DAL
{
    public static class DbHelper
    {
        // Sửa Server cho đúng máy của bạn (vd: .\SQLEXPRESS, (localdb)\MSSQLLocalDB, localhost)
        public static string ConnectionString =
            @"Server=.\SQLEXPRESS;Database=KaraokeDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);

        public static SqlParameter P(string name, object value) => new SqlParameter(name, value ?? DBNull.Value);

        public static void SafeRollback(SqlTransaction t) { try { t.Rollback(); } catch { } }

        public static bool IsDuplicate(SqlException ex) => ex.Number == 2627 || ex.Number == 2601;
    }
}
