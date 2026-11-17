using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public class frmNhanVien : Form
    {
        private readonly NhanVienBUS bus = new NhanVienBUS();
        private readonly PhongBanBUS pbBus = new PhongBanBUS();

        private readonly TextBox txtMa, txtTen, txtCCCD, txtLuong, txtHeSo, txtTim;
        private readonly DateTimePicker dtpNgaySinh;
        private readonly ComboBox cboPB, cboLoc;
        private readonly DataGridView dgv;

        public frmNhanVien()
        {
            Text = "Quản lý hồ sơ nhân viên";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(1000, 560);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.CellClick += dgv_CellClick;
            Controls.Add(dgv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 175 };
            Controls.Add(pnl);

            // Hàng 1
            Ui.AddLabel(pnl, "Mã NV", 10, 10);
            txtMa = Ui.AddText(pnl, 90, 10, 70);
            txtMa.ReadOnly = true;
            Ui.AddLabel(pnl, "Họ tên", 190, 10);
            txtTen = Ui.AddText(pnl, 255, 10, 250);
            Ui.AddLabel(pnl, "Ngày sinh", 530, 10);
            dtpNgaySinh = new DateTimePicker
            {
                Left = 610, Top = 10, Width = 140,
                Format = DateTimePickerFormat.Short,
                ShowCheckBox = true,
                Checked = false
            };
            pnl.Controls.Add(dtpNgaySinh);

            // Hàng 2
            Ui.AddLabel(pnl, "CCCD", 10, 45);
            txtCCCD = Ui.AddText(pnl, 90, 45, 170);
            Ui.AddLabel(pnl, "Phòng ban", 290, 45);
            cboPB = Ui.AddCombo(pnl, 375, 45, 170);
            Ui.AddLabel(pnl, "Lương CB", 570, 45);
            txtLuong = Ui.AddText(pnl, 645, 45, 110);
            Ui.AddLabel(pnl, "Hệ số", 775, 45);
            txtHeSo = Ui.AddText(pnl, 825, 45, 60);
            txtHeSo.Text = (1.0).ToString("0.0");

            // Hàng 3: nút chức năng
            Ui.AddButton(pnl, "Thêm", 10, 85, 90, btnThem_Click);
            Ui.AddButton(pnl, "Sửa", 110, 85, 90, btnSua_Click);
            Ui.AddButton(pnl, "Xóa", 210, 85, 90, btnXoa_Click);
            Ui.AddButton(pnl, "Chuyển phòng ban", 310, 85, 160, btnChuyen_Click);
            Ui.AddButton(pnl, "Làm mới", 480, 85, 90, btnLamMoi_Click);

            // Hàng 4: tìm kiếm
            Ui.AddLabel(pnl, "Tìm (tên / CCCD / phòng ban)", 10, 130);
            txtTim = Ui.AddText(pnl, 235, 130, 200);
            txtTim.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { btnTim_Click(s, EventArgs.Empty); e.SuppressKeyPress = true; }
            };
            Ui.AddLabel(pnl, "Lọc phòng ban", 460, 130);
            cboLoc = Ui.AddCombo(pnl, 565, 130, 170);
            Ui.AddButton(pnl, "Tìm", 750, 127, 90, btnTim_Click);

            Load += frmNhanVien_Load;
        }

        private void frmNhanVien_Load(object sender, EventArgs e)
        {
            try
            {
                NapPhongBan();
                TaiDuLieu();
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        // Hai ComboBox dùng hai bản sao riêng để chọn ở ô này không làm ô kia nhảy theo
        private void NapPhongBan()
        {
            DataTable dt = pbBus.GetAll();

            cboPB.DisplayMember = "TenPB";
            cboPB.ValueMember = "MaPB";
            cboPB.DataSource = dt.Copy();
            cboPB.SelectedIndex = -1;

            DataTable dtLoc = dt.Copy();
            DataRow all = dtLoc.NewRow();
            all["MaPB"] = 0;
            all["TenPB"] = "-- Tất cả --";
            dtLoc.Rows.InsertAt(all, 0);
            cboLoc.DisplayMember = "TenPB";
            cboLoc.ValueMember = "MaPB";
            cboLoc.DataSource = dtLoc;
            cboLoc.SelectedIndex = 0;
        }

        private static int LayMa(ComboBox cbo, string cot)
        {
            var drv = cbo.SelectedItem as DataRowView;
            return drv == null ? 0 : Convert.ToInt32(drv[cot]);
        }

        private void TaiDuLieu()
        {
            int pb = LayMa(cboLoc, "MaPB");
            dgv.DataSource = bus.Search(txtTim.Text, pb == 0 ? (int?)null : pb);
            Ui.SetHeader(dgv, "MaNV", "Mã NV");
            Ui.SetHeader(dgv, "HoTen", "Họ tên");
            Ui.SetHeader(dgv, "NgaySinh", "Ngày sinh", "dd/MM/yyyy");
            Ui.SetHeader(dgv, "CCCD", "CCCD");
            Ui.SetHeader(dgv, "TenPB", "Phòng ban");
            Ui.SetHeader(dgv, "LuongCoBan", "Lương cơ bản", "N0");
            Ui.SetHeader(dgv, "HeSoLuong", "Hệ số", "0.0#");
            if (dgv.Columns["MaPB"] != null) dgv.Columns["MaPB"].Visible = false;
        }

        private NhanVienDTO LayTuForm()
        {
            int ma;
            int.TryParse(txtMa.Text, out ma);

            decimal luong;
            if (!Ui.TryParseDecimal(txtLuong.Text, out luong))
                throw new Exception("Lương cơ bản không hợp lệ.");
            double heSo;
            if (!Ui.TryParseDouble(txtHeSo.Text, out heSo))
                throw new Exception("Hệ số lương không hợp lệ. Ví dụ số thập phân trên máy bạn: " + Ui.ViDuThapPhan());

            return new NhanVienDTO
            {
                MaNV = ma,
                HoTen = txtTen.Text.Trim(),
                NgaySinh = dtpNgaySinh.Checked ? (DateTime?)dtpNgaySinh.Value.Date : null,
                CCCD = txtCCCD.Text.Trim(),
                MaPB = LayMa(cboPB, "MaPB"),
                LuongCoBan = luong,
                HeSoLuong = heSo
            };
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgv.Rows[e.RowIndex];
            txtMa.Text = Convert.ToString(r.Cells["MaNV"].Value);
            txtTen.Text = Convert.ToString(r.Cells["HoTen"].Value);
            txtCCCD.Text = Convert.ToString(r.Cells["CCCD"].Value);

            object ns = r.Cells["NgaySinh"].Value;
            if (ns == null || ns == DBNull.Value) dtpNgaySinh.Checked = false;
            else { dtpNgaySinh.Checked = true; dtpNgaySinh.Value = Convert.ToDateTime(ns); }

            cboPB.SelectedValue = Convert.ToInt32(r.Cells["MaPB"].Value);
            txtLuong.Text = Convert.ToDecimal(r.Cells["LuongCoBan"].Value).ToString("0.##");
            txtHeSo.Text = Convert.ToDouble(r.Cells["HeSoLuong"].Value).ToString();
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            try { TaiDuLieu(); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try { bus.Insert(LayTuForm()); TaiDuLieu(); Ui.ShowInfo("Đã thêm nhân viên."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn nhân viên cần sửa."); return; }
            try { bus.Update(LayTuForm()); TaiDuLieu(); Ui.ShowInfo("Đã cập nhật nhân viên."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnChuyen_Click(object sender, EventArgs e)
        {
            int ma;
            if (!int.TryParse(txtMa.Text, out ma)) { Ui.ShowInfo("Hãy chọn nhân viên cần chuyển trên lưới."); return; }
            try
            {
                bus.ChuyenPhongBan(ma, LayMa(cboPB, "MaPB"));
                TaiDuLieu();
                Ui.ShowInfo("Đã chuyển nhân viên sang phòng ban \"" + cboPB.Text + "\".");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            int ma;
            if (!int.TryParse(txtMa.Text, out ma)) { Ui.ShowInfo("Hãy chọn nhân viên cần xóa."); return; }
            if (MessageBox.Show("Xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { bus.Delete(ma); btnLamMoi_Click(sender, e); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMa.Clear(); txtTen.Clear(); txtCCCD.Clear(); txtLuong.Clear(); txtTim.Clear();
            txtHeSo.Text = (1.0).ToString("0.0");
            dtpNgaySinh.Checked = false;
            cboPB.SelectedIndex = -1;
            cboLoc.SelectedIndex = 0;
            try { TaiDuLieu(); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }
    }
}
