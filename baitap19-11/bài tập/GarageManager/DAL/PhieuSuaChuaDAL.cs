using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;
using static GarageManager.DAL.DbHelper;

namespace GarageManager.DAL
{
    public class PhieuSuaChuaDAL
    {
        private static PhieuSuaChuaDTO Map(SqlDataReader rd) => new PhieuSuaChuaDTO
        {
            MaPhieu = rd.GetInt32(0),
            BienSo = rd.GetString(1),
            NgayNhan = rd.GetDateTime(2),
            NoiDungSua = rd.IsDBNull(3) ? null : rd.GetString(3),
            TongTien = rd.GetDecimal(4),
            TrangThai = rd.GetString(5),
            TenKH = rd.GetString(6),
            DienThoai = rd.IsDBNull(7) ? null : rd.GetString(7),
            HieuXe = rd.IsDBNull(8) ? null : rd.GetString(8)
        };

        private const string SelectBase = @"
            SELECT p.MaPhieu, p.BienSo, p.NgayNhan, p.NoiDungSua, p.TongTien, p.TrangThai,
                   k.TenKH, k.DienThoai, x.HieuXe
            FROM PhieuSuaChua p
            JOIN Xe x ON x.BienSo = p.BienSo
            JOIN KhachHang k ON k.MaKH = x.MaKH ";

        public List<PhieuSuaChuaDTO> GetAll(string trangThai)
        {
            var list = new List<PhieuSuaChuaDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(SelectBase + "WHERE (@tt IS NULL OR p.TrangThai=@tt) ORDER BY p.NgayNhan DESC", cn);
            cmd.Parameters.Add(P("@tt", trangThai));
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(Map(rd));
            return list;
        }

        public PhieuSuaChuaDTO GetById(int maPhieu)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(SelectBase + "WHERE p.MaPhieu=@ma", cn);
            cmd.Parameters.Add(P("@ma", maPhieu));
            cn.Open();
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        public List<ChiTietSuaChuaDTO> GetChiTiet(int maPhieu)
        {
            var list = new List<ChiTietSuaChuaDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT ct.MaPhieu, ct.MaPhuTung, pt.TenPhuTung, pt.DonGia, ct.SoLuong, ct.TienCong
                FROM ChiTietSuaChua ct JOIN PhuTung pt ON pt.MaPhuTung = ct.MaPhuTung
                WHERE ct.MaPhieu = @ma ORDER BY pt.TenPhuTung", cn);
            cmd.Parameters.Add(P("@ma", maPhieu));
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new ChiTietSuaChuaDTO
                {
                    MaPhieu = rd.GetInt32(0),
                    MaPhuTung = rd.GetInt32(1),
                    TenPhuTung = rd.GetString(2),
                    DonGia = rd.GetDecimal(3),
                    SoLuong = rd.GetInt32(4),
                    TienCong = rd.GetDecimal(5)
                });
            return list;
        }

        /// <summary>
        /// Xác nhận hoàn tất phiếu - MỘT SqlTransaction gồm:
        ///   1) tạo PhieuSuaChua
        ///   2) tạo các ChiTietSuaChua
        ///   3) trừ kho: SoLuongTon -= SoLuong (điều kiện SoLuongTon >= SoLuong nằm ngay trong UPDATE
        ///      nên kiểm tra + trừ là một thao tác nguyên tử, không bị 2 người cùng trừ vượt kho)
        ///   4) thiếu hàng -> KhoKhongDuException -> Rollback toàn bộ
        ///   5) cập nhật TongTien rồi Commit
        /// </summary>
        public int LapPhieuHoanTat(string bienSo, string noiDung, List<ChiTietSuaChuaDTO> chiTiet)
        {
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                // 1. Phiếu
                int maPhieu;
                using (var cmd = new SqlCommand(
                    @"INSERT PhieuSuaChua (BienSo, NoiDungSua, TongTien, TrangThai)
                      VALUES (@bs, @nd, 0, @tt);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tran))
                {
                    cmd.Parameters.Add(P("@bs", bienSo));
                    cmd.Parameters.Add(P("@nd", noiDung));
                    cmd.Parameters.Add(P("@tt", TrangThaiPhieu.HoanThanh));
                    maPhieu = (int)cmd.ExecuteScalar();
                }

                foreach (var c in chiTiet)
                {
                    // 2. Chi tiết
                    using (var cmd = new SqlCommand(
                        "INSERT ChiTietSuaChua (MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES (@p, @pt, @sl, @cong)", cn, tran))
                    {
                        cmd.Parameters.Add(P("@p", maPhieu));
                        cmd.Parameters.Add(P("@pt", c.MaPhuTung));
                        cmd.Parameters.Add(P("@sl", c.SoLuong));
                        cmd.Parameters.Add(P("@cong", c.TienCong));
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Trừ kho (nguyên tử)
                    int affected;
                    using (var cmd = new SqlCommand(
                        @"UPDATE PhuTung SET SoLuongTon = SoLuongTon - @sl
                          WHERE MaPhuTung = @pt AND SoLuongTon >= @sl", cn, tran))
                    {
                        cmd.Parameters.Add(P("@sl", c.SoLuong));
                        cmd.Parameters.Add(P("@pt", c.MaPhuTung));
                        affected = cmd.ExecuteNonQuery();
                    }

                    // 4. Không đủ hàng
                    if (affected == 0)
                    {
                        string ten = "(không rõ)"; int ton = 0;
                        using (var q = new SqlCommand("SELECT TenPhuTung, SoLuongTon FROM PhuTung WHERE MaPhuTung=@pt", cn, tran))
                        {
                            q.Parameters.Add(P("@pt", c.MaPhuTung));
                            using var rd = q.ExecuteReader();
                            if (rd.Read()) { ten = rd.GetString(0); ton = rd.GetInt32(1); }
                        }
                        throw new KhoKhongDuException(ten, c.SoLuong, ton);
                    }
                }

                // 5. Tổng tiền = Σ(SoLuong × DonGia) + Σ TienCong
                using (var cmd = new SqlCommand(TinhTongSql + " WHERE MaPhieu=@p", cn, tran))
                {
                    cmd.Parameters.Add(P("@p", maPhieu));
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return maPhieu;
            }
            catch
            {
                try { tran.Rollback(); } catch { }   // Rollback: phiếu, chi tiết và việc trừ kho đều bị hủy
                throw;
            }
        }

        private const string TinhTongSql = @"
            UPDATE PhieuSuaChua SET TongTien = (
                SELECT ISNULL(SUM(ct.SoLuong * pt.DonGia + ct.TienCong), 0)
                FROM ChiTietSuaChua ct JOIN PhuTung pt ON pt.MaPhuTung = ct.MaPhuTung
                WHERE ct.MaPhieu = PhieuSuaChua.MaPhieu)";

        /// <summary>Tính lại tổng chi phí và chuyển sang "Đã thanh toán" (chỉ khi đang "Đã hoàn thành").</summary>
        public int ThanhToan(int maPhieu) => ExecuteNonQuery(
            TinhTongSql + @", TrangThai = N'Đã thanh toán'
              WHERE MaPhieu = @p AND TrangThai = N'Đã hoàn thành'", P("@p", maPhieu));
    }
}
