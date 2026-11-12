using System;

namespace Bai5_4
{
    // Nhân viên thuộc một nhóm của một phòng ban
    public class Employee
    {
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public string ChucVu { get; set; }
        public DateTime NgayVaoLam { get; set; }
        public string Phong { get; set; }   // tên phòng ban
        public string Nhom { get; set; }    // tên nhóm

        public Employee(string ma, string ten, string chucVu, DateTime ngayVao,
                        string phong, string nhom)
        {
            MaNV = ma;
            HoTen = ten;
            ChucVu = chucVu;
            NgayVaoLam = ngayVao;
            Phong = phong;
            Nhom = nhom;
        }
    }
}
