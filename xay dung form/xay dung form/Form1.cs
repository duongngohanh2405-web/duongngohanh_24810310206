namespace xay_dung_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            errorProvider1.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            this.AcceptButton = btnLogin;   // Enter -> btnLogin
            this.CancelButton = btnExit;    // Esc -> btnExit

            // Mặc định ẩn mật khẩu
            txtPassword.UseSystemPasswordChar = true;

            // Đăng ký sự kiện
            btnLogin.Click += btnLogin_Click;
            btnExit.Click += btnExit_Click;
            chkShowPassword.CheckedChanged += chkShowPassword_CheckedChanged;
        }

        // Tick/bỏ tick checkbox -> hiện/ẩn mật khẩu
        private void chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        // Kiểm tra hợp lệ dữ liệu
        private bool ValidateInput()
        {
            bool isValid = true;

            errorProvider1.Clear();

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                errorProvider1.SetError(txtUsername, "Tên đăng nhập không được để trống!");
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                errorProvider1.SetError(txtPassword, "Mật khẩu không được để trống!");
                isValid = false;
            }

            return isValid;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                return;
            }

            // TODO: xử lý logic đăng nhập thực tế (kiểm tra tài khoản, mật khẩu...)
            MessageBox.Show("Đăng nhập thành công!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void chkShowPassword_CheckedChanged_1(object sender, EventArgs e)
        {

        }
    }
}
