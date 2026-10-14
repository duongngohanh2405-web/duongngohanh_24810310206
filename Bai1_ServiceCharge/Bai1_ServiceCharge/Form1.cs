namespace Bai1_ServiceCharge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTinhTien_Click(object sender, EventArgs e)
        {

            decimal donGia;
            int soLuong;
            decimal giamGia;

            if (string.IsNullOrWhiteSpace(txtDonGia.Text) ||
                !decimal.TryParse(txtDonGia.Text, out donGia) ||
                donGia < 0)
            {
                MessageBox.Show("Vui lòng nhập đơn giá hợp lệ!");
                txtDonGia.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSoLuong.Text) ||
                !int.TryParse(txtSoLuong.Text, out soLuong) ||
                soLuong <= 0)
            {
                MessageBox.Show("Vui lòng nhập số lượng khách hợp lệ!");
                txtSoLuong.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGiamGia.Text))
            {
                giamGia = 0;
            }
            else if (!decimal.TryParse(txtGiamGia.Text, out giamGia) ||
                     giamGia < 0 || giamGia > 100)
            {
                MessageBox.Show("Giảm giá phải từ 0 đến 100%!");
                txtGiamGia.Focus();
                return;
            }

            decimal tongTien = donGia * soLuong * (100 - giamGia) / 100;

            lblTongTien.Text = "Tổng tiền: " +
                tongTien.ToString("N0") + " VNĐ";

        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {

            txtDonGia.Clear();
            txtSoLuong.Clear();
            txtGiamGia.Clear();

            lblTongTien.Text = "Tổng tiền: 0 VNĐ";

            txtDonGia.Focus();

        }
    }
}
