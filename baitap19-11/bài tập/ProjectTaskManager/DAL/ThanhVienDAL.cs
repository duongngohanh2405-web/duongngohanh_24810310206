using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;
using static ProjectTaskManager.DAL.DbHelper;

namespace ProjectTaskManager.DAL
{
    public class ThanhVienDAL
    {
        public List<ThanhVienDTO> GetAll()
        {
            var list = new List<ThanhVienDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand("SELECT MaThanhVien, HoTen, Email, ChuyenMon FROM ThanhVien ORDER BY HoTen", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new ThanhVienDTO
                {
                    MaThanhVien = rd.GetInt32(0),
                    HoTen = rd.GetString(1),
                    Email = rd.IsDBNull(2) ? null : rd.GetString(2),
                    ChuyenMon = rd.IsDBNull(3) ? null : rd.GetString(3)
                });
            return list;
        }
    }
}
