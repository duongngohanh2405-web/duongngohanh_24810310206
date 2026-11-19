using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;
using static ProjectTaskManager.DAL.DbHelper;

namespace ProjectTaskManager.DAL
{
    public class ThongKeDAL
    {
        /// <summary>% hoàn thành của từng thành viên trong 1 dự án (GROUP BY).</summary>
        public List<ThongKeThanhVienDTO> ThongKeTheoThanhVien(int maDuAn)
        {
            var list = new List<ThongKeThanhVienDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(@"
                SELECT tv.MaThanhVien, tv.HoTen,
                       COUNT(*) AS TongTask,
                       SUM(CASE WHEN cv.TrangThai = N'Done' THEN 1 ELSE 0 END) AS DaHoanThanh,
                       CAST(100.0 * SUM(CASE WHEN cv.TrangThai = N'Done' THEN 1 ELSE 0 END) / COUNT(*) AS DECIMAL(5,2)) AS TyLe
                FROM PhanCong pc
                JOIN CongViec cv ON cv.MaTask = pc.MaTask
                JOIN ThanhVien tv ON tv.MaThanhVien = pc.MaThanhVien
                WHERE cv.MaDuAn = @da
                GROUP BY tv.MaThanhVien, tv.HoTen
                ORDER BY TyLe DESC", cn);
            cmd.Parameters.AddWithValue("@da", maDuAn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new ThongKeThanhVienDTO
                {
                    MaThanhVien = rd.GetInt32(0),
                    HoTen = rd.GetString(1),
                    TongTask = rd.GetInt32(2),
                    DaHoanThanh = rd.GetInt32(3),
                    TyLeHoanThanh = rd.GetDecimal(4)
                });
            return list;
        }

        /// <summary>% hoàn thành của 1 thành viên trong 1 dự án (ExecuteScalar). Chưa có task -> 0.</summary>
        public decimal TyLeThanhVien(int maDuAn, int maThanhVien)
        {
            object r = ExecuteScalar(@"
                SELECT CAST(100.0 * SUM(CASE WHEN cv.TrangThai = N'Done' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5,2))
                FROM PhanCong pc JOIN CongViec cv ON cv.MaTask = pc.MaTask
                WHERE cv.MaDuAn = @da AND pc.MaThanhVien = @tv",
                P("@da", maDuAn), P("@tv", maThanhVien));
            return r == null || r == DBNull.Value ? 0m : Convert.ToDecimal(r);
        }

        /// <summary>% hoàn thành chung của cả dự án (ExecuteScalar).</summary>
        public decimal TyLeDuAn(int maDuAn)
        {
            object r = ExecuteScalar(@"
                SELECT CAST(100.0 * SUM(CASE WHEN TrangThai = N'Done' THEN 1 ELSE 0 END) / NULLIF(COUNT(*), 0) AS DECIMAL(5,2))
                FROM CongViec WHERE MaDuAn = @da", P("@da", maDuAn));
            return r == null || r == DBNull.Value ? 0m : Convert.ToDecimal(r);
        }
    }
}
