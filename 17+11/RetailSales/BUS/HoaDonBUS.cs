using System;
using System.Linq;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class HoaDonBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public static decimal TinhTongTien(HoaDonDTO hd)
        {
            return hd.ChiTiet.Sum(c => c.ThanhTien);
        }

        // 100.000 đ = 1 điểm (chia nguyên)
        public static int TinhDiem(decimal tongTien)
        {
            return (int)(tongTien / 100000m);
        }

        public int ThanhToan(HoaDonDTO hd)
        {
            if (hd.ChiTiet == null || hd.ChiTiet.Count == 0)
                throw new Exception("Giỏ hàng đang trống.");
            if (hd.MaNV <= 0)
                throw new Exception("Chưa chọn nhân viên bán hàng.");
            hd.TongTien = TinhTongTien(hd);
            return dal.ThanhToan(hd);
        }
    }
}
