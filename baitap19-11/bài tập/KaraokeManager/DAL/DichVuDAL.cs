using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using KaraokeManager.DTO;
using static KaraokeManager.DAL.DbHelper;

namespace KaraokeManager.DAL
{
    public class DichVuDAL
    {
        public List<DichVuDTO> GetAll()
        {
            var list = new List<DichVuDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand("SELECT MaDV, TenDV, DonGia, TonKho FROM DichVu ORDER BY TenDV", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new DichVuDTO
                {
                    MaDV = rd.GetInt32(0), TenDV = rd.GetString(1),
                    DonGia = rd.GetDecimal(2), TonKho = rd.GetInt32(3)
                });
            return list;
        }
    }
}
