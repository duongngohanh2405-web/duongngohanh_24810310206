using System.Data;
using Microsoft.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAL
{
    public class PhongBanDAL
    {
        public DataTable GetAll()
        {
            return DBHelper.ExecuteQuery("SELECT MaPB, TenPB, Mota FROM PhongBan ORDER BY TenPB");
        }

        public bool Insert(PhongBanDTO pb)
        {
            return DBHelper.ExecuteNonQuery(
                "INSERT INTO PhongBan (TenPB, Mota) VALUES (@ten, @mt)",
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = pb.TenPB },
                new SqlParameter("@mt", SqlDbType.NVarChar, 255) { Value = DBHelper.NullIfEmpty(pb.Mota) }) > 0;
        }

        public bool Update(PhongBanDTO pb)
        {
            return DBHelper.ExecuteNonQuery(
                "UPDATE PhongBan SET TenPB=@ten, Mota=@mt WHERE MaPB=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = pb.MaPB },
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = pb.TenPB },
                new SqlParameter("@mt", SqlDbType.NVarChar, 255) { Value = DBHelper.NullIfEmpty(pb.Mota) }) > 0;
        }

        public bool Delete(int maPB)
        {
            return DBHelper.ExecuteNonQuery("DELETE FROM PhongBan WHERE MaPB=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = maPB }) > 0;
        }
    }
}
