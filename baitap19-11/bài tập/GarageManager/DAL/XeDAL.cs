using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using GarageManager.DTO;
using static GarageManager.DAL.DbHelper;

namespace GarageManager.DAL
{
    public class XeDAL
    {
        private const string SelectBase = @"
            SELECT x.BienSo, x.MaKH, x.HieuXe, x.NamSanXuat, k.TenKH, k.DienThoai, k.DiaChi
            FROM Xe x JOIN KhachHang k ON k.MaKH = x.MaKH ";

        private static XeDTO Map(SqlDataReader rd) => new XeDTO
        {
            BienSo = rd.GetString(0),
            MaKH = rd.GetInt32(1),
            HieuXe = rd.IsDBNull(2) ? null : rd.GetString(2),
            NamSanXuat = rd.IsDBNull(3) ? null : rd.GetInt32(3),
            TenKH = rd.GetString(4),
            DienThoai = rd.IsDBNull(5) ? null : rd.GetString(5),
            DiaChi = rd.IsDBNull(6) ? null : rd.GetString(6)
        };

        public List<XeDTO> GetAll()
        {
            var list = new List<XeDTO>();
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(SelectBase + "ORDER BY x.BienSo", cn);
            cn.Open();
            using var rd = cmd.ExecuteReader();
            while (rd.Read()) list.Add(Map(rd));
            return list;
        }

        /// <summary>Tra cứu biển số bằng SqlParameter. Không có -> null.</summary>
        public XeDTO GetByBienSo(string bienSo)
        {
            using var cn = CreateConnection();
            using var cmd = new SqlCommand(SelectBase + "WHERE x.BienSo = @bs", cn);
            cmd.Parameters.Add(P("@bs", bienSo));
            cn.Open();
            using var rd = cmd.ExecuteReader();
            return rd.Read() ? Map(rd) : null;
        }

        /// <summary>
        /// Tiếp nhận xe mới trong 1 SqlTransaction: tìm/tạo chủ xe (theo SĐT) rồi tạo xe.
        /// Lỗi ở bất kỳ bước nào -> Rollback, không để lại chủ xe "mồ côi".
        /// </summary>
        public void TiepNhanXeMoi(KhachHangDTO kh, XeDTO xe)
        {
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                int maKH = 0;
                if (!string.IsNullOrEmpty(kh.DienThoai))
                {
                    using var find = new SqlCommand("SELECT MaKH FROM KhachHang WHERE DienThoai = @dt", cn, tran);
                    find.Parameters.Add(P("@dt", kh.DienThoai));
                    object o = find.ExecuteScalar();
                    if (o != null) maKH = (int)o;           // chủ xe cũ mang thêm xe mới
                }
                if (maKH == 0)
                {
                    using var ins = new SqlCommand(
                        @"INSERT KhachHang (TenKH, DienThoai, DiaChi) VALUES (@ten, @dt, @dc);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tran);
                    ins.Parameters.Add(P("@ten", kh.TenKH));
                    ins.Parameters.Add(P("@dt", kh.DienThoai));
                    ins.Parameters.Add(P("@dc", kh.DiaChi));
                    maKH = (int)ins.ExecuteScalar();
                }
                using var insXe = new SqlCommand(
                    "INSERT Xe (BienSo, MaKH, HieuXe, NamSanXuat) VALUES (@bs, @ma, @hx, @nam)", cn, tran);
                insXe.Parameters.Add(P("@bs", xe.BienSo));
                insXe.Parameters.Add(P("@ma", maKH));
                insXe.Parameters.Add(P("@hx", xe.HieuXe));
                insXe.Parameters.Add(P("@nam", xe.NamSanXuat));
                insXe.ExecuteNonQuery();

                tran.Commit();
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                SafeRollback(tran);
                throw new ArgumentException("Biển số hoặc số điện thoại đã tồn tại trong hệ thống.");
            }
            catch { SafeRollback(tran); throw; }
        }

        /// <summary>Cập nhật thông tin xe và chủ xe của xe đó (1 transaction).</summary>
        public void CapNhat(KhachHangDTO kh, XeDTO xe)
        {
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                using var c1 = new SqlCommand(
                    @"UPDATE KhachHang SET TenKH=@ten, DienThoai=@dt, DiaChi=@dc
                      WHERE MaKH = (SELECT MaKH FROM Xe WHERE BienSo=@bs)", cn, tran);
                c1.Parameters.Add(P("@ten", kh.TenKH));
                c1.Parameters.Add(P("@dt", kh.DienThoai));
                c1.Parameters.Add(P("@dc", kh.DiaChi));
                c1.Parameters.Add(P("@bs", xe.BienSo));
                c1.ExecuteNonQuery();

                using var c2 = new SqlCommand("UPDATE Xe SET HieuXe=@hx, NamSanXuat=@nam WHERE BienSo=@bs", cn, tran);
                c2.Parameters.Add(P("@hx", xe.HieuXe));
                c2.Parameters.Add(P("@nam", xe.NamSanXuat));
                c2.Parameters.Add(P("@bs", xe.BienSo));
                if (c2.ExecuteNonQuery() == 0) throw new InvalidOperationException("Không tìm thấy xe để cập nhật.");

                tran.Commit();
            }
            catch (SqlException ex) when (IsDuplicate(ex))
            {
                SafeRollback(tran);
                throw new ArgumentException("Số điện thoại đã thuộc về chủ xe khác.");
            }
            catch { SafeRollback(tran); throw; }
        }

        public int Delete(string bienSo)
        {
            try { return ExecuteNonQuery("DELETE FROM Xe WHERE BienSo=@bs", P("@bs", bienSo)); }
            catch (SqlException ex) when (IsForeignKey(ex))
            {
                throw new InvalidOperationException("Xe đã có phiếu sửa chữa nên không thể xóa.");
            }
        }

        private static void SafeRollback(SqlTransaction t) { try { t.Rollback(); } catch { } }
    }
}
