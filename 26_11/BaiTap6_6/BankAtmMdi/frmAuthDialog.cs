namespace BankAtmMdi
{
    /// <summary>Hộp thoại nhập mật khẩu dùng cho Khóa màn hình / Đăng nhập lại.</summary>
    public class frmAuthDialog : Form
    {
        private readonly TextBox _txtPassword = new TextBox();
        private readonly Label _lblError = new Label();
        private readonly string _password;
        private readonly bool _allowCancel;

        public frmAuthDialog(string title, string message, string password, bool allowCancel)
        {
            _password = password;
            _allowCancel = allowCancel;

            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ControlBox = allowCancel;
            ClientSize = new Size(390, 175);

            var lblMsg = new Label { Text = message, Left = 15, Top = 15, Width = 360, Height = 40 };
            _txtPassword.SetBounds(15, 62, 360, 27);
            _txtPassword.UseSystemPasswordChar = true;
            _lblError.SetBounds(15, 94, 360, 20);
            _lblError.ForeColor = Color.Firebrick;

            var btnOk = new Button { Text = "Xác nhận", Width = 100, Height = 32 };
            btnOk.Location = new Point(allowCancel ? 160 : 275, 128);
            btnOk.Click += (s, e) =>
            {
                if (_txtPassword.Text == _password)
                {
                    DialogResult = DialogResult.OK;
                }
                else
                {
                    _lblError.Text = "Mật khẩu không đúng, vui lòng thử lại.";
                    _txtPassword.Clear();
                    _txtPassword.Focus();
                }
            };
            Controls.AddRange(new Control[] { lblMsg, _txtPassword, _lblError, btnOk });
            AcceptButton = btnOk;

            if (allowCancel)
            {
                var btnCancel = new Button { Text = "Hủy", Width = 100, Height = 32, DialogResult = DialogResult.Cancel };
                btnCancel.Location = new Point(275, 128);
                Controls.Add(btnCancel);
                CancelButton = btnCancel;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Màn hình khóa: không cho đóng nếu chưa nhập đúng mật khẩu
            if (!_allowCancel && DialogResult != DialogResult.OK)
                e.Cancel = true;
            base.OnFormClosing(e);
        }
    }
}
