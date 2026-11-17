using System;
using System.Data;
using HRM.DAL;
using HRM.DTO;

namespace HRM.BUS
{
    public class PhongBanBUS
    {
        private readonly PhongBanDAL dal = new PhongBanDAL();

        public DataTable GetAll() { return dal.GetAll(); }

        private static void Validate(PhongBanDTO pb)
        {
            if (string.IsNullOrWhiteSpace(pb.TenPB)) throw new Exception("Tên phòng ban không được để trống.");
        }

        public bool Insert(PhongBanDTO pb) { Validate(pb); return dal.Insert(pb); }
        public bool Update(PhongBanDTO pb) { Validate(pb); return dal.Update(pb); }
        public bool Delete(int ma) { return dal.Delete(ma); }
    }
}
