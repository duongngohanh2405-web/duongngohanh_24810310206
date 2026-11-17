using System;
using System.Drawing;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class frmThongKe : Form
    {
        private readonly ThongKeBUS bus = new ThongKeBUS();
        private readonly DateTimePicker dtpNgay;
        private readonly NumericUpDown numThang, numNam;
        private readonly Label lblNgay, lblThang;
        private readonly ListView lv;

        public frmThongKe()
        {
            Text = "Báo cáo - Thống kê";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(760, 480);
            StartPosition = FormStartPosition.CenterScreen;

            lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true
            };
            lv.Columns.Add("Hạng", 60);
            lv.Columns.Add("Mã SP", 140);
            lv.Columns.Add("Tên sản phẩm", 360);
            lv.Columns.Add("Số lượng bán", 140);
            Controls.Add(lv);

            var pnl = new Panel { Dock = DockStyle.Top, Height = 170 };
            Controls.Add(pnl);

            // Doanh thu theo ngày
            Ui.AddLabel(pnl, "Doanh thu ngày", 10, 12);
            dtpNgay = new DateTimePicker { Left = 130, Top = 12, Width = 130, Format = DateTimePickerFormat.Short };
            pnl.Controls.Add(dtpNgay);
            Ui.AddButton(pnl, "Xem", 275, 9, 80, btnNgay_Click);
            lblNgay = new Label
            {
                Left = 370, Top = 14, AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), Text = "-"
            };
            pnl.Controls.Add(lblNgay);

            // Doanh thu theo tháng
            Ui.AddLabel(pnl, "Doanh thu tháng", 10, 57);
            numThang = new NumericUpDown { Left = 130, Top = 57, Width = 55, Minimum = 1, Maximum = 12, Value = DateTime.Now.Month };
            pnl.Controls.Add(numThang);
            Ui.AddLabel(pnl, "/", 190, 57);
            numNam = new NumericUpDown { Left = 205, Top = 57, Width = 75, Minimum = 2000, Maximum = 2100, Value = DateTime.Now.Year };
            pnl.Controls.Add(numNam);
            Ui.AddButton(pnl, "Xem", 295, 54, 60, btnThang_Click);
            lblThang = new Label
            {
                Left = 370, Top = 59, AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold), Text = "-"
            };
            pnl.Controls.Add(lblThang);

            // Top 5
            Ui.AddButton(pnl, "Top 5 sản phẩm bán chạy", 10, 105, 220, btnTop5_Click);
        }

        private void btnNgay_Click(object sender, EventArgs e)
        {
            try { lblNgay.Text = bus.DoanhThuNgay(dtpNgay.Value).ToString("N0") + " đ"; }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnThang_Click(object sender, EventArgs e)
        {
            try
            {
                lblThang.Text = bus.DoanhThuThang((int)numThang.Value, (int)numNam.Value).ToString("N0") + " đ";
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void btnTop5_Click(object sender, EventArgs e)
        {
            try
            {
                lv.Items.Clear();
                int hang = 1;
                foreach (ThongKeSanPhamDTO t in bus.Top5BanChay())
                {
                    var item = new ListViewItem(hang.ToString());
                    item.SubItems.Add(t.MaSP);
                    item.SubItems.Add(t.TenSP);
                    item.SubItems.Add(t.SoLuongBan.ToString("N0"));
                    lv.Items.Add(item);
                    hang++;
                }
                if (lv.Items.Count == 0) Ui.ShowInfo("Chưa có dữ liệu bán hàng.");
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }
    }
}
