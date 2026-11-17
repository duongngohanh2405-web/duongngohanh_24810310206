using System;
using System.Drawing;
using System.Windows.Forms;

namespace HRM.GUI
{
    public class frmMain : Form
    {
        public frmMain()
        {
            Text = "HRM & Payroll - Quản lý nhân sự và tính lương";
            Font = new Font("Segoe UI", 11F);
            ClientSize = new Size(400, 380);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Controls.Add(new Label
            {
                Text = "QUẢN LÝ NHÂN SỰ & TÍNH LƯƠNG",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 0, Top = 15, Width = 400, Height = 30
            });

            AddMenu("Quản lý phòng ban", 60, () => new frmPhongBan());
            AddMenu("Quản lý hồ sơ nhân viên", 120, () => new frmNhanVien());
            AddMenu("Chấm công tháng", 180, () => new frmChamCong());
            AddMenu("Tính lương & chốt lương", 240, () => new frmLuong());
        }

        private void AddMenu(string text, int top, Func<Form> create)
        {
            var b = new Button { Text = text, Left = 60, Top = top, Width = 280, Height = 45 };
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
