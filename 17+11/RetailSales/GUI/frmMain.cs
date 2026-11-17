using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetailSales.GUI
{
    public class frmMain : Form
    {
        public frmMain()
        {
            Text = "Retail Sales System - Siêu thị mini";
            Font = new Font("Segoe UI", 11F);
            ClientSize = new Size(380, 400);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var title = new Label
            {
                Text = "HỆ THỐNG BÁN HÀNG SIÊU THỊ MINI",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0, Top = 15, Width = 380, Height = 30
            };
            Controls.Add(title);

            AddMenu("Bán hàng / Lập hóa đơn", 60, () => new frmBanHang());
            AddMenu("Quản lý sản phẩm", 120, () => new frmSanPham());
            AddMenu("Quản lý khách hàng", 180, () => new frmKhachHang());
            AddMenu("Quản lý nhân viên", 240, () => new frmNhanVien());
            AddMenu("Báo cáo - Thống kê", 300, () => new frmThongKe());
        }

        private void AddMenu(string text, int top, Func<Form> create)
        {
            var b = new Button { Text = text, Left = 50, Top = top, Width = 280, Height = 45 };
            b.Click += (s, e) =>
            {
                using (Form f = create())
                {
                    f.ShowDialog(this);
                }
            };
            Controls.Add(b);
        }
    }
}
