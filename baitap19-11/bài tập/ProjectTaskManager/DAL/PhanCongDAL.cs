using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using static ProjectTaskManager.DAL.DbHelper;

namespace ProjectTaskManager.DAL
{
    public class PhanCongDAL
    {
        /// <summary>Giao 1 task cho nhiều thành viên trong 1 transaction (bỏ qua người đã được giao).</summary>
        public int GiaoViec(int maTask, IEnumerable<int> dsThanhVien)
        {
            int count = 0;
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                foreach (int tv in dsThanhVien)
                {
                    using var cmd = new SqlCommand(
                        @"IF NOT EXISTS (SELECT 1 FROM PhanCong WHERE MaTask=@t AND MaThanhVien=@tv)
                              INSERT PhanCong (MaTask, MaThanhVien) VALUES (@t, @tv)", cn, tran);
                    cmd.Parameters.AddWithValue("@t", maTask);
                    cmd.Parameters.AddWithValue("@tv", tv);
                    count += cmd.ExecuteNonQuery() > 0 ? 1 : 0;
                }
                tran.Commit();
                return count;
            }
            catch { tran.Rollback(); throw; }
        }

        public int HuyPhanCong(int maTask, int maThanhVien) => ExecuteNonQuery(
            "DELETE FROM PhanCong WHERE MaTask=@t AND MaThanhVien=@tv", P("@t", maTask), P("@tv", maThanhVien));

        public List<int> GetMaThanhVienCuaTask(int maTask)
        {
            var list = new List<int>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand("SELECT MaThanhVien FROM PhanCong WHERE MaTask=@t", cn);
            cmd.Parameters.AddWithValue("@t", maTask);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(rd.GetInt32(0));
            return list;
        }
    }
}
