using System;
using System.Collections.Generic;
using HRM.DAL;
using HRM.DTO;

namespace HRM.BUS
{
    public class LuongBUS
    {
        public const decimal SoNgayChuan = 22m;
        public const decimal TienOT = 50000m;

        private readonly LuongDAL dal = new LuongDAL();

        // ThucLinh = (LuongCoBan * HeSoLuong / 22) * SoNgayCong + SoGioOT * 50.000 (làm tròn đến đồng)
        public static decimal TinhThucLinh(decimal luongCoBan, double heSo, double ngayCong, double gioOT)
        {
            decimal v = luongCoBan * (decimal)heSo / SoNgayChuan * (decimal)ngayCong
                        + (decimal)gioOT * TienOT;
            return Math.Round(v, 0, MidpointRounding.AwayFromZero);
        }

        public bool IsKhoa(string thangNam) { return ChamCongDAL.IsKhoa(thangNam); }

        /// <summary>Tháng chưa chốt: tính thử theo công thức. Tháng đã chốt: lấy số đã lưu trong BangLuong.</summary>
        public List<LuongChiTietDTO> XemLuong(string thangNam)
        {
            List<LuongChiTietDTO> list = dal.GetChiTiet(thangNam);
            foreach (LuongChiTietDTO c in list)
            {
                c.ThucLinh = c.ThucLinhDaChot.HasValue
                    ? c.ThucLinhDaChot.Value
                    : TinhThucLinh(c.LuongCoBan, c.HeSoLuong, c.SoNgayCong, c.SoGioOT);
            }
            return list;
        }

        public int ChotLuong(string thangNam)
        {
            return dal.ChotLuong(thangNam,
                c => TinhThucLinh(c.LuongCoBan, c.HeSoLuong, c.SoNgayCong, c.SoGioOT));
        }
    }
}
