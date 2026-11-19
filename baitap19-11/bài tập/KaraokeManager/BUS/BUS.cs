using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using KaraokeManager.DAL;
using KaraokeManager.DTO;

namespace KaraokeManager.BUS
{
    public class PhongBUS
    {
        private readonly PhongDAL dal = new PhongDAL();
        public List<PhongDTO> GetAll() => dal.GetAll();
        public int MoPhong(int maPhong) => dal.MoPhong(maPhong);
        public DateTime ThoiGianServer() => dal.GetServerTime();
    }

    public class DichVuBUS
    {
        private readonly DichVuDAL dal = new DichVuDAL();
        public List<DichVuDTO> GetAll() => dal.GetAll();
    }

    public class ChiTietDichVuBUS
    {
        private readonly ChiTietDichVuDAL dal = new ChiTietDichVuDAL();

        public DataTable LayDanhSach(int maHD) => dal.LayDanhSach(maHD);

        public void ThemDichVu(int maHD, int maDV, int soLuong)
        {
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0.");
            dal.ThemDichVu(maHD, maDV, soLuong);
        }

        public int LuuThayDoi(DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if (r.RowState == DataRowState.Modified && Convert.ToInt32(r["SoLuong"]) <= 0)
                    throw new ArgumentException("Số lượng phải lớn hơn 0. Muốn bỏ món hãy dùng nút \"Xóa dòng\".");
            }
            return dal.LuuThayDoi(dt);
        }
    }

    public class HoaDonBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public HoaDonDTO XemTruoc(int maHD) =>
            dal.XemTruoc(maHD) ?? throw new InvalidOperationException("Không tìm thấy hóa đơn.");

        public HoaDonDTO ThanhToan(int maHD) => dal.ThanhToan(maHD);

        public static string XuatHoaDonText(HoaDonDTO hd)
        {
            var vn = new CultureInfo("vi-VN");
            string M(decimal v) => v.ToString("N0", vn) + " đ";
            var sb = new StringBuilder();
            sb.AppendLine("================ HÓA ĐƠN KARAOKE ================");
            sb.AppendLine($"Số hóa đơn : {hd.MaHD}");
            sb.AppendLine($"Phòng      : {hd.TenPhong}   ({hd.GiaTheoGio.ToString("N0", vn)} đ/giờ)");
            sb.AppendLine($"Giờ vào    : {hd.GioVao:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Giờ ra     : {hd.GioRa:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine($"Thời gian  : {BangGia.DinhDangThoiGian(hd.ThoiGianHat)}  ({hd.ThoiGianHat.TotalHours:0.00} giờ)");
            sb.AppendLine("-------------------------------------------------");
            sb.AppendLine($"{"Dịch vụ",-24}{"SL",4}{"Đơn giá",10}{"Thành tiền",12}");
            foreach (var c in hd.ChiTiet)
                sb.AppendLine($"{c.TenDV,-24}{c.SoLuong,4}{c.DonGia.ToString("N0", vn),10}{c.ThanhTien.ToString("N0", vn),12}");
            sb.AppendLine("-------------------------------------------------");
            sb.AppendLine($"Tiền phòng : {M(hd.TongTienPhong)}");
            sb.AppendLine($"Tiền dịch vụ: {M(hd.TongTienDichVu)}");
            sb.AppendLine($"TỔNG CỘNG  : {M(hd.TongCong)}");
            sb.AppendLine("=================================================");
            return sb.ToString();
        }
    }
}
