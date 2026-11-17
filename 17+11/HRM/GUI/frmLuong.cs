using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HRM.BUS;
using HRM.DTO;

namespace HRM.GUI
{
    public class frmLuong : Form
    {
        private readonly LuongBUS bus = new LuongBUS();
        private readonly DateTimePicker dtpThang;
        private readonly Button btnTinh, btnChot;
        private readonly Label lblTrangThai, lblTong;
        private readonly DataGridView dgv;

        public frmLuong()
        {
            Text = "Tính lương & chốt lương";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(960, 560);
            StartPosition = FormStartPosition.CenterScreen;

            dgv = Ui.NewGrid();
            dgv.AutoGenerateColumns = false;
            AddCol("MaNV", "Mã NV", null);
            AddCol("HoTen", "Họ tên", null);
            AddCol("LuongCoBan", "Lương cơ bản", "N0");
            AddCol("HeSoLuong", "Hệ số", "0.0#");
            AddCol("SoNgayCong", "Ngày công", "0.##");
            AddCol("SoGioOT", "Giờ OT", "0.##");
            AddCol("ThucLinh", "Thực lĩnh", "N0");
            Controls.Add(dgv);

            var pnlDuoi = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            Controls.Add(pnlDuoi);
            lblTong = new Label
            {
                Left = 10, Top = 12, AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = "Tổng thực lĩnh: 0 đ"
            };
            pnlDuoi.Controls.Add(lblTong);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 85 };
            Controls.Add(pnl);

            Ui.AddLabel(pnl, "Tháng/Năm", 10, 12);
            dtpThang = Ui.AddMonthPicker(pnl, 100, 12, 110);
            dtpThang.ValueChanged += (s, e) => XoaLuoi();
            btnTinh = Ui.AddButton(pnl, "Tính lương (xem trước)", 230, 9, 200, btnTinh_Click);
            btnChot = Ui.AddButton(pnl, "Chốt lương tháng", 440, 9, 170, btnChot_Click);
            btnChot.Enabled = false;

            lblTrangThai = new Label
            {
                Left = 10, Top = 55, AutoSize = true,
                Text = "ThucLinh = (LuongCoBan × HeSoLuong / 22) × SoNgayCong + SoGioOT × 50.000"
            };
            pnl.Controls.Add(lblTrangThai);
        }

        private void AddCol(string prop, string header, string format)
        {
            var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, Name = prop };
            if (format != null)
            {
                c.DefaultCellStyle.Format = format;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgv.Columns.Add(c);
        }

        private void XoaLuoi()
        {
            dgv.DataSource = null;
            btnChot.Enabled = false;
            lblTong.Text = "Tổng thực lĩnh: 0 đ";
        }

        private void btnTinh_Click(object sender, EventArgs e)
        {
            try
            {
                string t = Ui.ToThangNam(dtpThang.Value);
                bool khoa = bus.IsKhoa(t);
                List<LuongChiTietDTO> list = bus.XemLuong(t);

                dgv.DataSource = list;
                lblTong.Text = "Tổng thực lĩnh: " + list.Sum(c => c.ThucLinh).ToString("N0") + " đ";

                if (list.Count == 0)
                {
                    lblTrangThai.Text = "Tháng " + t + " chưa có dữ liệu chấm công. Hãy vào \"Chấm công tháng\" nạp và lưu trước.";
                    btnChot.Enabled = false;
                }
                else if (khoa)
                {
                    lblTrangThai.Text = "Tháng " + t + " ĐÃ CHỐT - đang hiển thị bảng lương đã lưu.";
                    btnChot.Enabled = false;
                }
                else
                {
                    lblTrangThai.Text = "Tháng " + t + " CHƯA CHỐT - đây là số liệu tính thử. Kiểm tra rồi bấm \"Chốt lương tháng\".";
                    btnChot.Enabled = true;
                }
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnChot_Click(object sender, EventArgs e)
        {
            string t = Ui.ToThangNam(dtpThang.Value);
            if (MessageBox.Show(
                "Chốt lương tháng " + t + "?\n\nSau khi chốt, dữ liệu chấm công của tháng này sẽ bị khóa và không thể sửa.",
                "Xác nhận chốt lương", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                int n = bus.ChotLuong(t);   // SqlTransaction: ghi BangLuong + khóa chấm công
                Ui.ShowInfo("Đã chốt lương tháng " + t + " cho " + n + " nhân viên.");
                btnTinh_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Chốt lương thất bại, giao dịch đã được hoàn tác.\n\n" + ErrorHelper.ToMessage(ex),
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
