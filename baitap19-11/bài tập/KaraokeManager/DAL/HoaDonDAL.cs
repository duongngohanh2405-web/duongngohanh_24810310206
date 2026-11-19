using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using KaraokeManager.DTO;
using static KaraokeManager.DAL.DbHelper;

namespace KaraokeManager.DAL
{
    public class HoaDonDAL
    {
        /// <summary>
        /// Đọc hóa đơn + giá phòng + giờ hiện tại của server + tiền dịch vụ. GioRa trả về là GETDATE() (tạm tính).
        /// khoa = true: giữ UPDLOCK trên dòng hóa đơn đến hết transaction.
        /// </summary>
        private static HoaDonDTO Doc(SqlConnection cn, SqlTransaction tran, int maHD, bool khoa)
        {
            string hint = khoa ? "WITH (UPDLOCK)" : "";
            using var cmd = new SqlCommand($@"
                SELECT hd.MaHD, hd.MaPhong, hd.GioVao, hd.TrangThai, p.TenPhong, p.GiaTheoGio, GETDATE(),
                       (SELECT ISNULL(SUM(SoLuong * DonGia), 0) FROM ChiTietDichVu WHERE MaHD = hd.MaHD)
                FROM HoaDon hd {hint}
                JOIN PhongHat p ON p.MaPhong = hd.MaPhong
                WHERE hd.MaHD = @hd", cn, tran);
            cmd.Parameters.Add(P("@hd", maHD));
            using var rd = cmd.ExecuteReader();
            if (!rd.Read()) return null;

            var hd = new HoaDonDTO
            {
                MaHD = rd.GetInt32(0), MaPhong = rd.GetInt32(1), GioVao = rd.GetDateTime(2),
                DaThanhToan = rd.GetBoolean(3), TenPhong = rd.GetString(4), GiaTheoGio = rd.GetDecimal(5),
                GioRa = rd.GetDateTime(6), TongTienDichVu = rd.GetDecimal(7)
            };
            hd.TongTienPhong = BangGia.TinhTienPhong(hd.GioVao, hd.GioRa.Value, hd.GiaTheoGio);
            return hd;
        }

        private static List<ChiTietDichVuDTO> DocChiTiet(SqlConnection cn, SqlTransaction tran, int maHD)
        {
            var list = new List<ChiTietDichVuDTO>();
            using var cmd = new SqlCommand(@"
                SELECT ct.MaHD, ct.MaDV, dv.TenDV, ct.SoLuong, ct.DonGia
                FROM ChiTietDichVu ct JOIN DichVu dv ON dv.MaDV = ct.MaDV
                WHERE ct.MaHD = @hd ORDER BY dv.TenDV", cn, tran);
            cmd.Parameters.Add(P("@hd", maHD));
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new ChiTietDichVuDTO
                {
                    MaHD = rd.GetInt32(0), MaDV = rd.GetInt32(1), TenDV = rd.GetString(2),
                    SoLuong = rd.GetInt32(3), DonGia = rd.GetDecimal(4)
                });
            return list;
        }

        /// <summary>Xem trước hóa đơn (chưa ghi gì vào CSDL): giờ ra = giờ hiện tại của server.</summary>
        public HoaDonDTO XemTruoc(int maHD)
        {
            using var cn = CreateConnection();
            cn.Open();
            var hd = Doc(cn, null, maHD, false);
            if (hd != null) hd.ChiTiet = DocChiTiet(cn, null, maHD);
            return hd;
        }

        /// <summary>
        /// THANH TOÁN - một SqlTransaction:
        ///   1) Cập nhật HoaDon: GioRa, TongTienPhong, TongTienDichVu, TrangThai = 1
        ///   2) Trừ TonKho từng dịch vụ (thiếu hàng -> ném KhoKhongDuException -> Rollback)
        ///   3) Đưa PhongHat về "Trống"
        ///   4) Commit
        /// </summary>
        public HoaDonDTO ThanhToan(int maHD)
        {
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                var hd = Doc(cn, tran, maHD, true) ?? throw new InvalidOperationException("Không tìm thấy hóa đơn.");
                if (hd.DaThanhToan) throw new InvalidOperationException("Hóa đơn này đã được thanh toán trước đó.");

                hd.ChiTiet = DocChiTiet(cn, tran, maHD);
                hd.TongTienDichVu = hd.ChiTiet.Sum(c => c.ThanhTien);

                // 1. Cập nhật hóa đơn
                using (var cmd = new SqlCommand(@"
                    UPDATE HoaDon SET GioRa = @ra, TongTienPhong = @tp, TongTienDichVu = @tdv, TrangThai = 1
                    WHERE MaHD = @hd AND TrangThai = 0", cn, tran))
                {
                    cmd.Parameters.Add(P("@ra", hd.GioRa));
                    cmd.Parameters.Add(P("@tp", hd.TongTienPhong));
                    cmd.Parameters.Add(P("@tdv", hd.TongTienDichVu));
                    cmd.Parameters.Add(P("@hd", maHD));
                    if (cmd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Hóa đơn đã được thanh toán ở nơi khác.");
                }

                // 2. Trừ tồn kho (kiểm tra và trừ trong cùng một câu UPDATE)
                foreach (var c in hd.ChiTiet)
                {
                    int affected;
                    using (var cmd = new SqlCommand(
                        "UPDATE DichVu SET TonKho = TonKho - @sl WHERE MaDV = @dv AND TonKho >= @sl", cn, tran))
                    {
                        cmd.Parameters.Add(P("@sl", c.SoLuong));
                        cmd.Parameters.Add(P("@dv", c.MaDV));
                        affected = cmd.ExecuteNonQuery();
                    }
                    if (affected == 0)
                    {
                        int ton = 0;
                        using (var q = new SqlCommand("SELECT TonKho FROM DichVu WHERE MaDV = @dv", cn, tran))
                        {
                            q.Parameters.Add(P("@dv", c.MaDV));
                            object o = q.ExecuteScalar();
                            if (o != null) ton = (int)o;
                        }
                        throw new KhoKhongDuException(c.TenDV, c.SoLuong, ton);
                    }
                }

                // 3. Phòng về "Trống"
                using (var cmd = new SqlCommand("UPDATE PhongHat SET TrangThai = N'Trống' WHERE MaPhong = @p", cn, tran))
                {
                    cmd.Parameters.Add(P("@p", hd.MaPhong));
                    cmd.ExecuteNonQuery();
                }

                // 4. Commit
                tran.Commit();
                hd.DaThanhToan = true;
                return hd;
            }
            catch { SafeRollback(tran); throw; }
        }
    }
}
