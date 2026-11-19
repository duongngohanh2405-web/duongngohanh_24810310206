using System;
using System.Data;
using Microsoft.Data.SqlClient;
using KaraokeManager.DTO;
using static KaraokeManager.DAL.DbHelper;

namespace KaraokeManager.DAL
{
    /// <summary>Mô hình ngắt kết nối: SqlDataAdapter + DataTable.</summary>
    public class ChiTietDichVuDAL
    {
        /// <summary>Nạp dịch vụ đã gọi của hóa đơn vào DataTable (Fill tự mở rồi đóng kết nối).</summary>
        public DataTable LayDanhSach(int maHD)
        {
            var dt = new DataTable("ChiTietDichVu");
            using var cn = CreateConnection();
            using var da = new SqlDataAdapter(@"
                SELECT ct.MaHD, ct.MaDV, dv.TenDV, ct.SoLuong, ct.DonGia
                FROM ChiTietDichVu ct JOIN DichVu dv ON dv.MaDV = ct.MaDV
                WHERE ct.MaHD = @hd ORDER BY dv.TenDV", cn);
            da.SelectCommand.Parameters.Add(P("@hd", maHD));
            da.Fill(dt);

            dt.PrimaryKey = new[] { dt.Columns["MaHD"], dt.Columns["MaDV"] };
            dt.Columns.Add("ThanhTien", typeof(decimal), "SoLuong * DonGia");   // cột tính toán ngay trên DataTable
            return dt;
        }

        /// <summary>
        /// Ghi các thay đổi (sửa số lượng / xóa dòng) từ DataTable về CSDL bằng SqlDataAdapter.Update
        /// trong 1 transaction. Các câu lệnh chỉ có hiệu lực khi hóa đơn chưa thanh toán và (với sửa)
        /// số lượng không vượt tồn kho; nếu không, adapter báo DBConcurrencyException và toàn bộ bị Rollback.
        /// </summary>
        public int LuuThayDoi(DataTable dt)
        {
            if (dt.GetChanges() == null) return 0;

            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                using var da = new SqlDataAdapter();

                da.UpdateCommand = new SqlCommand(@"
                    UPDATE ChiTietDichVu SET SoLuong = @sl
                    WHERE MaHD = @hd AND MaDV = @dv
                      AND EXISTS (SELECT 1 FROM HoaDon WHERE MaHD = @hd AND TrangThai = 0)
                      AND EXISTS (SELECT 1 FROM DichVu WHERE MaDV = @dv AND TonKho >= @sl)", cn, tran);
                da.UpdateCommand.Parameters.Add(new SqlParameter("@sl", SqlDbType.Int) { SourceColumn = "SoLuong" });
                da.UpdateCommand.Parameters.Add(new SqlParameter("@hd", SqlDbType.Int) { SourceColumn = "MaHD", SourceVersion = DataRowVersion.Original });
                da.UpdateCommand.Parameters.Add(new SqlParameter("@dv", SqlDbType.Int) { SourceColumn = "MaDV", SourceVersion = DataRowVersion.Original });
                da.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;

                da.DeleteCommand = new SqlCommand(@"
                    DELETE FROM ChiTietDichVu
                    WHERE MaHD = @hd AND MaDV = @dv
                      AND EXISTS (SELECT 1 FROM HoaDon WHERE MaHD = @hd AND TrangThai = 0)", cn, tran);
                da.DeleteCommand.Parameters.Add(new SqlParameter("@hd", SqlDbType.Int) { SourceColumn = "MaHD", SourceVersion = DataRowVersion.Original });
                da.DeleteCommand.Parameters.Add(new SqlParameter("@dv", SqlDbType.Int) { SourceColumn = "MaDV", SourceVersion = DataRowVersion.Original });
                da.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;

                int n = da.Update(dt);
                tran.Commit();
                return n;
            }
            catch (DBConcurrencyException)
            {
                SafeRollback(tran);
                throw new InvalidOperationException(
                    "Không lưu được: số lượng vượt tồn kho hoặc hóa đơn đã được thanh toán. Mọi thay đổi đã bị hủy, hãy chọn lại phòng để tải lại danh sách.");
            }
            catch { SafeRollback(tran); throw; }
        }

        /// <summary>
        /// Thêm dịch vụ vào hóa đơn (cộng dồn nếu đã gọi). Kiểm tra: hóa đơn chưa thanh toán,
        /// (đã gọi + gọi thêm) không vượt tồn kho. Đơn giá được chốt tại thời điểm gọi lần đầu.
        /// </summary>
        public void ThemDichVu(int maHD, int maDV, int soLuong)
        {
            using var cn = CreateConnection();
            cn.Open();
            using var tran = cn.BeginTransaction();
            try
            {
                // Khóa dòng hóa đơn để không trùng với lúc đang thanh toán
                using (var c = new SqlCommand("SELECT TrangThai FROM HoaDon WITH (UPDLOCK) WHERE MaHD = @hd", cn, tran))
                {
                    c.Parameters.Add(P("@hd", maHD));
                    object tt = c.ExecuteScalar();
                    if (tt == null) throw new InvalidOperationException("Không tìm thấy hóa đơn.");
                    if ((bool)tt) throw new InvalidOperationException("Hóa đơn đã thanh toán, không thể gọi thêm.");
                }

                string ten; decimal gia; int ton;
                using (var c = new SqlCommand("SELECT TenDV, DonGia, TonKho FROM DichVu WHERE MaDV = @dv", cn, tran))
                {
                    c.Parameters.Add(P("@dv", maDV));
                    using var rd = c.ExecuteReader();
                    if (!rd.Read()) throw new InvalidOperationException("Không tìm thấy dịch vụ.");
                    ten = rd.GetString(0); gia = rd.GetDecimal(1); ton = rd.GetInt32(2);
                }

                object daGoiObj;
                using (var c = new SqlCommand("SELECT SoLuong FROM ChiTietDichVu WHERE MaHD = @hd AND MaDV = @dv", cn, tran))
                {
                    c.Parameters.Add(P("@hd", maHD));
                    c.Parameters.Add(P("@dv", maDV));
                    daGoiObj = c.ExecuteScalar();
                }
                int daGoi = daGoiObj == null ? 0 : (int)daGoiObj;
                if (daGoi + soLuong > ton) throw new KhoKhongDuException(ten, daGoi + soLuong, ton);

                string sql = daGoiObj == null
                    ? "INSERT ChiTietDichVu (MaHD, MaDV, SoLuong, DonGia) VALUES (@hd, @dv, @sl, @gia)"
                    : "UPDATE ChiTietDichVu SET SoLuong = SoLuong + @sl WHERE MaHD = @hd AND MaDV = @dv";
                using (var c = new SqlCommand(sql, cn, tran))
                {
                    c.Parameters.Add(P("@hd", maHD));
                    c.Parameters.Add(P("@dv", maDV));
                    c.Parameters.Add(P("@sl", soLuong));
                    c.Parameters.Add(P("@gia", gia));
                    c.ExecuteNonQuery();
                }
                tran.Commit();
            }
            catch { SafeRollback(tran); throw; }
        }
    }
}
