using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class ThongKeDAL
    {
        private decimal DoanhThu(DateTime tu, DateTime den)
        {
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(
                @"SELECT ISNULL(SUM(TongTien), 0) FROM HoaDon
                  WHERE NgayLap >= @tu AND NgayLap < @den", conn))
            {
                cmd.Parameters.Add("@tu", SqlDbType.DateTime).Value = tu;
                cmd.Parameters.Add("@den", SqlDbType.DateTime).Value = den;
                conn.Open();
                return Convert.ToDecimal(cmd.ExecuteScalar());   // ExecuteScalar
            }
        }

        public decimal DoanhThuNgay(DateTime ngay)
        {
            return DoanhThu(ngay.Date, ngay.Date.AddDays(1));
        }

        public decimal DoanhThuThang(int thang, int nam)
        {
            var dau = new DateTime(nam, thang, 1);
            return DoanhThu(dau, dau.AddMonths(1));
        }

        // Top 5 sản phẩm bán chạy: dùng SqlDataReader
        public List<ThongKeSanPhamDTO> Top5BanChay()
        {
            var list = new List<ThongKeSanPhamDTO>();
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(
                @"SELECT TOP 5 sp.MaSP, sp.TenSP, SUM(ct.SoLuong) AS TongBan
                  FROM ChiTietHoaDon ct
                  JOIN SanPham sp ON sp.MaSP = ct.MaSP
                  GROUP BY sp.MaSP, sp.TenSP
                  ORDER BY TongBan DESC", conn))
            {
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new ThongKeSanPhamDTO
                        {
                            MaSP = rd.GetString(0),
                            TenSP = rd.GetString(1),
                            SoLuongBan = rd.GetInt32(2)
                        });
                    }
                }
            }
            return list;
        }
    }
}
