using System;
using System.Collections.Generic;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class ThongKeBUS
    {
        private readonly ThongKeDAL dal = new ThongKeDAL();

        public decimal DoanhThuNgay(DateTime ngay) { return dal.DoanhThuNgay(ngay); }
        public decimal DoanhThuThang(int thang, int nam) { return dal.DoanhThuThang(thang, nam); }
        public List<ThongKeSanPhamDTO> Top5BanChay() { return dal.Top5BanChay(); }
    }
}
