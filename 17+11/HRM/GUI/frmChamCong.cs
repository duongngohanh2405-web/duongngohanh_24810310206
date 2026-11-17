using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;

namespace HRM.GUI
{
    /// <summary>Chấm công theo mô hình ngắt kết nối: nạp vào DataTable, sửa trên lưới, lưu bằng adapter.Update().</summary>
    public class frmChamCong : Form
    {
        private readonly ChamCongBUS bus = new ChamCongBUS();
        private readonly DateTimePicker dtpThang;
        private readonly Button btnNap, btnLuu;
        private readonly Label lblTrangThai;
        private readonly DataGridView dgv;
        private readonly BindingSource bs = new BindingSource();

        private DataTable table;        // bảng đang nạp (ngắt kết nối)
        private string thangNamDangNap; // tháng của bảng đang nạp

        public frmChamCong()
        {
            Text = "Chấm công tháng";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(860, 560);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.ReadOnly = false;
            dgv.EditMode = DataGridViewEditMode.EditOnEnter;
            dgv.CellValidating += dgv_CellValidating;
            dgv.DataError += (s, e) =>
            {
                e.ThrowException = false;
                Ui.ShowInfo("Giá trị không hợp lệ. Nhập số, ví dụ số thập phân trên máy bạn: " + Ui.ViDuThapPhan());
            };
            Controls.Add(dgv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 85 };
            Controls.Add(pnl);

            Ui.AddLabel(pnl, "Tháng/Năm", 10, 12);
            dtpThang = Ui.AddMonthPicker(pnl, 100, 12, 110);
            dtpThang.ValueChanged += (s, e) => XoaLuoi();
            btnNap = Ui.AddButton(pnl, "Nạp bảng chấm công", 230, 9, 180, btnNap_Click);
            btnLuu = Ui.AddButton(pnl, "Lưu chấm công", 420, 9, 150, btnLuu_Click);
            btnLuu.Enabled = false;

            lblTrangThai = new Label
            {
                Left = 10, Top = 55, AutoSize = true,
                Text = "Chọn tháng rồi bấm \"Nạp bảng chấm công\". Nhập số ngày công và giờ OT trực tiếp trên lưới."
            };
            pnl.Controls.Add(lblTrangThai);

            FormClosing += frmChamCong_FormClosing;
        }

        private void XoaLuoi()
        {
            bs.DataSource = null;
            dgv.DataSource = null;
            table = null;
            btnLuu.Enabled = false;
            lblTrangThai.Text = "Bấm \"Nạp bảng chấm công\" để tải dữ liệu tháng " + Ui.ToThangNam(dtpThang.Value) + ".";
        }

        private bool CoThayDoi()
        {
            if (table == null) return false;
            dgv.EndEdit();
            bs.EndEdit();
            return table.GetChanges() != null;
        }

        private void btnNap_Click(object sender, EventArgs e)
        {
            if (CoThayDoi() &&
                MessageBox.Show("Bảng hiện tại có thay đổi chưa lưu. Nạp lại sẽ mất các thay đổi này. Tiếp tục?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                string t = Ui.ToThangNam(dtpThang.Value);
                bool khoa = bus.IsKhoa(t);
                table = bus.Nap(t);
                thangNamDangNap = t;

                bs.DataSource = table;
                dgv.DataSource = bs;

                Ui.SetHeader(dgv, "MaNV", "Mã NV");
                Ui.SetHeader(dgv, "HoTen", "Họ tên");
                Ui.SetHeader(dgv, "SoNgayCong", "Số ngày công", "0.##");
                Ui.SetHeader(dgv, "SoGioOT", "Số giờ OT", "0.##");
                if (dgv.Columns["MaCong"] != null) dgv.Columns["MaCong"].Visible = false;
                if (dgv.Columns["ThangNam"] != null) dgv.Columns["ThangNam"].Visible = false;
                dgv.Columns["MaNV"].ReadOnly = true;
                dgv.Columns["HoTen"].ReadOnly = true;

                dgv.ReadOnly = khoa;
                btnLuu.Enabled = !khoa;
                lblTrangThai.Text = khoa
                    ? "Tháng " + t + " ĐÃ CHỐT LƯƠNG - chỉ xem, không thể sửa."
                    : "Tháng " + t + ": " + table.Rows.Count + " nhân viên. Sửa trực tiếp trên lưới rồi bấm \"Lưu chấm công\".";
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (table == null) { Ui.ShowInfo("Hãy nạp bảng chấm công trước."); return; }
            try
            {
                dgv.EndEdit();
                bs.EndEdit();
                if (table.GetChanges() == null) { Ui.ShowInfo("Không có thay đổi nào để lưu."); return; }

                int n = bus.Luu(table, thangNamDangNap);   // SqlCommandBuilder + adapter.Update()
                Ui.ShowInfo("Đã lưu " + n + " dòng chấm công về SQL Server.");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || !dgv.IsCurrentCellInEditMode) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            if (col != "SoNgayCong" && col != "SoGioOT") return;

            double v;
            if (!Ui.TryParseDouble(Convert.ToString(e.FormattedValue), out v))
            {
                Ui.ShowInfo("Giá trị không hợp lệ. Ví dụ số thập phân trên máy bạn: " + Ui.ViDuThapPhan());
                e.Cancel = true;
                return;
            }
            if (v < 0 || (col == "SoNgayCong" && v > 31))
            {
                Ui.ShowInfo(col == "SoNgayCong"
                    ? "Số ngày công phải từ 0 đến 31."
                    : "Số giờ OT không được âm.");
                e.Cancel = true;
            }
        }

        private void frmChamCong_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CoThayDoi() &&
                MessageBox.Show("Còn thay đổi chưa lưu. Đóng form sẽ mất các thay đổi này. Đóng?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        }
    }
}
