using System;
using System.Collections.Generic;
using System.Linq;

namespace KaraokeManager.DTO
{
    public static class TrangThaiPhong
    {
        public const string Trong = "Trống", DangHat = "Đang hát";
    }

    public class PhongDTO
    {
        public int MaPhong { get; set; }
        public string TenPhong { get; set; }
        public string LoaiPhong { get; set; }
        public decimal GiaTheoGio { get; set; }
        public string TrangThai { get; set; }
        // Hóa đơn đang mở của phòng (LEFT JOIN) - null nếu phòng trống
        public int? MaHDHienTai { get; set; }
        public DateTime? GioVao { get; set; }
    }

    public class DichVuDTO
    {
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public decimal DonGia { get; set; }
        public int TonKho { get; set; }
        public override string ToString() => $"{TenDV} - {DonGia:N0}đ (tồn {TonKho})";
    }

    public class ChiTietDichVuDTO
    {
        public int MaHD { get; set; }
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }

    public class HoaDonDTO
    {
        public int MaHD { get; set; }
        public int MaPhong { get; set; }
        public string TenPhong { get; set; }
        public decimal GiaTheoGio { get; set; }
        public DateTime GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public decimal TongTienPhong { get; set; }
        public decimal TongTienDichVu { get; set; }
        public bool DaThanhToan { get; set; }
        public List<ChiTietDichVuDTO> ChiTiet { get; set; } = new List<ChiTietDichVuDTO>();

        public TimeSpan ThoiGianHat => (GioRa ?? GioVao) - GioVao;
        public decimal TongCong => TongTienPhong + TongTienDichVu;
    }

    /// <summary>Quy tắc tính tiền phòng, dùng chung cho xem trước và thanh toán.</summary>
    public static class BangGia
    {
        /// <summary>TongTienPhong = (Số giờ) × GiaTheoGio; số giờ tính chính xác đến giây, tiền làm tròn đến đồng.</summary>
        public static decimal TinhTienPhong(DateTime gioVao, DateTime gioRa, decimal giaTheoGio)
        {
            double gio = Math.Max(0, (gioRa - gioVao).TotalHours);
            return Math.Round((decimal)gio * giaTheoGio, 0, MidpointRounding.AwayFromZero);
        }

        public static string DinhDangThoiGian(TimeSpan t)
        {
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;
            return $"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
        }
    }

    /// <summary>Không đủ tồn kho cho một mặt hàng.</summary>
    public class KhoKhongDuException : Exception
    {
        public KhoKhongDuException(string tenDV, int can, int ton)
            : base($"Mặt hàng \"{tenDV}\" không đủ tồn kho (cần {can}, chỉ còn {ton}).") { }
    }
}
