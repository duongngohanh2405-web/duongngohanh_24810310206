using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class frmKhachHang : Form
    {
        private readonly KhachHangBUS bus = new KhachHangBUS();
        private readonly TextBox txtMa, txtTen, txtSdt, txtDiem;
        private readonly DataGridView dgv;

        public frmKhachHang()
        {
            Text = "Quản lý khách hàng";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(800, 480);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.CellClick += dgv_CellClick;
            Controls.Add(dgv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            Controls.Add(pnl);

            Ui.AddLabel(pnl, "Mã KH", 10, 10);
            txtMa = Ui.AddText(pnl, 110, 10, 100);
            txtMa.ReadOnly = true;
            Ui.AddLabel(pnl, "Họ tên", 250, 10);
            txtTen = Ui.AddText(pnl, 320, 10, 300);

            Ui.AddLabel(pnl, "Điện thoại", 10, 45);
            txtSdt = Ui.AddText(pnl, 110, 45, 150);
            Ui.AddLabel(pnl, "Điểm tích lũy", 290, 45);
            txtDiem = Ui.AddText(pnl, 390, 45, 100);
            txtDiem.ReadOnly = true;

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
                Ui.SetHeader(dgv, "MaKH", "Mã KH");
                Ui.SetHeader(dgv, "TenKH", "Họ tên");
                Ui.SetHeader(dgv, "DienThoai", "Điện thoại");
                Ui.SetHeader(dgv, "DiemTichLuy", "Điểm tích lũy", "N0");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgv.Rows[e.RowIndex];
            txtMa.Text = Convert.ToString(r.Cells["MaKH"].Value);
            txtTen.Text = Convert.ToString(r.Cells["TenKH"].Value);
            txtSdt.Text = Convert.ToString(r.Cells["DienThoai"].Value);
            txtDiem.Text = Convert.ToString(r.Cells["DiemTichLuy"].Value);
        }

        private KhachHangDTO LayTuForm()
        {
            int ma;
            int.TryParse(txtMa.Text, out ma);
            return new KhachHangDTO { MaKH = ma, TenKH = txtTen.Text.Trim(), DienThoai = txtSdt.Text.Trim() };
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try { bus.Insert(LayTuForm()); LoadData(); Ui.ShowInfo("Đã thêm khách hàng."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn khách hàng cần sửa."); return; }
            try { bus.Update(LayTuForm()); LoadData(); Ui.ShowInfo("Đã cập nhật khách hàng."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn khách hàng cần xóa."); return; }
            if (MessageBox.Show("Xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { bus.Delete(LayTuForm().MaKH); btnLamMoi_Click(sender, e); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMa.Clear(); txtTen.Clear(); txtSdt.Clear(); txtDiem.Clear();
            LoadData();
        }
    }
}
