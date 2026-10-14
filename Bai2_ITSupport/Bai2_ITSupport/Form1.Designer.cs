namespace Bai2_ITSupport
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTieuDe = new Label();
            lblMaPhieu = new Label();
            lblNguoiYeuCau = new Label();
            txtMaPhieu = new TextBox();
            txtNguoiYeuCau = new MaskedTextBox();
            lblNgayGhiNhan = new Label();
            lblLoaiSuCo = new Label();
            dtpNgayGhiNhan = new DateTimePicker();
            grpUuTien = new GroupBox();
            rdoTrungBinh = new RadioButton();
            rdoKhanCap = new RadioButton();
            rdoThap = new RadioButton();
            cboLoaiSuCo = new ComboBox();
            lblThietBi = new Label();
            chkMayTinhBan = new CheckBox();
            comboBox1 = new ComboBox();
            chkLaptop = new CheckBox();
            chkDienThoai = new CheckBox();
            chkMayIn = new CheckBox();
            lblAnhLoi = new Label();
            picAnhLoi = new PictureBox();
            btnTaiAnh = new Button();
            btnGuiYeuCau = new Button();
            btnNhapLai = new Button();
            grpUuTien.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAnhLoi).BeginInit();
            SuspendLayout();
            // 
            // lblTieuDe
            // 
            lblTieuDe.AutoSize = true;
            lblTieuDe.Font = new Font("Segoe UI", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTieuDe.ForeColor = Color.Navy;
            lblTieuDe.Location = new Point(70, 23);
            lblTieuDe.Name = "lblTieuDe";
            lblTieuDe.Size = new Size(382, 38);
            lblTieuDe.TabIndex = 0;
            lblTieuDe.Text = "PHIẾU TIẾP NHẬN SỰ CỐ IT";
            lblTieuDe.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblMaPhieu
            // 
            lblMaPhieu.AutoSize = true;
            lblMaPhieu.Location = new Point(57, 90);
            lblMaPhieu.Name = "lblMaPhieu";
            lblMaPhieu.Size = new Size(74, 20);
            lblMaPhieu.TabIndex = 1;
            lblMaPhieu.Text = "Mã phiếu:";
            // 
            // lblNguoiYeuCau
            // 
            lblNguoiYeuCau.AutoSize = true;
            lblNguoiYeuCau.Location = new Point(57, 123);
            lblNguoiYeuCau.Name = "lblNguoiYeuCau";
            lblNguoiYeuCau.Size = new Size(108, 20);
            lblNguoiYeuCau.TabIndex = 2;
            lblNguoiYeuCau.Text = "Người yêu cầu:";
            // 
            // txtMaPhieu
            // 
            txtMaPhieu.Location = new Point(202, 87);
            txtMaPhieu.Name = "txtMaPhieu";
            txtMaPhieu.Size = new Size(250, 27);
            txtMaPhieu.TabIndex = 0;
            // 
            // txtNguoiYeuCau
            // 
            txtNguoiYeuCau.Location = new Point(202, 123);
            txtNguoiYeuCau.Name = "txtNguoiYeuCau";
            txtNguoiYeuCau.Size = new Size(250, 27);
            txtNguoiYeuCau.TabIndex = 1;
            // 
            // lblNgayGhiNhan
            // 
            lblNgayGhiNhan.AutoSize = true;
            lblNgayGhiNhan.Location = new Point(57, 163);
            lblNgayGhiNhan.Name = "lblNgayGhiNhan";
            lblNgayGhiNhan.Size = new Size(108, 20);
            lblNgayGhiNhan.TabIndex = 3;
            lblNgayGhiNhan.Text = "Ngày ghi nhận:";
            // 
            // lblLoaiSuCo
            // 
            lblLoaiSuCo.AutoSize = true;
            lblLoaiSuCo.Location = new Point(57, 317);
            lblLoaiSuCo.Name = "lblLoaiSuCo";
            lblLoaiSuCo.Size = new Size(79, 20);
            lblLoaiSuCo.TabIndex = 4;
            lblLoaiSuCo.Text = "Loại sự cố:";
            // 
            // dtpNgayGhiNhan
            // 
            dtpNgayGhiNhan.Format = DateTimePickerFormat.Short;
            dtpNgayGhiNhan.Location = new Point(202, 156);
            dtpNgayGhiNhan.Name = "dtpNgayGhiNhan";
            dtpNgayGhiNhan.Size = new Size(250, 27);
            dtpNgayGhiNhan.TabIndex = 2;
            // 
            // grpUuTien
            // 
            grpUuTien.Controls.Add(rdoTrungBinh);
            grpUuTien.Controls.Add(rdoKhanCap);
            grpUuTien.Controls.Add(rdoThap);
            grpUuTien.Location = new Point(70, 201);
            grpUuTien.Name = "grpUuTien";
            grpUuTien.Size = new Size(350, 90);
            grpUuTien.TabIndex = 5;
            grpUuTien.TabStop = false;
            grpUuTien.Text = "Mức độ ưu tiên";
            // 
            // rdoTrungBinh
            // 
            rdoTrungBinh.AutoSize = true;
            rdoTrungBinh.Location = new Point(104, 47);
            rdoTrungBinh.Name = "rdoTrungBinh";
            rdoTrungBinh.Size = new Size(100, 24);
            rdoTrungBinh.TabIndex = 6;
            rdoTrungBinh.TabStop = true;
            rdoTrungBinh.Text = "Trung bình";
            rdoTrungBinh.UseVisualStyleBackColor = true;
            rdoTrungBinh.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // rdoKhanCap
            // 
            rdoKhanCap.AutoSize = true;
            rdoKhanCap.Location = new Point(235, 47);
            rdoKhanCap.Name = "rdoKhanCap";
            rdoKhanCap.Size = new Size(91, 24);
            rdoKhanCap.TabIndex = 5;
            rdoKhanCap.TabStop = true;
            rdoKhanCap.Text = "Khẩn cấp";
            rdoKhanCap.UseVisualStyleBackColor = true;
            // 
            // rdoThap
            // 
            rdoThap.AutoSize = true;
            rdoThap.Location = new Point(13, 47);
            rdoThap.Name = "rdoThap";
            rdoThap.Size = new Size(63, 24);
            rdoThap.TabIndex = 4;
            rdoThap.TabStop = true;
            rdoThap.Text = "Thấp";
            rdoThap.UseVisualStyleBackColor = true;
            // 
            // cboLoaiSuCo
            // 
            cboLoaiSuCo.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLoaiSuCo.FormattingEnabled = true;
            cboLoaiSuCo.Location = new Point(202, 309);
            cboLoaiSuCo.Name = "cboLoaiSuCo";
            cboLoaiSuCo.Size = new Size(151, 28);
            cboLoaiSuCo.TabIndex = 3;
            // 
            // lblThietBi
            // 
            lblThietBi.AutoSize = true;
            lblThietBi.Location = new Point(57, 356);
            lblThietBi.Name = "lblThietBi";
            lblThietBi.Size = new Size(137, 20);
            lblThietBi.TabIndex = 6;
            lblThietBi.Text = "Thiết bị ảnh hưởng:";
            // 
            // chkMayTinhBan
            // 
            chkMayTinhBan.AutoSize = true;
            chkMayTinhBan.Location = new Point(57, 392);
            chkMayTinhBan.Name = "chkMayTinhBan";
            chkMayTinhBan.Size = new Size(117, 24);
            chkMayTinhBan.TabIndex = 4;
            chkMayTinhBan.Text = "Máy tính bàn";
            chkMayTinhBan.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "Phần cứng", "", "Phần mềm", "", "Mạng", "", "Tài khoản" });
            comboBox1.Location = new Point(202, 353);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(151, 28);
            comboBox1.TabIndex = 8;
            // 
            // chkLaptop
            // 
            chkLaptop.AutoSize = true;
            chkLaptop.Location = new Point(212, 392);
            chkLaptop.Name = "chkLaptop";
            chkLaptop.Size = new Size(78, 24);
            chkLaptop.TabIndex = 5;
            chkLaptop.Text = "Laptop";
            chkLaptop.UseVisualStyleBackColor = true;
            // 
            // chkDienThoai
            // 
            chkDienThoai.AutoSize = true;
            chkDienThoai.Location = new Point(464, 392);
            chkDienThoai.Name = "chkDienThoai";
            chkDienThoai.Size = new Size(100, 24);
            chkDienThoai.TabIndex = 7;
            chkDienThoai.Text = "Điện thoại";
            chkDienThoai.UseVisualStyleBackColor = true;
            // 
            // chkMayIn
            // 
            chkMayIn.AutoSize = true;
            chkMayIn.Location = new Point(345, 392);
            chkMayIn.Name = "chkMayIn";
            chkMayIn.Size = new Size(75, 24);
            chkMayIn.TabIndex = 6;
            chkMayIn.Text = "Máy in";
            chkMayIn.UseVisualStyleBackColor = true;
            // 
            // lblAnhLoi
            // 
            lblAnhLoi.AutoSize = true;
            lblAnhLoi.Location = new Point(57, 430);
            lblAnhLoi.Name = "lblAnhLoi";
            lblAnhLoi.Size = new Size(95, 20);
            lblAnhLoi.TabIndex = 9;
            lblAnhLoi.Text = "Ảnh chụp lỗi:";
            // 
            // picAnhLoi
            // 
            picAnhLoi.BorderStyle = BorderStyle.FixedSingle;
            picAnhLoi.Location = new Point(170, 430);
            picAnhLoi.Name = "picAnhLoi";
            picAnhLoi.Size = new Size(250, 150);
            picAnhLoi.SizeMode = PictureBoxSizeMode.StretchImage;
            picAnhLoi.TabIndex = 10;
            picAnhLoi.TabStop = false;
            // 
            // btnTaiAnh
            // 
            btnTaiAnh.Location = new Point(42, 470);
            btnTaiAnh.Name = "btnTaiAnh";
            btnTaiAnh.Size = new Size(110, 35);
            btnTaiAnh.TabIndex = 8;
            btnTaiAnh.Text = "Tải ảnh lỗi";
            btnTaiAnh.UseVisualStyleBackColor = true;
            btnTaiAnh.Click += btnTaiAnh_Click;
            // 
            // btnGuiYeuCau
            // 
            btnGuiYeuCau.Location = new Point(83, 601);
            btnGuiYeuCau.Name = "btnGuiYeuCau";
            btnGuiYeuCau.Size = new Size(120, 40);
            btnGuiYeuCau.TabIndex = 9;
            btnGuiYeuCau.Text = "Gửi yêu cầu";
            btnGuiYeuCau.UseVisualStyleBackColor = true;
            btnGuiYeuCau.Click += btnGuiYeuCau_Click;
            // 
            // btnNhapLai
            // 
            btnNhapLai.Location = new Point(413, 601);
            btnNhapLai.Name = "btnNhapLai";
            btnNhapLai.Size = new Size(120, 40);
            btnNhapLai.TabIndex = 10;
            btnNhapLai.Text = "Nhập lại";
            btnNhapLai.UseVisualStyleBackColor = true;
            btnNhapLai.Click += btnNhapLai_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(832, 653);
            Controls.Add(btnNhapLai);
            Controls.Add(btnGuiYeuCau);
            Controls.Add(btnTaiAnh);
            Controls.Add(picAnhLoi);
            Controls.Add(lblAnhLoi);
            Controls.Add(chkMayIn);
            Controls.Add(chkDienThoai);
            Controls.Add(chkLaptop);
            Controls.Add(comboBox1);
            Controls.Add(chkMayTinhBan);
            Controls.Add(lblThietBi);
            Controls.Add(cboLoaiSuCo);
            Controls.Add(grpUuTien);
            Controls.Add(lblLoaiSuCo);
            Controls.Add(lblNgayGhiNhan);
            Controls.Add(dtpNgayGhiNhan);
            Controls.Add(lblTieuDe);
            Controls.Add(txtMaPhieu);
            Controls.Add(txtNguoiYeuCau);
            Controls.Add(lblMaPhieu);
            Controls.Add(lblNguoiYeuCau);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tiếp nhận và phân loại sự cố IT";
            Load += Form1_Load;
            grpUuTien.ResumeLayout(false);
            grpUuTien.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picAnhLoi).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTieuDe;
        private Label lblMaPhieu;
        private Label lblNguoiYeuCau;
        private TextBox txtMaPhieu;
        private MaskedTextBox txtNguoiYeuCau;
        private Label lblNgayGhiNhan;
        private Label lblLoaiSuCo;
        private DateTimePicker dtpNgayGhiNhan;
        private GroupBox grpUuTien;
        private RadioButton rdoKhanCap;
        private RadioButton rdoThap;
        private RadioButton rdoTrungBinh;
        private ComboBox cboLoaiSuCo;
        private Label lblThietBi;
        private CheckBox chkMayTinhBan;
        private ComboBox comboBox1;
        private CheckBox chkLaptop;
        private CheckBox chkDienThoai;
        private CheckBox chkMayIn;
        private Label lblAnhLoi;
        private PictureBox picAnhLoi;
        private Button btnTaiAnh;
        private Button btnGuiYeuCau;
        private Button btnNhapLai;
    }
}
