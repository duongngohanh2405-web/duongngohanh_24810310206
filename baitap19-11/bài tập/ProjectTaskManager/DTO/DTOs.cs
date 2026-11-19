using System;

namespace ProjectTaskManager.DTO
{
    public static class TrangThaiTask
    {
        public const string ToDo = "To Do", InProgress = "In Progress", Review = "Review", Done = "Done";
        public static readonly string[] All = { ToDo, InProgress, Review, Done };
    }

    public static class DoUuTienTask
    {
        public static readonly string[] All = { "Cao", "Trung bình", "Thấp" };
    }

    public static class TrangThaiDuAn
    {
        public static readonly string[] All = { "Đang chạy", "Hoàn thành", "Tạm dừng" };
    }

    public class DuAnDTO
    {
        public int MaDuAn { get; set; }
        public string TenDuAn { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; }
    }

    public class ThanhVienDTO
    {
        public int MaThanhVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string ChuyenMon { get; set; }
        public override string ToString() => $"{HoTen} ({ChuyenMon})";
    }

    public class CongViecDTO
    {
        public int MaTask { get; set; }
        public int MaDuAn { get; set; }
        public string TenTask { get; set; }
        public string MoTa { get; set; }
        public DateTime? HanNop { get; set; }
        public string DoUuTien { get; set; }
        public string TrangThai { get; set; }
        // Thông tin hiển thị (JOIN) - không phải cột của bảng CongViec
        public string TenDuAn { get; set; }
        public string NguoiPhuTrach { get; set; }
        public override string ToString() => $"#{MaTask} {TenTask} [{DoUuTien}]";
    }

    public class PhanCongDTO
    {
        public int MaTask { get; set; }
        public int MaThanhVien { get; set; }
        public DateTime NgayGiao { get; set; }
    }

    public class ThongKeThanhVienDTO
    {
        public int MaThanhVien { get; set; }
        public string HoTen { get; set; }
        public int TongTask { get; set; }
        public int DaHoanThanh { get; set; }
        public decimal TyLeHoanThanh { get; set; }   // %
    }
}
