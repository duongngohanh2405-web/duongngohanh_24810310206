namespace BankAtmMdi
{
    public partial class frmBankAtmMain : Form
    {
        private const string DemoPassword = "123456";
        private const string TellerName = "Nguyễn Văn An (GDV-001)";
        private bool _forceClose;

        public frmBankAtmMain()
        {
            InitializeComponent();
        }

        private void frmBankAtmMain_Load(object sender, EventArgs e)
        {
            lblTeller.Text = "Giao dịch viên trực: " + TellerName;
            lblCore.Text = "Core Banking: Đã kết nối";
            lblCore.ForeColor = Color.DarkGreen;
            lblMsg.Text = "Sẵn sàng";
        }

        // ================= ĐIỀU HƯỚNG MDI (chống mở trùng) =================
        /// <summary>
        /// Duyệt this.MdiChildren: nếu Form con kiểu T đã tồn tại thì Activate(),
        /// ngược lại khởi tạo mới, gán MdiParent = this rồi Show().
        /// </summary>
        private void OpenOrActivate<T>() where T : Form, new()
        {
            foreach (Form child in this.MdiChildren)
            {
                if (child.GetType() == typeof(T))
                {
                    if (child.WindowState == FormWindowState.Minimized)
                        child.WindowState = FormWindowState.Normal;
                    child.Activate();
                    return;
                }
            }

            T frm = new T();
            frm.MdiParent = this;
            frm.Show();
        }

        private void mnuDsTram_Click(object sender, EventArgs e) => OpenOrActivate<frmAtmStations>();
        private void mnuDoiSoat_Click(object sender, EventArgs e) => OpenOrActivate<frmAtmReconciliation>();

        // ================= MENU CỬA SỔ =================
        private void mnuCascade_Click(object sender, EventArgs e) => LayoutMdi(MdiLayout.Cascade);
        private void mnuTileH_Click(object sender, EventArgs e) => LayoutMdi(MdiLayout.TileHorizontal);
        private void mnuTileV_Click(object sender, EventArgs e) => LayoutMdi(MdiLayout.TileVertical);

        private void mnuDongTatCa_Click(object sender, EventArgs e) => CloseAllChildren();

        private void CloseAllChildren()
        {
            foreach (Form child in this.MdiChildren.ToArray())
                child.Close();
        }

        // ================= MENU HỆ THỐNG =================
        private bool Authenticate(string title, string message, bool allowCancel)
        {
            using var dlg = new frmAuthDialog(title, message, DemoPassword, allowCancel);
            return dlg.ShowDialog(this) == DialogResult.OK;
        }

        private void mnuKhoaManHinh_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "Màn hình đang khóa";
            Authenticate("Màn hình đã khóa",
                "Nhập mật khẩu giao dịch viên để mở khóa (demo: 123456).", false);
            lblMsg.Text = "Sẵn sàng";
        }

        private void mnuDangXuat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất? Tất cả cửa sổ sẽ được đóng.",
                    "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            CloseAllChildren();
            lblTeller.Text = "Giao dịch viên trực: (chưa đăng nhập)";
            menuStrip.Enabled = false;

            bool ok = Authenticate("Đăng nhập lại",
                "Nhập mật khẩu để đăng nhập lại (demo: 123456) hoặc Hủy để thoát.", true);

            if (ok)
            {
                menuStrip.Enabled = true;
                lblTeller.Text = "Giao dịch viên trực: " + TellerName;
            }
            else
            {
                _forceClose = true;
                Close();
            }
        }

        private void mnuThoat_Click(object sender, EventArgs e) => Close();

        private void frmBankAtmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_forceClose) return;
            if (MessageBox.Show("Bạn có chắc muốn thoát hệ thống?", "Thoát",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                e.Cancel = true;
        }

        // ================= API CHO FORM CON (thanh tiến trình trên StatusStrip) =================
        public void StartProgress(string message)
        {
            progressBar.Value = 0;
            progressBar.Visible = true;
            lblMsg.Text = message;
        }

        public void UpdateProgress(int percent, string message = null)
        {
            progressBar.Value = Math.Max(0, Math.Min(100, percent));
            if (!string.IsNullOrEmpty(message)) lblMsg.Text = message;
        }

        public void EndProgress(string message)
        {
            progressBar.Visible = false;
            progressBar.Value = 0;
            lblMsg.Text = message;
        }
    }
}
