using System;
using System.Windows.Forms;

namespace Bai5_1
{
    public partial class frmDangKy : Form
    {
        public frmDangKy()
        {
            InitializeComponent();
        }

        private void frmDangKy_Load(object sender, EventArgs e)
        {
            // Chỉ cho chọn ngày sinh không vượt quá hôm nay
            dtpNgaySinh.MaxDate = DateTime.Today;
            // Không cho biểu tượng lỗi nhấp nháy
            epCheck.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        }

        // Tính tuổi chính xác từ ngày sinh
        private int TinhTuoi(DateTime ngaySinh)
        {
            DateTime homNay = DateTime.Today;
            int tuoi = homNay.Year - ngaySinh.Year;
            if (ngaySinh.Date > homNay.AddYears(-tuoi))
                tuoi--; // chưa đến sinh nhật năm nay
            return tuoi;
        }

        // Kiểm tra toàn bộ dữ liệu, trả về true nếu hợp lệ
        private bool KiemTraHopLe()
        {
            bool hopLe = true;

            // Xóa toàn bộ lỗi cũ trước khi kiểm tra lại
            epCheck.Clear();

            // 1. Tên đăng nhập không được trống
            if (string.IsNullOrWhiteSpace(txtTenDN.Text))
            {
                epCheck.SetError(txtTenDN, "Tên đăng nhập không được để trống!");
                hopLe = false;
            }

            // 2. Mật khẩu không được trống
            if (string.IsNullOrEmpty(txtMatKhau.Text))
            {
                epCheck.SetError(txtMatKhau, "Mật khẩu không được để trống!");
                hopLe = false;
            }

            // 3. Xác nhận mật khẩu phải khớp
            if (txtXacNhan.Text != txtMatKhau.Text)
            {
                epCheck.SetError(txtXacNhan, "Mật khẩu xác nhận không khớp!");
                hopLe = false;
            }

            // 4. Tuổi phải >= 18
            if (TinhTuoi(dtpNgaySinh.Value) < 18)
            {
                epCheck.SetError(dtpNgaySinh, "Bạn phải từ 18 tuổi trở lên!");
                hopLe = false;
            }

            // 5. Phải đồng ý điều khoản
            if (!chkDieuKhoan.Checked)
            {
                epCheck.SetError(chkDieuKhoan, "Bạn phải đồng ý với điều khoản dịch vụ!");
                hopLe = false;
            }

            return hopLe;
        }

        private void btnDangKy_Click(object sender, EventArgs e)
        {
            if (KiemTraHopLe())
            {
                string gioiTinh = rdoNam.Checked ? "Nam" : "Nữ";
                MessageBox.Show(
                    "Đăng ký tài khoản thành công!\n\n" +
                    "Tên đăng nhập: " + txtTenDN.Text.Trim() + "\n" +
                    "Ngày sinh: " + dtpNgaySinh.Value.ToString("dd/MM/yyyy") + "\n" +
                    "Giới tính: " + gioiTinh,
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtTenDN.Clear();
            txtMatKhau.Clear();
            txtXacNhan.Clear();
            dtpNgaySinh.Value = DateTime.Today;
            rdoNam.Checked = true;
            chkDieuKhoan.Checked = false;
            epCheck.Clear();       // xóa mọi biểu tượng lỗi
            txtTenDN.Focus();
        }
    }
}
