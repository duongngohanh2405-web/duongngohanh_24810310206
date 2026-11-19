using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ProjectTaskManager.DTO;
using static ProjectTaskManager.DAL.DbHelper;

namespace ProjectTaskManager.DAL
{
    public class CongViecDAL
    {
        // Subquery gom tên người phụ trách (STRING_AGG: SQL Server 2017+)
        private const string SelectBase = @"
            SELECT cv.MaTask, cv.MaDuAn, cv.TenTask, cv.MoTa, cv.HanNop, cv.DoUuTien, cv.TrangThai,
                   da.TenDuAn,
                   (SELECT STRING_AGG(tv.HoTen, N', ')
                      FROM PhanCong pc JOIN ThanhVien tv ON tv.MaThanhVien = pc.MaThanhVien
                     WHERE pc.MaTask = cv.MaTask) AS NguoiPhuTrach
            FROM CongViec cv JOIN DuAn da ON da.MaDuAn = cv.MaDuAn ";

        private static CongViecDTO Map(SqlDataReader rd) => new CongViecDTO
        {
            MaTask = rd.GetInt32(0),
            MaDuAn = rd.GetInt32(1),
            TenTask = rd.GetString(2),
            MoTa = rd.IsDBNull(3) ? null : rd.GetString(3),
            HanNop = rd.IsDBNull(4) ? null : rd.GetDateTime(4),
            DoUuTien = rd.IsDBNull(5) ? null : rd.GetString(5),
            TrangThai = rd.GetString(6),
            TenDuAn = rd.GetString(7),
            NguoiPhuTrach = rd.IsDBNull(8) ? null : rd.GetString(8)
        };

        /// <summary>Lọc theo dự án và (tuỳ chọn) trạng thái. trangThai = null -> tất cả.</summary>
        public List<CongViecDTO> GetByDuAn(int maDuAn, string trangThai)
        {
            var list = new List<CongViecDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(
                SelectBase + "WHERE cv.MaDuAn=@da AND (@tt IS NULL OR cv.TrangThai=@tt) ORDER BY cv.HanNop", cn);
            cmd.Parameters.Add(P("@da", maDuAn));
            cmd.Parameters.Add(P("@tt", trangThai));
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(Map(rd));
            return list;
        }

        /// <summary>Công việc quá hạn: HanNop &lt; hiện tại và TrangThai != 'Done' (dùng SqlDataReader).</summary>
        public List<CongViecDTO> GetOverdue(int? maDuAn)
        {
            var list = new List<CongViecDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(
                SelectBase + @"WHERE cv.HanNop < GETDATE() AND cv.TrangThai <> N'Done'
                               AND (@da IS NULL OR cv.MaDuAn=@da) ORDER BY cv.HanNop", cn);
            cmd.Parameters.Add(P("@da", maDuAn));
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(Map(rd));
            return list;
        }

        public int Insert(CongViecDTO t) => (int)ExecuteScalar(
            @"INSERT CongViec (MaDuAn, TenTask, MoTa, HanNop, DoUuTien, TrangThai)
              VALUES (@da, @ten, @mt, @han, @uu, @tt);
              SELECT CAST(SCOPE_IDENTITY() AS INT);",
            P("@da", t.MaDuAn), P("@ten", t.TenTask), P("@mt", t.MoTa),
            P("@han", t.HanNop), P("@uu", t.DoUuTien), P("@tt", t.TrangThai ?? TrangThaiTask.ToDo));

        public int Update(CongViecDTO t) => ExecuteNonQuery(
            @"UPDATE CongViec SET TenTask=@ten, MoTa=@mt, HanNop=@han, DoUuTien=@uu WHERE MaTask=@ma",
            P("@ten", t.TenTask), P("@mt", t.MoTa), P("@han", t.HanNop), P("@uu", t.DoUuTien), P("@ma", t.MaTask));

        /// <summary>Chuyển trạng thái bằng Parameterized SQL UPDATE.</summary>
        public int UpdateTrangThai(int maTask, string trangThaiMoi) => ExecuteNonQuery(
            "UPDATE CongViec SET TrangThai=@tt WHERE MaTask=@ma",
            P("@tt", trangThaiMoi), P("@ma", maTask));

        public int Delete(int maTask) => ExecuteNonQuery("DELETE FROM CongViec WHERE MaTask=@ma", P("@ma", maTask));
    }
}
