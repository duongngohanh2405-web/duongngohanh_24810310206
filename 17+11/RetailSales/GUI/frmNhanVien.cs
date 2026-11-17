using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class frmNhanVien : Form
    {
        private readonly NhanVienBUS bus = new NhanVienBUS();
        private readonly TextBox txtMa, txtTen, txtSdt, txtChucVu;
        private readonly DataGridView dgv;

        public frmNhanVien()
        {
            Text = "Quản lý nhân viên";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(800, 480);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.CellClick += dgv_CellClick;
            Controls.Add(dgv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            Controls.Add(pnl);

            Ui.AddLabel(pnl, "Mã NV", 10, 10);
            txtMa = Ui.AddText(pnl, 110, 10, 100);
            txtMa.ReadOnly = true;
            Ui.AddLabel(pnl, "Họ tên", 250, 10);
            txtTen = Ui.AddText(pnl, 320, 10, 300);

            Ui.AddLabel(pnl, "Điện thoại", 10, 45);
            txtSdt = Ui.AddText(pnl, 110, 45, 150);
            Ui.AddLabel(pnl, "Chức vụ", 290, 45);
            txtChucVu = Ui.AddText(pnl, 360, 45, 200);

            Ui.AddButton(pnl, "Thêm", 10, 85, 90, btnThem_Click);
            Ui.AddButton(pnl, "Sửa", 110, 85, 90, btnSua_Click);
            Ui.AddButton(pnl, "Xóa", 210, 85, 90, btnXoa_Click);
            Ui.AddButton(pnl, "Làm mới", 310, 85, 90, btnLamMoi_Click);

            Load += (s, e) => LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgv.DataSource = bus.GetAll();
                Ui.SetHeader(dgv, "MaNV", "Mã NV");
                Ui.SetHeader(dgv, "TenNV", "Họ tên");
                Ui.SetHeader(dgv, "SoDienThoai", "Điện thoại");
                Ui.SetHeader(dgv, "ChucVu", "Chức vụ");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgv.Rows[e.RowIndex];
            txtMa.Text = Convert.ToString(r.Cells["MaNV"].Value);
            txtTen.Text = Convert.ToString(r.Cells["TenNV"].Value);
            txtSdt.Text = Convert.ToString(r.Cells["SoDienThoai"].Value);
            txtChucVu.Text = Convert.ToString(r.Cells["ChucVu"].Value);
        }

        private NhanVienDTO LayTuForm()
        {
            int ma;
            int.TryParse(txtMa.Text, out ma);
            return new NhanVienDTO
            {
                MaNV = ma,
                TenNV = txtTen.Text.Trim(),
                SoDienThoai = txtSdt.Text.Trim(),
                ChucVu = txtChucVu.Text.Trim()
            };
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try { bus.Insert(LayTuForm()); LoadData(); Ui.ShowInfo("Đã thêm nhân viên."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn nhân viên cần sửa."); return; }
            try { bus.Update(LayTuForm()); LoadData(); Ui.ShowInfo("Đã cập nhật nhân viên."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn nhân viên cần xóa."); return; }
            if (MessageBox.Show("Xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { bus.Delete(LayTuForm().MaNV); btnLamMoi_Click(sender, e); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMa.Clear(); txtTen.Clear(); txtSdt.Clear(); txtChucVu.Clear();
            LoadData();
        }
    }
}
