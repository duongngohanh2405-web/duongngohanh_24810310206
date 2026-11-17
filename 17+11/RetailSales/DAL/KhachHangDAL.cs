using System.Data;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class KhachHangDAL
    {
        public DataTable GetAll()
        {
            return DBHelper.ExecuteQuery(
                "SELECT MaKH, TenKH, DienThoai, DiemTichLuy FROM KhachHang ORDER BY TenKH");
        }

        public bool Insert(KhachHangDTO kh)
        {
            return DBHelper.ExecuteNonQuery(
                "INSERT INTO KhachHang (TenKH, DienThoai) VALUES (@ten, @sdt)",
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = kh.TenKH },
                new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = DBHelper.NullIfEmpty(kh.DienThoai) }) > 0;
        }

        public bool Update(KhachHangDTO kh)
        {
            return DBHelper.ExecuteNonQuery(
                "UPDATE KhachHang SET TenKH=@ten, DienThoai=@sdt WHERE MaKH=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = kh.MaKH },
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = kh.TenKH },
                new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = DBHelper.NullIfEmpty(kh.DienThoai) }) > 0;
        }

        public bool Delete(int maKH)
        {
            return DBHelper.ExecuteNonQuery("DELETE FROM KhachHang WHERE MaKH=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = maKH }) > 0;
        }
    }
}
