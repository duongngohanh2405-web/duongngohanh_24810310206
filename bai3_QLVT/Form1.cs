using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace QuanLyVatTu
{
    public class Form1 : Form
    {
        private TextBox txtMaVT;
        private TextBox txtTenVT;
        private ComboBox cboDonVi;
        private TextBox txtDonGia;
        private ListView lvVatTu;
        private Button btnThem;
        private Button btnCapNhat;
        private Button btnXoaDong;
        private Button btnXoaTatCa;

        public Form1()
        {
            InitializeForm();
            TaoGiaoDien();
        }

        private void InitializeForm()
        {
            Text = "Quản lý danh mục Vật tư / Linh kiện";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1000, 570);
            MinimumSize = new Size(850, 500);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.FromArgb(245, 247, 250);
        }

        private void TaoGiaoDien()
        {
            Label lblTieuDe = new Label
            {
                Text = "QUẢN LÝ DANH MỤC VẬT TƯ / LINH KIỆN",
                Font = new Font("Segoe UI", 17F, FontStyle.Bold),
                ForeColor = Color.FromArgb(31, 78, 121),
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(lblTieuDe);

            GroupBox grpThongTin = new GroupBox
            {
                Text = "Thông tin vật tư",
                Location = new Point(20, 75),
                Size = new Size(300, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            Controls.Add(grpThongTin);

            txtMaVT = TaoTextBox(30, 55);
            txtTenVT = TaoTextBox(30, 120);
            cboDonVi = new ComboBox
            {
                Location = new Point(30, 185),
                Size = new Size(235, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboDonVi.Items.AddRange(new object[] { "Cái", "Bộ", "Kg", "Mết" });
            txtDonGia = TaoTextBox(30, 250);

            ThemNhan(grpThongTin, "Mã vật tư", 30, 30);
            ThemNhan(grpThongTin, "Tên vật tư", 30, 95);
            ThemNhan(grpThongTin, "Đơn vị tính", 30, 160);
            ThemNhan(grpThongTin, "Đơn giá nhập", 30, 225);

            grpThongTin.Controls.Add(txtMaVT);
            grpThongTin.Controls.Add(txtTenVT);
            grpThongTin.Controls.Add(cboDonVi);
            grpThongTin.Controls.Add(txtDonGia);

            GroupBox grpDanhSach = new GroupBox
            {
                Text = "Danh sách vật tư",
                Location = new Point(340, 75),
                Size = new Size(620, 340),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(grpDanhSach);

            lvVatTu = new ListView
            {
                Location = new Point(15, 28),
                Size = new Size(590, 295),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            lvVatTu.Columns.Add("Mã VT", 100);
            lvVatTu.Columns.Add("Tên VT", 210);
            lvVatTu.Columns.Add("Đơn vị tính", 110);
            lvVatTu.Columns.Add("Đơn giá", 140);
            lvVatTu.SelectedIndexChanged += lvVatTu_SelectedIndexChanged;
            grpDanhSach.Controls.Add(lvVatTu);

            btnThem = TaoNut("Thêm mới", 20, 440, Color.FromArgb(39, 133, 75));
            btnCapNhat = TaoNut("Cập nhật", 250, 440, Color.FromArgb(36, 105, 180));
            btnXoaDong = TaoNut("Xóa dòng", 480, 440, Color.FromArgb(210, 125, 30));
            btnXoaTatCa = TaoNut("Xóa toàn bộ", 710, 440, Color.FromArgb(185, 55, 55));

            btnThem.Click += btnThem_Click;
            btnCapNhat.Click += btnCapNhat_Click;
            btnXoaDong.Click += btnXoaDong_Click;
            btnXoaTatCa.Click += btnXoaTatCa_Click;

            Controls.AddRange(new Control[] { btnThem, btnCapNhat, btnXoaDong, btnXoaTatCa });
        }

        private TextBox TaoTextBox(int x, int y)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(235, 30)
            };
        }

        private void ThemNhan(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true
            });
        }

        private Button TaoNut(string text, int x, int y, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(200, 45),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
        }

        private bool KiemTraDuLieu(out decimal donGia)
        {
            donGia = 0;

            if (string.IsNullOrWhiteSpace(txtMaVT.Text))
            {
                MessageBox.Show("Vui lòng nhập mã vật tư!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaVT.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTenVT.Text))
            {
                MessageBox.Show("Vui lòng nhập tên vật tư!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenVT.Focus();
                return false;
            }

            if (cboDonVi.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn đơn vị tính!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboDonVi.Focus();
                return false;
            }

            string giaNhap = txtDonGia.Text.Trim().Replace(",", "").Replace(".", "");
            if (!decimal.TryParse(giaNhap, NumberStyles.Number,
                CultureInfo.InvariantCulture, out donGia) || donGia < 0)
            {
                MessageBox.Show("Đơn giá phải là số không âm!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDonGia.Focus();
                return false;
            }

            return true;
        }

        private bool MaVatTuDaTonTai(string maVT, ListViewItem boQua = null)
        {
            foreach (ListViewItem item in lvVatTu.Items)
            {
                if (item != boQua && string.Equals(item.Text.Trim(), maVT.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu(out decimal donGia))
                return;

            if (MaVatTuDaTonTai(txtMaVT.Text))
            {
                MessageBox.Show("Mã vật tư đã tồn tại!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaVT.Focus();
                return;
            }

            ListViewItem dong = new ListViewItem(txtMaVT.Text.Trim());
            dong.SubItems.Add(txtTenVT.Text.Trim());
            dong.SubItems.Add(cboDonVi.Text);
            dong.SubItems.Add(donGia.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")));
            lvVatTu.Items.Add(dong);

            XoaTrang();
            MessageBox.Show("Thêm vật tư thành công!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void lvVatTu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvVatTu.SelectedItems.Count == 0)
                return;

            ListViewItem dong = lvVatTu.SelectedItems[0];
            txtMaVT.Text = dong.SubItems[0].Text;
            txtTenVT.Text = dong.SubItems[1].Text;
            cboDonVi.SelectedItem = dong.SubItems[2].Text;
            txtDonGia.Text = dong.SubItems[3].Text.Replace(".", "").Replace(",", "");
        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            if (lvVatTu.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn vật tư cần cập nhật!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!KiemTraDuLieu(out decimal donGia))
                return;

            ListViewItem dong = lvVatTu.SelectedItems[0];

            if (MaVatTuDaTonTai(txtMaVT.Text, dong))
            {
                MessageBox.Show("Mã vật tư đã tồn tại!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaVT.Focus();
                return;
            }

            dong.SubItems[0].Text = txtMaVT.Text.Trim();
            dong.SubItems[1].Text = txtTenVT.Text.Trim();
            dong.SubItems[2].Text = cboDonVi.Text;
            dong.SubItems[3].Text = donGia.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"));

            XoaTrang();
            MessageBox.Show("Cập nhật thành công!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnXoaDong_Click(object sender, EventArgs e)
        {
            if (lvVatTu.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn dòng cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult kq = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa vật tư đang chọn không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (kq == DialogResult.Yes)
            {
                lvVatTu.Items.Remove(lvVatTu.SelectedItems[0]);
                XoaTrang();
            }
        }

        private void btnXoaTatCa_Click(object sender, EventArgs e)
        {
            if (lvVatTu.Items.Count == 0)
            {
                MessageBox.Show("Danh sách đang trống!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult kq = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa toàn bộ danh sách vật tư không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (kq == DialogResult.Yes)
            {
                lvVatTu.Items.Clear();
                XoaTrang();
            }
        }

        private void XoaTrang()
        {
            lvVatTu.SelectedItems.Clear();
            txtMaVT.Clear();
            txtTenVT.Clear();
            cboDonVi.SelectedIndex = -1;
            txtDonGia.Clear();
            txtMaVT.Focus();
        }
    }
}
