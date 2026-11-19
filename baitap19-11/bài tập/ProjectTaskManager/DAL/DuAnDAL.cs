using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;
using static ProjectTaskManager.DAL.DbHelper;

namespace ProjectTaskManager.DAL
{
    public class DuAnDAL
    {
        public List<DuAnDTO> GetAll()
        {
            var list = new List<DuAnDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand("SELECT MaDuAn, TenDuAn, NgayBatDau, NgayKetThuc, TrangThai FROM DuAn ORDER BY MaDuAn", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new DuAnDTO
                {
                    MaDuAn = rd.GetInt32(0),
                    TenDuAn = rd.GetString(1),
                    NgayBatDau = rd.IsDBNull(2) ? null : rd.GetDateTime(2),
                    NgayKetThuc = rd.IsDBNull(3) ? null : rd.GetDateTime(3),
                    TrangThai = rd.IsDBNull(4) ? null : rd.GetString(4)
                });
            }
            return list;
        }

        public int Insert(DuAnDTO d) => (int)ExecuteScalar(
            @"INSERT DuAn (TenDuAn, NgayBatDau, NgayKetThuc, TrangThai)
              VALUES (@ten, @bd, @kt, @tt);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            P("@ten", d.TenDuAn), P("@bd", d.NgayBatDau), P("@kt", d.NgayKetThuc), P("@tt", d.TrangThai));

        public int Update(DuAnDTO d) => ExecuteNonQuery(
            @"UPDATE DuAn SET TenDuAn=@ten, NgayBatDau=@bd, NgayKetThuc=@kt, TrangThai=@tt
              WHERE MaDuAn=@ma",
            P("@ten", d.TenDuAn), P("@bd", d.NgayBatDau), P("@kt", d.NgayKetThuc),
            P("@tt", d.TrangThai), P("@ma", d.MaDuAn));

        // CongViec/PhanCong bị xóa theo nhờ ON DELETE CASCADE
        public int Delete(int maDuAn) => ExecuteNonQuery("DELETE FROM DuAn WHERE MaDuAn=@ma", P("@ma", maDuAn));
    }
}
