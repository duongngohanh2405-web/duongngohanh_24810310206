using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace HRM.DAL
{
    /// <summary>
    /// Mô hình ngắt kết nối: SqlDataAdapter nạp DataSet, người dùng sửa trên lưới,
    /// SqlCommandBuilder + adapter.Update() đẩy toàn bộ thay đổi về SQL Server.
    /// </summary>
    public class ChamCongDAL
    {
        private SqlConnection conn;          // giữ lại để adapter dùng lại khi Update
        private SqlDataAdapter adapter;
        private SqlCommandBuilder builder;

        public static bool IsKhoa(string thangNam)
        {
            using (var c = DBHelper.GetConnection())
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM BangChamCong WHERE ThangNam = @t AND DaKhoa = 1", c))
            {
                cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                c.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        /// <summary>Nạp chấm công của tháng vào DataSet; bảng "ChamCong" có thêm cột HoTen (lấy từ bảng cha NhanVien).</summary>
        public DataTable Nap(string thangNam)
        {
            bool khoa = IsKhoa(thangNam);

            if (conn != null) conn.Dispose();
            conn = DBHelper.GetConnection();

            // Tháng chưa khóa: bổ sung dòng chấm công (0 công, 0 OT) cho nhân viên chưa có trong tháng
            if (!khoa)
            {
                using (var cmd = new SqlCommand(
                    @"INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT)
                      SELECT nv.MaNV, @t, 0, 0 FROM NhanVien nv
                      WHERE NOT EXISTS (SELECT 1 FROM BangChamCong c
                                        WHERE c.MaNV = nv.MaNV AND c.ThangNam = @t)", conn))
                {
                    cmd.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }

            var ds = new DataSet();

            // Bảng cha: danh sách nhân viên
            var daNV = new SqlDataAdapter("SELECT MaNV, HoTen FROM NhanVien", conn);
            daNV.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            daNV.Fill(ds, "NhanVien");

            // Bảng chấm công: adapter + CommandBuilder
            var select = new SqlCommand(
                @"SELECT MaCong, MaNV, ThangNam, SoNgayCong, SoGioOT
                  FROM BangChamCong WHERE ThangNam = @t ORDER BY MaNV", conn);
            select.Parameters.Add("@t", SqlDbType.VarChar, 7).Value = thangNam;

            adapter = new SqlDataAdapter(select);
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            builder = new SqlCommandBuilder(adapter);
            builder.ConflictOption = ConflictOption.OverwriteChanges;   // chỉ so khóa chính, không so mọi cột
            adapter.Fill(ds, "ChamCong");

            // Quan hệ cha-con để hiển thị họ tên (cột tính toán, không ghi về CSDL)
            ds.Relations.Add(new DataRelation("NV_ChamCong",
                ds.Tables["NhanVien"].Columns["MaNV"],
                ds.Tables["ChamCong"].Columns["MaNV"], false));
            var colTen = new DataColumn("HoTen", typeof(string), "Parent(NV_ChamCong).HoTen");
            ds.Tables["ChamCong"].Columns.Add(colTen);
            colTen.SetOrdinal(2);

            return ds.Tables["ChamCong"];
        }

        /// <summary>Đồng bộ mọi thay đổi trong DataTable về SQL Server trong một lần Update.</summary>
        public int Luu(DataTable table, string thangNam)
        {
            if (adapter == null) throw new Exception("Chưa nạp dữ liệu chấm công.");
            if (IsKhoa(thangNam))
                throw new Exception("Tháng " + thangNam + " đã chốt lương, không thể sửa chấm công.");
            return adapter.Update(table);
        }
    }
}
