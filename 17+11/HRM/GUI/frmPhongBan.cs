using System;
using System.Drawing;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public class frmPhongBan : Form
    {
        private readonly PhongBanBUS bus = new PhongBanBUS();
        private readonly TextBox txtMa, txtTen, txtMoTa;
        private readonly DataGridView dgv;

        public frmPhongBan()
        {
            Text = "Quản lý phòng ban";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(760, 440);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.CellClick += dgv_CellClick;
            Controls.Add(dgv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            Controls.Add(pnl);

            Ui.AddLabel(pnl, "Mã PB", 10, 10);
            txtMa = Ui.AddText(pnl, 90, 10, 80);
            txtMa.ReadOnly = true;
            Ui.AddLabel(pnl, "Tên phòng ban", 200, 10);
            txtTen = Ui.AddText(pnl, 310, 10, 300);

            Ui.AddLabel(pnl, "Mô tả", 10, 45);
            txtMoTa = Ui.AddText(pnl, 90, 45, 520);

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
                Ui.SetHeader(dgv, "MaPB", "Mã PB");
                Ui.SetHeader(dgv, "TenPB", "Tên phòng ban");
                Ui.SetHeader(dgv, "Mota", "Mô tả");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private PhongBanDTO LayTuForm()
        {
            int ma;
            int.TryParse(txtMa.Text, out ma);
            return new PhongBanDTO { MaPB = ma, TenPB = txtTen.Text.Trim(), Mota = txtMoTa.Text.Trim() };
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dgv.Rows[e.RowIndex];
            txtMa.Text = Convert.ToString(r.Cells["MaPB"].Value);
            txtTen.Text = Convert.ToString(r.Cells["TenPB"].Value);
            txtMoTa.Text = Convert.ToString(r.Cells["Mota"].Value);
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            try { bus.Insert(LayTuForm()); LoadData(); Ui.ShowInfo("Đã thêm phòng ban."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn phòng ban cần sửa."); return; }
            try { bus.Update(LayTuForm()); LoadData(); Ui.ShowInfo("Đã cập nhật phòng ban."); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMa.Text)) { Ui.ShowInfo("Hãy chọn phòng ban cần xóa."); return; }
            if (MessageBox.Show("Xóa phòng ban này?", "Xác nhận", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { bus.Delete(LayTuForm().MaPB); btnLamMoi_Click(sender, e); }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMa.Clear(); txtTen.Clear(); txtMoTa.Clear();
            LoadData();
        }
    }
}
