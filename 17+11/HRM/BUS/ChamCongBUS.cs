using System;
using System.Data;
using HRM.DAL;

namespace HRM.BUS
{
    public class ChamCongBUS
    {
        private readonly ChamCongDAL dal = new ChamCongDAL();

        public bool IsKhoa(string thangNam) { return ChamCongDAL.IsKhoa(thangNam); }

        public DataTable Nap(string thangNam) { return dal.Nap(thangNam); }

        public int Luu(DataTable table, string thangNam)
        {
            // Kiểm tra từng dòng đã sửa trước khi gửi về SQL Server
            foreach (DataRow r in table.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if (r.IsNull("SoNgayCong") || r.IsNull("SoGioOT"))
                    throw new Exception("Số ngày công và số giờ OT không được để trống (nhân viên " + r["HoTen"] + ").");
                double ngay = Convert.ToDouble(r["SoNgayCong"]);
                double ot = Convert.ToDouble(r["SoGioOT"]);
                if (ngay < 0 || ngay > 31)
                    throw new Exception("Số ngày công phải từ 0 đến 31 (nhân viên " + r["HoTen"] + ").");
                if (ot < 0)
                    throw new Exception("Số giờ OT không được âm (nhân viên " + r["HoTen"] + ").");
            }
            return dal.Luu(table, thangNam);
        }
    }
}
