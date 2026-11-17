using System;
using System.Data;
using Microsoft.Data.SqlClient;
using RetailSales.DTO;

namespace RetailSales.DAL
{
    public class HoaDonDAL
    {
        /// <summary>
        /// Thanh toán trong MỘT SqlTransaction:
        /// Các bước: chèn HoaDon, chèn ChiTietHoaDon, trừ SoLuongTon (thiếu hàng thì Rollback),
        /// cộng DiemTichLuy, rồi Commit. Trả về MaHD.
        /// </summary>
        public int ThanhToan(HoaDonDTO hd)
        {
            using (var conn = DBHelper.GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Chèn hóa đơn
                        int maHD;
                        using (var cmd = new SqlCommand(
                            @"INSERT INTO HoaDon (NgayLap, MaNV, MaKH, TongTien)
                              OUTPUT INSERTED.MaHD
                              VALUES (GETDATE(), @manv, @makh, @tong)", conn, trans))
                        {
                            cmd.Parameters.Add("@manv", SqlDbType.Int).Value = hd.MaNV;
                            cmd.Parameters.Add("@makh", SqlDbType.Int).Value =
                                hd.MaKH.HasValue ? (object)hd.MaKH.Value : DBNull.Value;
                            cmd.Parameters.Add("@tong", SqlDbType.Decimal).Value = hd.TongTien;
                            maHD = (int)cmd.ExecuteScalar();
                        }

                        foreach (ChiTietHoaDonDTO ct in hd.ChiTiet)
                        {
                            // 2. Chèn chi tiết hóa đơn
                            using (var cmd = new SqlCommand(
                                @"INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, DonGia)
                                  VALUES (@mahd, @masp, @sl, @gia)", conn, trans))
                            {
                                cmd.Parameters.Add("@mahd", SqlDbType.Int).Value = maHD;
                                cmd.Parameters.Add("@masp", SqlDbType.VarChar, 20).Value = ct.MaSP;
                                cmd.Parameters.Add("@sl", SqlDbType.Int).Value = ct.SoLuong;
                                cmd.Parameters.Add("@gia", SqlDbType.Decimal).Value = ct.DonGia;
                                cmd.ExecuteNonQuery();
                            }

                            // 3. Trừ tồn kho (kiểm tra và trừ trong cùng một câu lệnh)
                            using (var cmd = new SqlCommand(
                                @"UPDATE SanPham SET SoLuongTon = SoLuongTon - @sl
                                  WHERE MaSP = @masp AND SoLuongTon >= @sl", conn, trans))
                            {
                                cmd.Parameters.Add("@masp", SqlDbType.VarChar, 20).Value = ct.MaSP;
                                cmd.Parameters.Add("@sl", SqlDbType.Int).Value = ct.SoLuong;
                                if (cmd.ExecuteNonQuery() == 0)
                                {
                                    trans.Rollback();
                                    throw new TonKhoException(
                                        "Sản phẩm \"" + ct.TenSP + "\" không đủ số lượng tồn kho.");
                                }
                            }
                        }

                        // 4. Cộng điểm tích lũy: TongTien / 100000 (chỉ khi có khách hàng)
                        if (hd.MaKH.HasValue)
                        {
                            int diem = (int)(hd.TongTien / 100000m);
                            using (var cmd = new SqlCommand(
                                "UPDATE KhachHang SET DiemTichLuy = DiemTichLuy + @diem WHERE MaKH = @makh",
                                conn, trans))
                            {
                                cmd.Parameters.Add("@diem", SqlDbType.Int).Value = diem;
                                cmd.Parameters.Add("@makh", SqlDbType.Int).Value = hd.MaKH.Value;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 5. Hoàn tất
                        trans.Commit();
                        return maHD;
                    }
                    catch (TonKhoException)
                    {
                        throw; // đã Rollback ở trên
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
