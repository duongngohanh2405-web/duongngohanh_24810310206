using System;
using System.Collections.Generic;
using System.Linq;

namespace GarageManager.DTO
{
    public static class TrangThaiPhieu
    {
        public const string DangSua = "Đang sửa", HoanThanh = "Đã hoàn thành", DaThanhToan = "Đã thanh toán";
        public static readonly string[] All = { DangSua, HoanThanh, DaThanhToan };
    }

    public class KhachHangDTO
    {
        public int MaKH { get; set; }
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public string DiaChi { get; set; }
    }

    public class XeDTO
    {
        public string BienSo { get; set; }
        public int MaKH { get; set; }
        public string HieuXe { get; set; }
        public int? NamSanXuat { get; set; }
        // Thông tin chủ xe (JOIN)
        public string TenKH { get; set; }
        public string DienThoai { get; set; }
        public string DiaChi { get; set; }
    }

    public class PhuTungDTO
    {
        public int MaPhuTung { get; set; }
        public string TenPhuTung { get; set; }
        public decimal DonGia { get; set; }
        public int SoLuongTon { get; set; }
        public override string ToString() => $"{TenPhuTung} - {DonGia:N0}đ (tồn {SoLuongTon})";
    }

    public class PhieuSuaChuaDTO
    {
        public int MaPhieu { get; set; }
        public string BienSo { get; set; }
        public DateTime NgayNhan { get; set; }
        public string NoiDungSua { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public string TenKH { get; set; }          // JOIN
        public string DienThoai { get; set; }      // JOIN
        public string HieuXe { get; set; }         // JOIN
    }

    public class ChiTietSuaChuaDTO
    {
        public int MaPhieu { get; set; }
        public int MaPhuTung { get; set; }
        public string TenPhuTung { get; set; }    // JOIN
        public decimal DonGia { get; set; }       // JOIN (đơn giá hiện hành của phụ tùng)
        public int SoLuong { get; set; }
        public decimal TienCong { get; set; }
        public decimal TienPhuTung => SoLuong * DonGia;
        public decimal ThanhTien => TienPhuTung + TienCong;
    }

    public class HoaDonDTO
    {
        public PhieuSuaChuaDTO Phieu { get; set; }
        public List<ChiTietSuaChuaDTO> ChiTiet { get; set; } = new List<ChiTietSuaChuaDTO>();
        public decimal TongTienPhuTung => ChiTiet.Sum(c => c.TienPhuTung);
        public decimal TongTienCong => ChiTiet.Sum(c => c.TienCong);
        public decimal TongChiPhi => TongTienPhuTung + TongTienCong;
    }

    /// <summary>Ném ra khi trừ kho thất bại vì không đủ phụ tùng -> SqlTransaction bị Rollback.</summary>
    public class KhoKhongDuException : Exception
    {
        public KhoKhongDuException(string tenPhuTung, int can, int ton)
            : base($"Phụ tùng \"{tenPhuTung}\" không đủ tồn kho (cần {can}, chỉ còn {ton}).\n" +
                   "Giao dịch đã được Rollback: phiếu sửa chữa chưa được lưu và kho không bị thay đổi.") { }
    }
}
