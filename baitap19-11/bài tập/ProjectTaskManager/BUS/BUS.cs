using System;
using System.Collections.Generic;
using System.Linq;
using ProjectTaskManager.DAL;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.BUS
{
    public class DuAnBUS
    {
        private readonly DuAnDAL dal = new DuAnDAL();

        public List<DuAnDTO> GetAll() => dal.GetAll();

        private static void Validate(DuAnDTO d)
        {
            if (string.IsNullOrWhiteSpace(d.TenDuAn)) throw new ArgumentException("Tên dự án không được để trống.");
            if (d.NgayBatDau.HasValue && d.NgayKetThuc.HasValue && d.NgayKetThuc < d.NgayBatDau)
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");
            if (!TrangThaiDuAn.All.Contains(d.TrangThai)) throw new ArgumentException("Trạng thái dự án không hợp lệ.");
        }

        public int Them(DuAnDTO d) { Validate(d); return dal.Insert(d); }
        public void Sua(DuAnDTO d)
        {
            Validate(d);
            if (dal.Update(d) == 0) throw new InvalidOperationException("Không tìm thấy dự án để sửa.");
        }
        public void Xoa(int maDuAn) => dal.Delete(maDuAn);
    }

    public class ThanhVienBUS
    {
        private readonly ThanhVienDAL dal = new ThanhVienDAL();
        public List<ThanhVienDTO> GetAll() => dal.GetAll();
    }

    public class CongViecBUS
    {
        private readonly CongViecDAL dal = new CongViecDAL();

        public List<CongViecDTO> GetByDuAn(int maDuAn, string trangThai = null) => dal.GetByDuAn(maDuAn, trangThai);
        public List<CongViecDTO> GetOverdue(int? maDuAn = null) => dal.GetOverdue(maDuAn);

        private static void Validate(CongViecDTO t)
        {
            if (string.IsNullOrWhiteSpace(t.TenTask)) throw new ArgumentException("Tên công việc không được để trống.");
            if (!DoUuTienTask.All.Contains(t.DoUuTien)) throw new ArgumentException("Độ ưu tiên không hợp lệ.");
        }

        public int Them(CongViecDTO t)
        {
            Validate(t);
            t.TrangThai = TrangThaiTask.ToDo;      // task mới luôn bắt đầu ở To Do
            return dal.Insert(t);
        }

        public void Sua(CongViecDTO t) { Validate(t); dal.Update(t); }
        public void Xoa(int maTask) => dal.Delete(maTask);

        public void ChuyenTrangThai(int maTask, string trangThaiMoi)
        {
            if (!TrangThaiTask.All.Contains(trangThaiMoi)) throw new ArgumentException("Trạng thái không hợp lệ.");
            if (dal.UpdateTrangThai(maTask, trangThaiMoi) == 0)
                throw new InvalidOperationException("Không tìm thấy công việc.");
        }

        /// <summary>Chuyển sang cột kế tiếp (+1) hoặc cột trước (-1) trên Kanban.</summary>
        public string ChuyenKe(CongViecDTO t, int buoc)
        {
            int idx = Array.IndexOf(TrangThaiTask.All, t.TrangThai) + buoc;
            if (idx < 0 || idx >= TrangThaiTask.All.Length)
                throw new InvalidOperationException("Không thể chuyển thêm theo hướng này.");
            ChuyenTrangThai(t.MaTask, TrangThaiTask.All[idx]);
            return TrangThaiTask.All[idx];
        }
    }

    public class PhanCongBUS
    {
        private readonly PhanCongDAL dal = new PhanCongDAL();

        public int GiaoViec(int maTask, IEnumerable<int> dsThanhVien)
        {
            var ds = dsThanhVien.Distinct().ToList();
            if (ds.Count == 0) throw new ArgumentException("Hãy chọn ít nhất một thành viên.");
            return dal.GiaoViec(maTask, ds);
        }

        public void HuyPhanCong(int maTask, int maThanhVien) => dal.HuyPhanCong(maTask, maThanhVien);
        public List<int> GetThanhVienCuaTask(int maTask) => dal.GetMaThanhVienCuaTask(maTask);
    }

    public class ThongKeBUS
    {
        private readonly ThongKeDAL dal = new ThongKeDAL();
        public List<ThongKeThanhVienDTO> TheoThanhVien(int maDuAn) => dal.ThongKeTheoThanhVien(maDuAn);
        public decimal TyLeThanhVien(int maDuAn, int maTV) => dal.TyLeThanhVien(maDuAn, maTV);
        public decimal TyLeDuAn(int maDuAn) => dal.TyLeDuAn(maDuAn);
    }
}
