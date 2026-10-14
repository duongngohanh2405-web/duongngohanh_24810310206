namespace Bai1_ServiceCharge
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
            lblDonGia = new Label();
            lblSoLuong = new Label();
            lblGiamGia = new Label();
            lblTongTien = new Label();
            txtDonGia = new TextBox();
            txtSoLuong = new TextBox();
            txtGiamGia = new TextBox();
            btnTinhTien = new Button();
            btnLamMoi = new Button();
            SuspendLayout();
            // 
            // lblDonGia
            // 
            lblDonGia.AutoSize = true;
            lblDonGia.Location = new Point(122, 73);
            lblDonGia.Name = "lblDonGia";
            lblDonGia.Size = new Size(158, 20);
            lblDonGia.TabIndex = 0;
            lblDonGia.Text = "Đơn giá dịch vụ (VNĐ)";
            // 
            // lblSoLuong
            // 
            lblSoLuong.AutoSize = true;
            lblSoLuong.Location = new Point(122, 154);
            lblSoLuong.Name = "lblSoLuong";
            lblSoLuong.Size = new Size(111, 20);
            lblSoLuong.TabIndex = 1;
            lblSoLuong.Text = "Số lượng khách";
            // 
            // lblGiamGia
            // 
            lblGiamGia.AutoSize = true;
            lblGiamGia.Location = new Point(122, 245);
            lblGiamGia.Name = "lblGiamGia";
            lblGiamGia.Size = new Size(119, 20);
            lblGiamGia.TabIndex = 2;
            lblGiamGia.Text = "Mã giảm giá (%)";
            // 
            // lblTongTien
            // 
            lblTongTien.AutoSize = true;
            lblTongTien.Location = new Point(122, 354);
            lblTongTien.Name = "lblTongTien";
            lblTongTien.Size = new Size(122, 20);
            lblTongTien.TabIndex = 3;
            lblTongTien.Text = "Tổng tiền: 0 VNĐ";
            // 
            // txtDonGia
            // 
            txtDonGia.Location = new Point(122, 114);
            txtDonGia.Name = "txtDonGia";
            txtDonGia.Size = new Size(189, 27);
            txtDonGia.TabIndex = 0;
            // 
            // txtSoLuong
            // 
            txtSoLuong.Location = new Point(122, 198);
            txtSoLuong.Name = "txtSoLuong";
            txtSoLuong.Size = new Size(189, 27);
            txtSoLuong.TabIndex = 1;
            // 
            // txtGiamGia
            // 
            txtGiamGia.Location = new Point(122, 305);
            txtGiamGia.Name = "txtGiamGia";
            txtGiamGia.Size = new Size(189, 27);
            txtGiamGia.TabIndex = 2;
            // 
            // btnTinhTien
            // 
            btnTinhTien.Location = new Point(65, 398);
            btnTinhTien.Name = "btnTinhTien";
            btnTinhTien.Size = new Size(113, 29);
            btnTinhTien.TabIndex = 3;
            btnTinhTien.Text = "Tính tiền";
            btnTinhTien.UseVisualStyleBackColor = true;
            btnTinhTien.Click += btnTinhTien_Click;
            // 
            // btnLamMoi
            // 
            btnLamMoi.Location = new Point(233, 398);
            btnLamMoi.Name = "btnLamMoi";
            btnLamMoi.Size = new Size(115, 29);
            btnLamMoi.TabIndex = 4;
            btnLamMoi.Text = "Làm mới";
            btnLamMoi.UseVisualStyleBackColor = true;
            btnLamMoi.Click += btnLamMoi_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(804, 450);
            Controls.Add(btnLamMoi);
            Controls.Add(btnTinhTien);
            Controls.Add(txtGiamGia);
            Controls.Add(txtSoLuong);
            Controls.Add(txtDonGia);
            Controls.Add(lblTongTien);
            Controls.Add(lblGiamGia);
            Controls.Add(lblSoLuong);
            Controls.Add(lblDonGia);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDonGia;
        private Label lblSoLuong;
        private Label lblGiamGia;
        private Label lblTongTien;
        private TextBox txtDonGia;
        private TextBox txtSoLuong;
        private TextBox txtGiamGia;
        private Button btnTinhTien;
        private Button btnLamMoi;
    }
}
