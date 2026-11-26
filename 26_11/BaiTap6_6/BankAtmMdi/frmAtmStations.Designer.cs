namespace BankAtmMdi
{
    partial class frmAtmStations
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dgvStations = new DataGridView();
            panelRight = new Panel();
            lblMa = new Label();
            txtMa = new TextBox();
            lblDiaChi = new Label();
            txtDiaChi = new TextBox();
            lblTon = new Label();
            numTonQuy = new NumericUpDown();
            lblTrangThai = new Label();
            cboTrangThai = new ComboBox();
            lblAnh = new Label();
            picAnh = new PictureBox();
            btnChonAnh = new Button();
            btnThem = new Button();
            btnSua = new Button();
            btnXoa = new Button();
            btnLamMoi = new Button();

            ((System.ComponentModel.ISupportInitialize)dgvStations).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numTonQuy).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAnh).BeginInit();
            panelRight.SuspendLayout();
            SuspendLayout();

            // dgvStations
            dgvStations.Dock = DockStyle.Fill;
            dgvStations.Name = "dgvStations";
            dgvStations.ReadOnly = true;
            dgvStations.AllowUserToAddRows = false;
            dgvStations.AllowUserToDeleteRows = false;
            dgvStations.MultiSelect = false;
            dgvStations.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStations.RowHeadersVisible = false;
            dgvStations.BackgroundColor = SystemColors.Window;
            dgvStations.SelectionChanged += dgvStations_SelectionChanged;
            dgvStations.CellFormatting += dgvStations_CellFormatting;

            // panelRight
            panelRight.Dock = DockStyle.Right;
            panelRight.Width = 340;
            panelRight.Name = "panelRight";
            panelRight.Controls.AddRange(new Control[] {
                lblMa, txtMa, lblDiaChi, txtDiaChi, lblTon, numTonQuy, lblTrangThai, cboTrangThai,
                lblAnh, picAnh, btnChonAnh, btnThem, btnSua, btnXoa, btnLamMoi });

            lblMa.Text = "Mã trạm:";
            lblMa.SetBounds(15, 15, 300, 20);
            txtMa.SetBounds(15, 37, 300, 27);

            lblDiaChi.Text = "Địa chỉ đặt máy:";
            lblDiaChi.SetBounds(15, 72, 300, 20);
            txtDiaChi.SetBounds(15, 94, 300, 27);

            lblTon.Text = "Số tiền tồn quỹ hiện tại (VNĐ):";
            lblTon.SetBounds(15, 129, 300, 20);
            numTonQuy.SetBounds(15, 151, 300, 27);
            numTonQuy.Maximum = 100000000000M;
            numTonQuy.Increment = 1000000M;
            numTonQuy.ThousandsSeparator = true;

            lblTrangThai.Text = "Trạng thái hoạt động:";
            lblTrangThai.SetBounds(15, 186, 300, 20);
            cboTrangThai.SetBounds(15, 208, 300, 28);
            cboTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;

            lblAnh.Text = "Ảnh chụp hiện trường:";
            lblAnh.SetBounds(15, 243, 300, 20);
            picAnh.SetBounds(15, 265, 300, 140);
            picAnh.BorderStyle = BorderStyle.FixedSingle;
            picAnh.SizeMode = PictureBoxSizeMode.Zoom;
            btnChonAnh.Text = "Chọn ảnh...";
            btnChonAnh.SetBounds(15, 411, 300, 30);
            btnChonAnh.Click += btnChonAnh_Click;

            btnThem.Text = "Thêm";
            btnThem.SetBounds(15, 455, 95, 34);
            btnThem.Click += btnThem_Click;
            btnSua.Text = "Sửa";
            btnSua.SetBounds(118, 455, 95, 34);
            btnSua.Click += btnSua_Click;
            btnXoa.Text = "Xóa";
            btnXoa.SetBounds(221, 455, 94, 34);
            btnXoa.Click += btnXoa_Click;
            btnLamMoi.Text = "Làm mới / Nhập mới";
            btnLamMoi.SetBounds(15, 497, 300, 34);
            btnLamMoi.Click += btnLamMoi_Click;

            // frmAtmStations
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 560);
            Controls.Add(dgvStations);
            Controls.Add(panelRight);
            Name = "frmAtmStations";
            Text = "Danh sách Trạm ATM & Tồn quỹ";
            Load += frmAtmStations_Load;
            FormClosed += frmAtmStations_FormClosed;

            ((System.ComponentModel.ISupportInitialize)dgvStations).EndInit();
            ((System.ComponentModel.ISupportInitialize)numTonQuy).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAnh).EndInit();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            ResumeLayout(false);
        }

        private DataGridView dgvStations;
        private Panel panelRight;
        private Label lblMa, lblDiaChi, lblTon, lblTrangThai, lblAnh;
        private TextBox txtMa, txtDiaChi;
        private NumericUpDown numTonQuy;
        private ComboBox cboTrangThai;
        private PictureBox picAnh;
        private Button btnChonAnh, btnThem, btnSua, btnXoa, btnLamMoi;
    }
}
