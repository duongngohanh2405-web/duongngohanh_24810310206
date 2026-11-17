using System;
using System.Data;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class KhachHangBUS
    {
        private readonly KhachHangDAL dal = new KhachHangDAL();

        public DataTable GetAll() { return dal.GetAll(); }

        private static void Validate(KhachHangDTO kh)
        {
            if (string.IsNullOrWhiteSpace(kh.TenKH)) throw new Exception("Tên khách hàng không được để trống.");
        }

        public bool Insert(KhachHangDTO kh) { Validate(kh); return dal.Insert(kh); }
        public bool Update(KhachHangDTO kh) { Validate(kh); return dal.Update(kh); }
        public bool Delete(int ma) { return dal.Delete(ma); }
    }
}
