using System;
using System.Collections.Generic;
using System.Data;
using RetailSales.DAL;
using RetailSales.DTO;

namespace RetailSales.BUS
{
    public class SanPhamBUS
    {
        private readonly SanPhamDAL dal = new SanPhamDAL();

        public DataTable GetAll() { return dal.GetAll(); }
        public List<SanPhamDTO> GetList() { return dal.GetList(); }
        public SanPhamDTO GetById(string ma) { return dal.GetById(ma); }

        public DataTable Search(string keyword)
        {
            return string.IsNullOrWhiteSpace(keyword) ? dal.GetAll() : dal.Search(keyword.Trim());
        }

        private static void Validate(SanPhamDTO sp)
        {
            if (string.IsNullOrWhiteSpace(sp.MaSP)) throw new Exception("Mã sản phẩm không được để trống.");
            if (string.IsNullOrWhiteSpace(sp.TenSP)) throw new Exception("Tên sản phẩm không được để trống.");
            if (sp.GiaBan <= 0) throw new Exception("Giá bán phải lớn hơn 0.");
            if (sp.SoLuongTon < 0) throw new Exception("Số lượng tồn không được âm.");
        }

        public bool Insert(SanPhamDTO sp) { Validate(sp); return dal.Insert(sp); }
        public bool Update(SanPhamDTO sp) { Validate(sp); return dal.Update(sp); }
        public bool Delete(string ma) { return dal.Delete(ma); }
    }
}
