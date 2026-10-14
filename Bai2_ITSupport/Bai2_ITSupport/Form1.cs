namespace Bai2_ITSupport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnTaiAnh_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Chọn ảnh lỗi";
            openFileDialog.Filter = "Ảnh JPG, PNG|*.jpg;*.jpeg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var fs = new System.IO.FileStream(
                        openFileDialog.FileName,
                        System.IO.FileMode.Open,
                        System.IO.FileAccess.Read))
                    {
                        using (var img = System.Drawing.Image.FromStream(fs))
                        {
                            picAnhLoi.Image = new System.Drawing.Bitmap(img);
                        }
                    }

                    picAnhLoi.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                catch
                {
                    MessageBox.Show("Không thể mở ảnh đã chọn!");
                }
            }

        }

        private void btnGuiYeuCau_Click(object sender, EventArgs e)
        {

            string maPhieu = txtMaPhieu.Text.Trim();
            string nguoiYeuCau = txtNguoiYeuCau.Text.Trim();

            if (maPhieu == "")
            {
                MessageBox.Show("Vui lòng nhập mã phiếu!");
                txtMaPhieu.Focus();
                return;
            }

            if (nguoiYeuCau == "")
            {
                MessageBox.Show("Vui lòng nhập người yêu cầu!");
                txtNguoiYeuCau.Focus();
                return;
            }

            if (!rdoThap.Checked &&
                !rdoTrungBinh.Checked &&
                !rdoKhanCap.Checked)
            {
                MessageBox.Show("Vui lòng chọn mức độ ưu tiên!");
                return;
            }

            if (cboLoaiSuCo.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn loại sự cố!");
                return;
            }

            string mucDoUuTien;

            if (rdoThap.Checked)
                mucDoUuTien = "Thấp";
            else if (rdoTrungBinh.Checked)
                mucDoUuTien = "Trung bình";
            else
                mucDoUuTien = "Khẩn cấp";

            string thietBi = "";

            if (chkMayTinhBan.Checked)
                thietBi += "Máy tính bàn, ";

            if (chkLaptop.Checked)
                thietBi += "Laptop, ";

            if (chkMayIn.Checked)
                thietBi += "Máy in, ";

            if (chkDienThoai.Checked)
                thietBi += "Điện thoại, ";

            if (thietBi == "")
                thietBi = "Chưa chọn thiết bị";
            else
                thietBi = thietBi.TrimEnd(',', ' ');

            string anhLoi = picAnhLoi.Image == null
                ? "Chưa tải ảnh"
                : "Đã tải ảnh";

            string thongTin =
                "Mã phiếu: " + maPhieu + "\n" +
                "Người yêu cầu: " + nguoiYeuCau + "\n" +
                "Ngày ghi nhận: " +
                dtpNgayGhiNhan.Value.ToString("dd/MM/yyyy") + "\n" +
                "Mức độ ưu tiên: " + mucDoUuTien + "\n" +
                "Loại sự cố: " + cboLoaiSuCo.SelectedItem + "\n" +
                "Thiết bị ảnh hưởng: " + thietBi + "\n" +
                "Ảnh chụp lỗi: " + anhLoi;

            MessageBox.Show(
                thongTin,
                "Tóm tắt phiếu sự cố IT",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

        }

        private void btnNhapLai_Click(object sender, EventArgs e)
        {

            txtMaPhieu.Clear();
            txtNguoiYeuCau.Clear();

            dtpNgayGhiNhan.Value = DateTime.Today;

            rdoThap.Checked = false;
            rdoTrungBinh.Checked = false;
            rdoKhanCap.Checked = false;

            cboLoaiSuCo.SelectedIndex = -1;

            chkMayTinhBan.Checked = false;
            chkLaptop.Checked = false;
            chkMayIn.Checked = false;
            chkDienThoai.Checked = false;

            if (picAnhLoi.Image != null)
            {
                var anhCu = picAnhLoi.Image;
                picAnhLoi.Image = null;
                anhCu.Dispose();
            }

            txtMaPhieu.Focus();

        }
    }
}
