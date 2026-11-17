using System;
using System.Data;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class NhanVienBUS
    {
        private readonly NhanVienDAL dal = new NhanVienDAL();

        public DataTable GetAll() { return dal.GetAll(); }

        private static void Validate(NhanVienDTO nv)
        {
            if (string.IsNullOrWhiteSpace(nv.TenNV)) throw new Exception("Tên nhân viên không được để trống.");
        }

        public bool Insert(NhanVienDTO nv) { Validate(nv); return dal.Insert(nv); }
        public bool Update(NhanVienDTO nv) { Validate(nv); return dal.Update(nv); }
        public bool Delete(int ma) { return dal.Delete(ma); }
    }
}
