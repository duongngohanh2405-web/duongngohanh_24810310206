namespace Bai5_1
{
    partial class frmDangKy
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpCaNhan = new System.Windows.Forms.GroupBox();
            lblTenDN = new System.Windows.Forms.Label();
            txtTenDN = new System.Windows.Forms.TextBox();
            lblMatKhau = new System.Windows.Forms.Label();
            txtMatKhau = new System.Windows.Forms.TextBox();
            lblXacNhan = new System.Windows.Forms.Label();
            txtXacNhan = new System.Windows.Forms.TextBox();
            grpBoSung = new System.Windows.Forms.GroupBox();
            lblNgaySinh = new System.Windows.Forms.Label();
            dtpNgaySinh = new System.Windows.Forms.DateTimePicker();
            lblGioiTinh = new System.Windows.Forms.Label();
            rdoNam = new System.Windows.Forms.RadioButton();
            rdoNu = new System.Windows.Forms.RadioButton();
            chkDieuKhoan = new System.Windows.Forms.CheckBox();
            btnDangKy = new System.Windows.Forms.Button();
            btnLamMoi = new System.Windows.Forms.Button();
            epCheck = new System.Windows.Forms.ErrorProvider(components);
            grpCaNhan.SuspendLayout();
            grpBoSung.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)epCheck).BeginInit();
            SuspendLayout();
            //
            // grpCaNhan
            //
            grpCaNhan.Controls.Add(lblTenDN);
            grpCaNhan.Controls.Add(txtTenDN);
            grpCaNhan.Controls.Add(lblMatKhau);
            grpCaNhan.Controls.Add(txtMatKhau);
            grpCaNhan.Controls.Add(lblXacNhan);
            grpCaNhan.Controls.Add(txtXacNhan);
            grpCaNhan.Location = new System.Drawing.Point(12, 12);
            grpCaNhan.Name = "grpCaNhan";
            grpCaNhan.Size = new System.Drawing.Size(400, 150);
            grpCaNhan.TabIndex = 0;
            grpCaNhan.TabStop = false;
            grpCaNhan.Text = "Thông tin cá nhân";
            //
            // lblTenDN
            //
            lblTenDN.AutoSize = true;
            lblTenDN.Location = new System.Drawing.Point(15, 33);
            lblTenDN.Name = "lblTenDN";
            lblTenDN.Text = "Tên đăng nhập";
            //
            // txtTenDN
            //
            txtTenDN.Location = new System.Drawing.Point(150, 30);
            txtTenDN.Name = "txtTenDN";
            txtTenDN.Size = new System.Drawing.Size(200, 27);
            txtTenDN.TabIndex = 1;
            //
            // lblMatKhau
            //
            lblMatKhau.AutoSize = true;
            lblMatKhau.Location = new System.Drawing.Point(15, 73);
            lblMatKhau.Name = "lblMatKhau";
            lblMatKhau.Text = "Mật khẩu";
            //
            // txtMatKhau
            //
            txtMatKhau.Location = new System.Drawing.Point(150, 70);
            txtMatKhau.Name = "txtMatKhau";
            txtMatKhau.Size = new System.Drawing.Size(200, 27);
            txtMatKhau.TabIndex = 2;
            txtMatKhau.UseSystemPasswordChar = true;
            //
            // lblXacNhan
            //
            lblXacNhan.AutoSize = true;
            lblXacNhan.Location = new System.Drawing.Point(15, 113);
            lblXacNhan.Name = "lblXacNhan";
            lblXacNhan.Text = "Xác nhận mật khẩu";
            //
            // txtXacNhan
            //
            txtXacNhan.Location = new System.Drawing.Point(150, 110);
            txtXacNhan.Name = "txtXacNhan";
            txtXacNhan.Size = new System.Drawing.Size(200, 27);
            txtXacNhan.TabIndex = 3;
            txtXacNhan.UseSystemPasswordChar = true;
            //
            // grpBoSung
            //
            grpBoSung.Controls.Add(lblNgaySinh);
            grpBoSung.Controls.Add(dtpNgaySinh);
            grpBoSung.Controls.Add(lblGioiTinh);
            grpBoSung.Controls.Add(rdoNam);
            grpBoSung.Controls.Add(rdoNu);
            grpBoSung.Controls.Add(chkDieuKhoan);
            grpBoSung.Location = new System.Drawing.Point(12, 172);
            grpBoSung.Name = "grpBoSung";
            grpBoSung.Size = new System.Drawing.Size(400, 150);
            grpBoSung.TabIndex = 1;
            grpBoSung.TabStop = false;
            grpBoSung.Text = "Thông tin bổ sung";
            //
            // lblNgaySinh
            //
            lblNgaySinh.AutoSize = true;
            lblNgaySinh.Location = new System.Drawing.Point(15, 33);
            lblNgaySinh.Name = "lblNgaySinh";
            lblNgaySinh.Text = "Ngày sinh";
            //
            // dtpNgaySinh
            //
            dtpNgaySinh.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dtpNgaySinh.Location = new System.Drawing.Point(150, 30);
            dtpNgaySinh.Name = "dtpNgaySinh";
            dtpNgaySinh.Size = new System.Drawing.Size(150, 27);
            dtpNgaySinh.TabIndex = 4;
            //
            // lblGioiTinh
            //
            lblGioiTinh.AutoSize = true;
            lblGioiTinh.Location = new System.Drawing.Point(15, 73);
            lblGioiTinh.Name = "lblGioiTinh";
            lblGioiTinh.Text = "Giới tính";
            //
            // rdoNam
            //
            rdoNam.AutoSize = true;
            rdoNam.Checked = true;
            rdoNam.Location = new System.Drawing.Point(150, 70);
            rdoNam.Name = "rdoNam";
            rdoNam.TabIndex = 5;
            rdoNam.TabStop = true;
            rdoNam.Text = "Nam";
            //
            // rdoNu
            //
            rdoNu.AutoSize = true;
            rdoNu.Location = new System.Drawing.Point(230, 70);
            rdoNu.Name = "rdoNu";
            rdoNu.TabIndex = 6;
            rdoNu.Text = "Nữ";
            //
            // chkDieuKhoan
            //
            chkDieuKhoan.AutoSize = true;
            chkDieuKhoan.Location = new System.Drawing.Point(15, 110);
            chkDieuKhoan.Name = "chkDieuKhoan";
            chkDieuKhoan.TabIndex = 7;
            chkDieuKhoan.Text = "Tôi đồng ý với điều khoản dịch vụ";
            //
            // btnDangKy
            //
            btnDangKy.Location = new System.Drawing.Point(100, 340);
            btnDangKy.Name = "btnDangKy";
            btnDangKy.Size = new System.Drawing.Size(110, 36);
            btnDangKy.TabIndex = 8;
            btnDangKy.Text = "Đăng Ký";
            btnDangKy.UseVisualStyleBackColor = true;
            btnDangKy.Click += btnDangKy_Click;
            //
            // btnLamMoi
            //
            btnLamMoi.Location = new System.Drawing.Point(230, 340);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new System.Drawing.Size(110, 36);
            btnLamMoi.TabIndex = 9;
            btnLamMoi.Text = "Làm Mới";
            btnLamMoi.UseVisualStyleBackColor = true;
            btnLamMoi.Click += btnLamMoi_Click;
            //
            // epCheck
            //
            epCheck.ContainerControl = this;
            //
            // frmDangKy
            //
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(426, 395);
            Controls.Add(grpCaNhan);
            Controls.Add(grpBoSung);
            Controls.Add(btnDangKy);
            Controls.Add(btnLamMoi);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "frmDangKy";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Đăng ký tài khoản";
            Load += frmDangKy_Load;
            grpCaNhan.ResumeLayout(false);
            grpCaNhan.PerformLayout();
            grpBoSung.ResumeLayout(false);
            grpBoSung.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)epCheck).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpCaNhan;
        private System.Windows.Forms.Label lblTenDN;
        private System.Windows.Forms.TextBox txtTenDN;
        private System.Windows.Forms.Label lblMatKhau;
        private System.Windows.Forms.TextBox txtMatKhau;
        private System.Windows.Forms.Label lblXacNhan;
        private System.Windows.Forms.TextBox txtXacNhan;
        private System.Windows.Forms.GroupBox grpBoSung;
        private System.Windows.Forms.Label lblNgaySinh;
        private System.Windows.Forms.DateTimePicker dtpNgaySinh;
        private System.Windows.Forms.Label lblGioiTinh;
        private System.Windows.Forms.RadioButton rdoNam;
        private System.Windows.Forms.RadioButton rdoNu;
        private System.Windows.Forms.CheckBox chkDieuKhoan;
        private System.Windows.Forms.Button btnDangKy;
        private System.Windows.Forms.Button btnLamMoi;
        private System.Windows.Forms.ErrorProvider epCheck;
    }
}
