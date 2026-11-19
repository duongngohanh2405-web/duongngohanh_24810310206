using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using KaraokeManager.DTO;
using static KaraokeManager.DAL.DbHelper;

namespace KaraokeManager.DAL
{
    public class PhongDAL
    {
        /// <summary>Danh sách phòng kèm hóa đơn đang mở (nếu có).</summary>
        public List<PhongDTO> GetAll()
        {
            var list = new List<PhongDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT p.MaPhong, p.TenPhong, p.LoaiPhong, p.GiaTheoGio, p.TrangThai, hd.MaHD, hd.GioVao
                FROM PhongHat p
                LEFT JOIN HoaDon hd ON hd.MaPhong = p.MaPhong AND hd.TrangThai = 0
                ORDER BY p.MaPhong", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new PhongDTO
                {
                    MaPhong = rd.GetInt32(0),
                    TenPhong = rd.GetString(1),
                    LoaiPhong = rd.IsDBNull(2) ? null : rd.GetString(2),
                    GiaTheoGio = rd.GetDecimal(3),
                    TrangThai = rd.GetString(4),
                    MaHDHienTai = rd.IsDBNull(5) ? (int?)null : rd.GetInt32(5),
                    GioVao = rd.IsDBNull(6) ? (DateTime?)null : rd.GetDateTime(6)
                });
            return list;
        }

        /// <summary>
        /// Mở phòng trong 1 transaction: đổi phòng "Trống" -> "Đang hát" rồi tạo HoaDon với GioVao = GETDATE().
        /// Điều kiện TrangThai = 'Trống' nằm ngay trong UPDATE nên 2 máy bấm cùng lúc chỉ 1 máy mở được.
        /// </summary>
        public int MoPhong(int maPhong)
        {
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                using (var upd = new SqlCommand(
                    "UPDATE PhongHat SET TrangThai = N'Đang hát' WHERE MaPhong = @p AND TrangThai = N'Trống'", cn, tran))
                {
                    upd.Parameters.Add(P("@p", maPhong));
                    if (upd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException("Phòng này không còn trống (có thể vừa được mở ở máy khác).");
                }

                int maHD;
                using (var ins = new SqlCommand(
                    @"INSERT HoaDon (MaPhong, GioVao, TongTienPhong, TongTienDichVu, TrangThai)
                      VALUES (@p, GETDATE(), 0, 0, 0);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tran))
                {
                    ins.Parameters.Add(P("@p", maPhong));
                    maHD = (int)ins.ExecuteScalar();
                }

                tran.Commit();
                return maHD;
            }
            catch { SafeRollback(tran); throw; }
        }

        /// <summary>Giờ của SQL Server - để đồng hồ trên màn hình khớp với GETDATE() lúc ghi GioVao/GioRa.</summary>
        public DateTime GetServerTime()
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand("SELECT GETDATE()", cn);
            cn.Open();
            return (DateTime)cmd.ExecuteScalar();
        }
    }
}
