using System.Data;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class NhanVienDAL
    {
        public DataTable GetAll()
        {
            return DBHelper.ExecuteQuery(
                "SELECT MaNV, TenNV, SoDienThoai, ChucVu FROM NhanVien ORDER BY TenNV");
        }

        public bool Insert(NhanVienDTO nv)
        {
            return DBHelper.ExecuteNonQuery(
                "INSERT INTO NhanVien (TenNV, SoDienThoai, ChucVu) VALUES (@ten, @sdt, @cv)",
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = nv.TenNV },
                new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = DBHelper.NullIfEmpty(nv.SoDienThoai) },
                new SqlParameter("@cv", SqlDbType.NVarChar, 50) { Value = DBHelper.NullIfEmpty(nv.ChucVu) }) > 0;
        }

        public bool Update(NhanVienDTO nv)
        {
            return DBHelper.ExecuteNonQuery(
                "UPDATE NhanVien SET TenNV=@ten, SoDienThoai=@sdt, ChucVu=@cv WHERE MaNV=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = nv.MaNV },
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = nv.TenNV },
                new SqlParameter("@sdt", SqlDbType.VarChar, 15) { Value = DBHelper.NullIfEmpty(nv.SoDienThoai) },
                new SqlParameter("@cv", SqlDbType.NVarChar, 50) { Value = DBHelper.NullIfEmpty(nv.ChucVu) }) > 0;
        }

        public bool Delete(int maNV)
        {
            return DBHelper.ExecuteNonQuery("DELETE FROM NhanVien WHERE MaNV=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = maNV }) > 0;
        }
    }
}
