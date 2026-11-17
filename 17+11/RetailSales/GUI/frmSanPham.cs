using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class frmSanPham : Form
    {
        private readonly SanPhamBUS bus = new SanPhamBUS();
        private readonly TextBox txtMa, txtTen, txtGia, txtTon, txtTim;
        private readonly DataGridView dgv;

        public frmSanPham()
        {
            Text = "Quản lý sản phẩm";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(900, 520);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.CellClick += dgv_CellClick;
            Controls.Add(dgv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 160 };
            Controls.Add(pnl);

            Ui.AddLabel(pnl, "Mã SP (mã vạch)", 10, 10);
            txtMa = Ui.AddText(pnl, 130, 10, 160);
            Ui.AddLabel(pnl, "Tên SP", 320, 10);
            txtTen = Ui.AddText(pnl, 390, 10, 300);

            Ui.AddLabel(pnl, "Giá bán", 10, 45);
            txtGia = Ui.AddText(pnl, 130, 45, 160);
            Ui.AddLabel(pnl, "Số lượng tồn", 320, 45);
            txtTon = Ui.AddText(pnl, 420, 45, 100);

            Ui.AddButton(pnl, "Thêm", 10, 82, 90, btnThem_Click);
            Ui.AddButton(pnl, "Sửa", 110, 82, 90, btnSua_Click);
            Ui.AddButton(pnl, "Xóa", 210, 82, 90, btnXoa_Click);
            Ui.AddButton(pnl, "Làm mới", 310, 82, 90, btnLamMoi_Click);

            Ui.AddLabel(pnl, "Tìm (tên hoặc mã)", 10, 122);
            txtTim = Ui.AddText(pnl, 150, 122, 250);
            txtTim.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { btnTim_Click(s, EventArgs.Empty); e.SuppressKeyPress = true; }
            };
            Ui.AddButton(pnl, "Tìm", 410, 119, 90, btnTim_Click);

            Load += (s, e) => LoadData();
        }

        private void LoadData()
        {
            try
            {
                dgv.DataSource = bus.GetAll();
                FormatGrid();
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void FormatGrid()
        {
            Ui.SetHeader(dgv, "MaSP", "Mã SP");
            Ui.SetHeader(dgv, "TenSP", "Tên sản phẩm");
            Ui.SetHeader(dgv, "GiaBan", "Giá bán", "N0");
            Ui.SetHeader(dgv, "SoLuongTon", "Tồn kho", "N0");
        }

        private SanPhamDTO LayTuForm()
        {
            decimal gia;
            int ton;
            if (!decimal.TryParse(txtGia.Text, out gia)) throw new Exception("Giá bán không hợp lệ.");
            if (!int.TryParse(txtTon.Text, out ton)) throw new Exception("Số lượng tồn không hợp lệ.");
            return new SanPhamDTO
            {
                MaSP = txtMa.Text.Trim(),
                TenSP = txtTen.Text.Trim(),
                GiaBan = gia,
                SoLuongTon = ton
            };
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgv.Rows[e.RowIndex];
            txtMa.Text = Convert.ToString(r.Cells["MaSP"].Value);
            txtTen.Text = Convert.ToString(r.Cells["TenSP"].Value);
            txtGia.Text = Convert.ToDecimal(r.Cells["GiaBan"].Value).ToString("0.##");
            txtTon.Text = Convert.ToString(r.Cells["SoLuongTon"].Value);
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            try
            {
                dgv.DataSource = bus.Search(txtTim.Text);
                FormatGrid();
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                bus.Insert(LayTuForm());
                LoadData();
                Ui.ShowInfo("Đã thêm sản phẩm.");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (!bus.Update(LayTuForm())) { Ui.ShowInfo("Không tìm thấy sản phẩm để sửa."); return; }
                LoadData();
                Ui.ShowInfo("Đã cập nhật sản phẩm.");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn sản phẩm cần xóa."); return; }
            if (MessageBox.Show("Xóa sản phẩm này?", "Xác nhận", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                bus.Delete(txtMa.Text.Trim());
                LoadData();
                btnLamMoi_Click(sender, e);
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMa.Clear(); txtTen.Clear(); txtGia.Clear(); txtTon.Clear(); txtTim.Clear();
            LoadData();
        }
    }
}
