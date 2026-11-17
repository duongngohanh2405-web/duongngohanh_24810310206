using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAL
{
    public class LuongDAL
    {
        // Dữ liệu chấm công + hồ sơ lương của cả tháng (kèm ThucLinh đã chốt nếu có)
        public List<LuongChiTietDTO> GetChiTiet(string thangNam)
        {
            var list = new List<LuongChiTietDTO>();
            using (var conn = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(
                @"SELECT c.MaNV, nv.HoTen, nv.LuongCoBan, nv.HeSoLuong, c.SoNgayCong, c.SoGioOT, bl.ThucLinh
                  FROM BangChamCong c
                  JOIN NhanVien nv ON nv.MaNV = c.MaNV
                  LEFT JOIN BangLuong bl ON bl.MaNV = c.MaNV AND bl.ThangNam = c.ThangNam
                  WHERE c.ThangNam = @t
                  ORDER BY c.MaNV", conn))
            {
                cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        list.Add(new LuongChiTietDTO
                        {
                            MaNV = rd.GetInt32(0),
                            HoTen = rd.GetString(1),
                            LuongCoBan = rd.GetDecimal(2),
                            HeSoLuong = rd.GetDouble(3),
                            SoNgayCong = rd.GetDouble(4),
                            SoGioOT = rd.GetDouble(5),
                            ThucLinhDaChot = rd.IsDBNull(6) ? (decimal?)null : rd.GetDecimal(6)
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Chốt lương trong MỘT SqlTransaction: ghi BangLuong cho mọi nhân viên có chấm công
        /// trong tháng, rồi khóa chấm công (DaKhoa = 1). Lỗi bất kỳ => Rollback toàn bộ.
        /// </summary>
        public int ChotLuong(string thangNam, Func<LuongChiTietDTO, decimal> tinhThucLinh)
        {
            using (var conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Kiểm tra tháng chưa chốt (khóa dòng để hai máy không chốt cùng lúc)
                        using (var cmd = new SqlCommand(
                            @"SELECT COUNT(*) FROM BangChamCong WITH (UPDLOCK, HOLDLOCK)
                              WHERE ThangNam = @t AND DaKhoa = 1", conn, trans))
                        {
                            cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                            if ((int)cmd.ExecuteScalar() > 0)
                                throw new Exception("Tháng " + thangNam + " đã được chốt lương trước đó.");
                        }

                        // 2. Đọc dữ liệu chấm công của tháng (đóng reader trước khi ghi)
                        var ds = new List<LuongChiTietDTO>();
                        using (var cmd = new SqlCommand(
                            @"SELECT c.MaNV, nv.HoTen, nv.LuongCoBan, nv.HeSoLuong, c.SoNgayCong, c.SoGioOT
                              FROM BangChamCong c
                              JOIN NhanVien nv ON nv.MaNV = c.MaNV
                              WHERE c.ThangNam = @t", conn, trans))
                        {
                            cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                            using (SqlDataReader rd = cmd.ExecuteReader())
                            {
                                while (rd.Read())
                                {
                                    ds.Add(new LuongChiTietDTO
                                    {
                                        MaNV = rd.GetInt32(0),
                                        HoTen = rd.GetString(1),
                                        LuongCoBan = rd.GetDecimal(2),
                                        HeSoLuong = rd.GetDouble(3),
                                        SoNgayCong = rd.GetDouble(4),
                                        SoGioOT = rd.GetDouble(5)
                                    });
                                }
                            }
                        }
                        if (ds.Count == 0)
                            throw new Exception("Tháng " + thangNam + " chưa có dữ liệu chấm công để chốt.");

                        // 3. Ghi bảng lương cho từng nhân viên
                        foreach (LuongChiTietDTO nv in ds)
                        {
                            using (var cmd = new SqlCommand(
                                "INSERT INTO BangLuong (MaNV, ThangNam, ThucLinh) VALUES (@ma, @t, @tl)", conn, trans))
                            {
                                cmd.Parameters.Add("@ma", SqlDbType.Int).Value = nv.MaNV;
                                cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                                cmd.Parameters.Add("@tl", SqlDbType.Decimal).Value = tinhThucLinh(nv);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 4. Khóa dữ liệu chấm công của tháng
                        using (var cmd = new SqlCommand(
                            "UPDATE BangChamCong SET DaKhoa = 1 WHERE ThangNam = @t", conn, trans))
                        {
                            cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                            cmd.ExecuteNonQuery();
                        }

                        // 5. Hoàn tất
                        trans.Commit();
                        return ds.Count;
                    }
                    catch
                    {
                        try { trans.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }
    }
}
