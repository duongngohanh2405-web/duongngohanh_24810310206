using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GarageManager.DAL;
using GarageManager.DTO;

namespace GarageManager.BUS
{
    public class XeBUS
    {
        private readonly XeDAL dal = new XeDAL();

        public static string ChuanHoaBienSo(string s) => (s ?? "").Trim().ToUpperInvariant();

        public List<XeDTO> GetAll() => dal.GetAll();

        public XeDTO TraCuu(string bienSo)
        {
            bienSo = ChuanHoaBienSo(bienSo);
            if (bienSo.Length == 0) throw new ArgumentException("Hãy nhập biển số cần tra cứu.");
            return dal.GetByBienSo(bienSo);
        }

        private static void Validate(KhachHangDTO kh, XeDTO xe)
        {
            xe.BienSo = ChuanHoaBienSo(xe.BienSo);
            if (xe.BienSo.Length == 0 || xe.BienSo.Length > 20) throw new ArgumentException("Biển số không hợp lệ (1-20 ký tự).");
            if (string.IsNullOrWhiteSpace(kh.TenKH)) throw new ArgumentException("Tên chủ xe không được để trống.");

            kh.TenKH = kh.TenKH.Trim();
            kh.DienThoai = string.IsNullOrWhiteSpace(kh.DienThoai) ? null : kh.DienThoai.Trim();
            kh.DiaChi = string.IsNullOrWhiteSpace(kh.DiaChi) ? null : kh.DiaChi.Trim();
            if (kh.DienThoai != null && !Regex.IsMatch(kh.DienThoai, @"^\+?[0-9]{8,14}$"))
                throw new ArgumentException("Số điện thoại chỉ gồm 8-14 chữ số.");

            if (string.IsNullOrWhiteSpace(xe.HieuXe)) throw new ArgumentException("Hãy nhập hiệu xe.");
            xe.HieuXe = xe.HieuXe.Trim();
            if (xe.NamSanXuat.HasValue && (xe.NamSanXuat < 1950 || xe.NamSanXuat > DateTime.Now.Year + 1))
                throw new ArgumentException("Năm sản xuất không hợp lệ.");
        }

        public void TiepNhanXeMoi(KhachHangDTO kh, XeDTO xe)
        {
            Validate(kh, xe);
            if (dal.GetByBienSo(xe.BienSo) != null) throw new ArgumentException("Biển số này đã được tiếp nhận trước đó.");
            dal.TiepNhanXeMoi(kh, xe);
        }

        public void CapNhat(KhachHangDTO kh, XeDTO xe) { Validate(kh, xe); dal.CapNhat(kh, xe); }
        public void Xoa(string bienSo) => dal.Delete(ChuanHoaBienSo(bienSo));
    }

    public class PhuTungBUS
    {
        private readonly PhuTungDAL dal = new PhuTungDAL();
        public List<PhuTungDTO> GetAll() => dal.GetAll();

        private static void Validate(PhuTungDTO p)
        {
            if (string.IsNullOrWhiteSpace(p.TenPhuTung)) throw new ArgumentException("Tên phụ tùng không được để trống.");
            if (p.DonGia < 0) throw new ArgumentException("Đơn giá không được âm.");
            if (p.SoLuongTon < 0) throw new ArgumentException("Số lượng tồn không được âm.");
            p.TenPhuTung = p.TenPhuTung.Trim();
        }

        public int Them(PhuTungDTO p) { Validate(p); return dal.Insert(p); }
        public void Sua(PhuTungDTO p)
        {
            Validate(p);
            if (dal.Update(p) == 0) throw new InvalidOperationException("Không tìm thấy phụ tùng.");
        }
        public void Xoa(int ma) => dal.Delete(ma);
    }

    public class PhieuSuaChuaBUS
    {
        private readonly PhieuSuaChuaDAL dal = new PhieuSuaChuaDAL();
        private readonly XeDAL xeDal = new XeDAL();

        public List<PhieuSuaChuaDTO> GetAll(string trangThai = null) => dal.GetAll(trangThai);

        /// <summary>Kiểm tra dữ liệu, gộp dòng trùng phụ tùng (do PK là MaPhieu+MaPhuTung) rồi chạy transaction.</summary>
        public int LapPhieuHoanTat(string bienSo, string noiDung, IEnumerable<ChiTietSuaChuaDTO> chiTiet)
        {
            bienSo = XeBUS.ChuanHoaBienSo(bienSo);
            if (xeDal.GetByBienSo(bienSo) == null)
                throw new ArgumentException("Xe chưa được tiếp nhận. Hãy tiếp nhận xe trước khi lập phiếu.");

            var gop = chiTiet
                .GroupBy(c => c.MaPhuTung)
                .Select(g => new ChiTietSuaChuaDTO
                {
                    MaPhuTung = g.Key,
                    TenPhuTung = g.First().TenPhuTung,
                    SoLuong = g.Sum(x => x.SoLuong),
                    TienCong = g.Sum(x => x.TienCong)
                }).ToList();

            if (gop.Count == 0) throw new ArgumentException("Phiếu phải có ít nhất một dòng phụ tùng/dịch vụ.");
            if (gop.Any(c => c.SoLuong <= 0)) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            if (gop.Any(c => c.TienCong < 0)) throw new ArgumentException("Tiền công không được âm.");

            return dal.LapPhieuHoanTat(bienSo, string.IsNullOrWhiteSpace(noiDung) ? null : noiDung.Trim(), gop);
        }

        public HoaDonDTO TaoHoaDon(int maPhieu)
        {
            var phieu = dal.GetById(maPhieu) ?? throw new InvalidOperationException("Không tìm thấy phiếu sửa chữa.");
            return new HoaDonDTO { Phieu = phieu, ChiTiet = dal.GetChiTiet(maPhieu) };
        }

        public void ThanhToan(int maPhieu)
        {
            if (dal.ThanhToan(maPhieu) == 0)
                throw new InvalidOperationException("Chỉ thanh toán được phiếu đang ở trạng thái \"Đã hoàn thành\".");
        }

        public static string XuatHoaDonText(HoaDonDTO hd)
        {
            var vn = new CultureInfo("vi-VN");
            string M(decimal v) => v.ToString("N0", vn) + " đ";
            var sb = new StringBuilder();
            sb.AppendLine("==================== HÓA ĐƠN DỊCH VỤ GARAGE ====================");
            sb.AppendLine($"Số phiếu     : {hd.Phieu.MaPhieu}");
            sb.AppendLine($"Ngày nhận xe : {hd.Phieu.NgayNhan:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Chủ xe       : {hd.Phieu.TenKH}  ({hd.Phieu.DienThoai})");
            sb.AppendLine($"Biển số      : {hd.Phieu.BienSo}  - {hd.Phieu.HieuXe}");
            sb.AppendLine($"Nội dung sửa : {hd.Phieu.NoiDungSua}");
            sb.AppendLine($"Trạng thái   : {hd.Phieu.TrangThai}");
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine($"{"Phụ tùng",-26}{"SL",4}{"Đơn giá",14}{"Tiền công",14}{"Thành tiền",16}");
            foreach (var c in hd.ChiTiet)
                sb.AppendLine($"{c.TenPhuTung,-26}{c.SoLuong,4}{c.DonGia.ToString("N0", vn),14}{c.TienCong.ToString("N0", vn),14}{c.ThanhTien.ToString("N0", vn),16}");
            sb.AppendLine("----------------------------------------------------------------");
            sb.AppendLine($"Tiền phụ tùng (Σ SL × Đơn giá): {M(hd.TongTienPhuTung)}");
            sb.AppendLine($"Tiền công (Σ)                 : {M(hd.TongTienCong)}");
            sb.AppendLine($"TỔNG CHI PHÍ                  : {M(hd.TongChiPhi)}");
            sb.AppendLine("================================================================");
            return sb.ToString();
        }
    }
}
