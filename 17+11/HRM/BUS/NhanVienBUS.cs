using System;
using System.Data;
using System.Linq;
using HRM.DAL;
using HRM.DTO;

namespace HRM.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new NhanVienDAL();

        public DataTable Search(string keyword, int? maPB) { return dal.Search(keyword, maPB); }

        private static void Validate(NhanVienDTO nv)
        {
            if (string.IsNullOrWhiteSpace(nv.HoTen)) throw new Exception("Họ tên không được để trống.");
            if (nv.MaPB <= 0) throw new Exception("Hãy chọn phòng ban.");
            if (nv.LuongCoBan <= 0) throw new Exception("Lương cơ bản phải lớn hơn 0.");
            if (nv.HeSoLuong <= 0) throw new Exception("Hệ số lương phải lớn hơn 0.");
            if (nv.NgaySinh.HasValue && nv.NgaySinh.Value.Date > DateTime.Today)
                throw new Exception("Ngày sinh không được lớn hơn ngày hiện tại.");
            string cccd = (nv.CCCD ?? "").Trim();
            if (cccd.Length > 0 && (cccd.Length < 9 || cccd.Length > 12 || !cccd.All(char.IsDigit)))
                throw new Exception("CCCD chỉ gồm chữ số (9 đến 12 số).");
        }

        public bool Insert(NhanVienDTO nv) { Validate(nv); return dal.Insert(nv); }
        public bool Update(NhanVienDTO nv) { Validate(nv); return dal.Update(nv); }

        public bool ChuyenPhongBan(int maNV, int maPB)
        {
            if (maNV <= 0) throw new Exception("Hãy chọn nhân viên cần chuyển.");
            if (maPB <= 0) throw new Exception("Hãy chọn phòng ban mới.");
            return dal.ChuyenPhongBan(maNV, maPB);
        }

        public bool Delete(int maNV) { return dal.Delete(maNV); }
    }
}
