using System;
using System.Data;
using Microsoft.Data.SqlClient;
using HRM.DTO;

namespace HRM.DAL
{
    public class NhanVienDAL
    {
        // Tìm theo tên / CCCD / tên phòng ban (một ô từ khóa) và lọc thêm theo mã phòng ban
        public DataTable Search(string keyword, int? maPB)
        {
            string sql = @"SELECT nv.MaNV, nv.HoTen, nv.NgaySinh, nv.CCCD, nv.MaPB, pb.TenPB,
                                  nv.LuongCoBan, nv.HeSoLuong
                           FROM NhanVien nv
                           LEFT JOIN PhongBan pb ON pb.MaPB = nv.MaPB
                           WHERE (nv.HoTen LIKE @kw
                                  OR ISNULL(nv.CCCD, '') LIKE @kw
                                  OR ISNULL(pb.TenPB, N'') LIKE @kw)
                             AND (@mapb IS NULL OR nv.MaPB = @mapb)
                           ORDER BY nv.MaNV";
            return DBHelper.ExecuteQuery(sql,
                new SqlParameter("@kw", SqlDbType.NVarChar, 150) { Value = "%" + (keyword ?? "").Trim() + "%" },
                new SqlParameter("@mapb", SqlDbType.Int) { Value = maPB.HasValue ? (object)maPB.Value : DBNull.Value });
        }

        private static SqlParameter[] ThamSo(NhanVienDTO nv, bool coMa)
        {
            var list = new System.Collections.Generic.List<SqlParameter>
            {
                new SqlParameter("@ten", SqlDbType.NVarChar, 100) { Value = nv.HoTen },
                new SqlParameter("@ns", SqlDbType.DateTime) { Value = nv.NgaySinh.HasValue ? (object)nv.NgaySinh.Value : DBNull.Value },
                new SqlParameter("@cccd", SqlDbType.VarChar, 20) { Value = DBHelper.NullIfEmpty(nv.CCCD) },
                new SqlParameter("@mapb", SqlDbType.Int) { Value = nv.MaPB },
                new SqlParameter("@luong", SqlDbType.Decimal) { Value = nv.LuongCoBan },
                new SqlParameter("@hs", SqlDbType.Float) { Value = nv.HeSoLuong }
            };
            if (coMa) list.Add(new SqlParameter("@ma", SqlDbType.Int) { Value = nv.MaNV });
            return list.ToArray();
        }

        public bool Insert(NhanVienDTO nv)
        {
            return DBHelper.ExecuteNonQuery(
                @"INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong)
                  VALUES (@ten, @ns, @cccd, @mapb, @luong, @hs)", ThamSo(nv, false)) > 0;
        }

        public bool Update(NhanVienDTO nv)
        {
            return DBHelper.ExecuteNonQuery(
                @"UPDATE NhanVien SET HoTen=@ten, NgaySinh=@ns, CCCD=@cccd, MaPB=@mapb,
                                      LuongCoBan=@luong, HeSoLuong=@hs
                  WHERE MaNV=@ma", ThamSo(nv, true)) > 0;
        }

        public bool ChuyenPhongBan(int maNV, int maPB)
        {
            return DBHelper.ExecuteNonQuery(
                "UPDATE NhanVien SET MaPB=@mapb WHERE MaNV=@ma",
                new SqlParameter("@ma", SqlDbType.Int) { Value = maNV },
                new SqlParameter("@mapb", SqlDbType.Int) { Value = maPB }) > 0;
        }

        // Xóa nhân viên: không cho xóa nếu đã có bảng lương; các dòng chấm công chưa chốt bị xóa theo
        public bool Delete(int maNV)
        {
            using (var conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM BangLuong WHERE MaNV=@ma", conn, trans))
                        {
                            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
                            if ((int)cmd.ExecuteScalar() > 0)
                                throw new Exception("Nhân viên này đã có bảng lương, không thể xóa.");
                        }
                        using (var cmd = new SqlCommand("DELETE FROM BangChamCong WHERE MaNV=@ma", conn, trans))
                        {
                            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
                            cmd.ExecuteNonQuery();
                        }
                        int n;
                        using (var cmd = new SqlCommand("DELETE FROM NhanVien WHERE MaNV=@ma", conn, trans))
                        {
                            cmd.Parameters.Add("@ma", SqlDbType.Int).Value = maNV;
                            n = cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                        return n > 0;
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
