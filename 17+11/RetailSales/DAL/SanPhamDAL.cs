using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class SanPhamDAL
    {
        public DataTable GetAll()
        {
            return DBHelper.ExecuteQuery(
                "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham ORDER BY TenSP");
        }

        public List<SanPhamDTO> GetList()
        {
            var list = new List<SanPhamDTO>();
            foreach (DataRow r in GetAll().Rows)
            {
                list.Add(new SanPhamDTO
                {
                    MaSP = r["MaSP"].ToString(),
                    TenSP = r["TenSP"].ToString(),
                    GiaBan = (decimal)r["GiaBan"],
                    SoLuongTon = (int)r["SoLuongTon"]
                });
            }
            return list;
        }

        // Tìm theo tên hoặc mã vạch, dùng SqlParameter với ký tự đại diện %
        public DataTable Search(string keyword)
        {
            string sql = @"SELECT MaSP, TenSP, GiaBan, SoLuongTon
                           FROM SanPham
                           WHERE TenSP LIKE @kw OR MaSP LIKE @kw
                           ORDER BY TenSP";
            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@kw", SqlDbType.NVarChar, 150) { Value = "%" + keyword + "%" });
        }

        public SanPhamDTO GetById(string maSP)
        {
            var dt = DBHelper.ExecuteQuery(
                "SELECT MaSP, TenSP, GiaBan, SoLuongTon FROM SanPham WHERE MaSP = @ma",
                new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = maSP });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new SanPhamDTO
            {
                MaSP = r["MaSP"].ToString(),
                TenSP = r["TenSP"].ToString(),
                GiaBan = (decimal)r["GiaBan"],
                SoLuongTon = (int)r["SoLuongTon"]
            };
        }

        public bool Insert(SanPhamDTO sp)
        {
            string sql = "INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES (@ma, @ten, @gia, @ton)";
            return DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = sp.MaSP },
                new SqlParameter("@ten", SqlDbType.NVarChar, 150) { Value = sp.TenSP },
                new SqlParameter("@gia", SqlDbType.Decimal) { Value = sp.GiaBan },
                new SqlParameter("@ton", SqlDbType.Int) { Value = sp.SoLuongTon }) > 0;
        }

        public bool Update(SanPhamDTO sp)
        {
            string sql = "UPDATE SanPham SET TenSP=@ten, GiaBan=@gia, SoLuongTon=@ton WHERE MaSP=@ma";
            return DBHelper.ExecuteNonQuery(sql,
                new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = sp.MaSP },
                new SqlParameter("@ten", SqlDbType.NVarChar, 150) { Value = sp.TenSP },
                new SqlParameter("@gia", SqlDbType.Decimal) { Value = sp.GiaBan },
                new SqlParameter("@ton", SqlDbType.Int) { Value = sp.SoLuongTon }) > 0;
        }

        public bool Delete(string maSP)
        {
            return DBHelper.ExecuteNonQuery("DELETE FROM SanPham WHERE MaSP = @ma",
                new SqlParameter("@ma", SqlDbType.VarChar, 20) { Value = maSP }) > 0;
        }
    }
}
