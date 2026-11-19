using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;
using static GarageManager.DAL.DbHelper;

namespace GarageManager.DAL
{
    public class PhuTungDAL
    {
        public List<PhuTungDTO> GetAll()
        {
            var list = new List<PhuTungDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand("SELECT MaPhuTung, TenPhuTung, DonGia, SoLuongTon FROM PhuTung ORDER BY TenPhuTung", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new PhuTungDTO
                {
                    MaPhuTung = rd.GetInt32(0),
                    TenPhuTung = rd.GetString(1),
                    DonGia = rd.GetDecimal(2),
                    SoLuongTon = rd.GetInt32(3)
                });
            return list;
        }

        public int Insert(PhuTungDTO p) => (int)ExecuteScalar(
            @"INSERT PhuTung (TenPhuTung, DonGia, SoLuongTon) VALUES (@ten, @gia, @ton);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            P("@ten", p.TenPhuTung), P("@gia", p.DonGia), P("@ton", p.SoLuongTon));

        public int Update(PhuTungDTO p) => ExecuteNonQuery(
            "UPDATE PhuTung SET TenPhuTung=@ten, DonGia=@gia, SoLuongTon=@ton WHERE MaPhuTung=@ma",
            P("@ten", p.TenPhuTung), P("@gia", p.DonGia), P("@ton", p.SoLuongTon), P("@ma", p.MaPhuTung));

        public int Delete(int ma)
        {
            try { return ExecuteNonQuery("DELETE FROM PhuTung WHERE MaPhuTung=@ma", P("@ma", ma)); }
            catch (SqlException ex) when (IsForeignKey(ex))
            {
                throw new InvalidOperationException("Phụ tùng đã được dùng trong phiếu sửa chữa nên không thể xóa.");
            }
        }
    }
}
