namespace Bai5_2
{
    partial class frmDichVu
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
            lblLoai = new System.Windows.Forms.Label();
            cboCategory = new System.Windows.Forms.ComboBox();
            lblCoSan = new System.Windows.Forms.Label();
            lstAvailableServices = new System.Windows.Forms.ListBox();
            btnSelect = new System.Windows.Forms.Button();
            btnRemove = new System.Windows.Forms.Button();
            btnClearAll = new System.Windows.Forms.Button();
            lblDaChon = new System.Windows.Forms.Label();
            lstSelectedServices = new System.Windows.Forms.ListBox();
            grpTinhTien = new System.Windows.Forms.GroupBox();
            lblTongTien = new System.Windows.Forms.Label();
            txtTongTien = new System.Windows.Forms.TextBox();
            lblChietKhau = new System.Windows.Forms.Label();
            nudChietKhau = new System.Windows.Forms.NumericUpDown();
            lblThanhTien = new System.Windows.Forms.Label();
            txtThanhTien = new System.Windows.Forms.TextBox();
            grpTinhTien.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudChietKhau).BeginInit();
            SuspendLayout();
            //
            // lblLoai
            //
            lblLoai.AutoSize = true;
            lblLoai.Location = new System.Drawing.Point(12, 18);
            lblLoai.Name = "lblLoai";
            lblLoai.Text = "Loại dịch vụ:";
            //
            // cboCategory
            //
            cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCategory.FormattingEnabled = true;
            cboCategory.Location = new System.Drawing.Point(110, 15);
            cboCategory.Name = "cboCategory";
            cboCategory.Size = new System.Drawing.Size(220, 28);
            cboCategory.TabIndex = 0;
            cboCategory.SelectedIndexChanged += cboCategory_SelectedIndexChanged;
            //
            // lblCoSan
            //
            lblCoSan.AutoSize = true;
            lblCoSan.Location = new System.Drawing.Point(12, 60);
            lblCoSan.Name = "lblCoSan";
            lblCoSan.Text = "Dịch vụ hiện có";
            //
            // lstAvailableServices
            //
            lstAvailableServices.FormattingEnabled = true;
            lstAvailableServices.ItemHeight = 20;
            lstAvailableServices.Location = new System.Drawing.Point(12, 85);
            lstAvailableServices.Name = "lstAvailableServices";
            lstAvailableServices.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            lstAvailableServices.Size = new System.Drawing.Size(300, 204);
            lstAvailableServices.TabIndex = 1;
            lstAvailableServices.MouseDoubleClick += lstAvailableServices_MouseDoubleClick;
            //
            // btnSelect
            //
            btnSelect.Location = new System.Drawing.Point(325, 105);
            btnSelect.Name = "btnSelect";
            btnSelect.Size = new System.Drawing.Size(60, 36);
            btnSelect.TabIndex = 2;
            btnSelect.Text = ">";
            btnSelect.UseVisualStyleBackColor = true;
            btnSelect.Click += btnSelect_Click;
            //
            // btnRemove
            //
            btnRemove.Location = new System.Drawing.Point(325, 150);
            btnRemove.Name = "btnRemove";
            btnRemove.Size = new System.Drawing.Size(60, 36);
            btnRemove.TabIndex = 3;
            btnRemove.Text = "<";
            btnRemove.UseVisualStyleBackColor = true;
            btnRemove.Click += btnRemove_Click;
            //
            // btnClearAll
            //
            btnClearAll.Location = new System.Drawing.Point(325, 195);
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Size = new System.Drawing.Size(60, 36);
            btnClearAll.TabIndex = 4;
            btnClearAll.Text = "<<";
            btnClearAll.UseVisualStyleBackColor = true;
            btnClearAll.Click += btnClearAll_Click;
            //
            // lblDaChon
            //
            lblDaChon.AutoSize = true;
            lblDaChon.Location = new System.Drawing.Point(398, 60);
            lblDaChon.Name = "lblDaChon";
            lblDaChon.Text = "Dịch vụ đã chọn";
            //
            // lstSelectedServices
            //
            lstSelectedServices.FormattingEnabled = true;
            lstSelectedServices.ItemHeight = 20;
            lstSelectedServices.Location = new System.Drawing.Point(398, 85);
            lstSelectedServices.Name = "lstSelectedServices";
            lstSelectedServices.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            lstSelectedServices.Size = new System.Drawing.Size(300, 204);
            lstSelectedServices.TabIndex = 5;
            //
            // grpTinhTien
            //
            grpTinhTien.Controls.Add(lblTongTien);
            grpTinhTien.Controls.Add(txtTongTien);
            grpTinhTien.Controls.Add(lblChietKhau);
            grpTinhTien.Controls.Add(nudChietKhau);
            grpTinhTien.Controls.Add(lblThanhTien);
            grpTinhTien.Controls.Add(txtThanhTien);
            grpTinhTien.Location = new System.Drawing.Point(12, 305);
            grpTinhTien.Name = "grpTinhTien";
            grpTinhTien.Size = new System.Drawing.Size(686, 150);
            grpTinhTien.TabIndex = 6;
            grpTinhTien.TabStop = false;
            grpTinhTien.Text = "Tính tiền";
            //
            // lblTongTien
            //
            lblTongTien.AutoSize = true;
            lblTongTien.Location = new System.Drawing.Point(15, 33);
            lblTongTien.Name = "lblTongTien";
            lblTongTien.Text = "Tổng tiền chưa giảm:";
            //
            // txtTongTien
            //
            txtTongTien.Location = new System.Drawing.Point(210, 30);
            txtTongTien.Name = "txtTongTien";
            txtTongTien.ReadOnly = true;
            txtTongTien.Size = new System.Drawing.Size(200, 27);
            txtTongTien.TabIndex = 7;
            txtTongTien.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // lblChietKhau
            //
            lblChietKhau.AutoSize = true;
            lblChietKhau.Location = new System.Drawing.Point(15, 73);
            lblChietKhau.Name = "lblChietKhau";
            lblChietKhau.Text = "Tỷ lệ chiết khấu (%):";
            //
            // nudChietKhau
            //
            nudChietKhau.Location = new System.Drawing.Point(210, 70);
            nudChietKhau.Name = "nudChietKhau";
            nudChietKhau.Size = new System.Drawing.Size(100, 27);
            nudChietKhau.TabIndex = 8;
            nudChietKhau.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            nudChietKhau.ValueChanged += nudChietKhau_ValueChanged;
            //
            // lblThanhTien
            //
            lblThanhTien.AutoSize = true;
            lblThanhTien.Location = new System.Drawing.Point(15, 113);
            lblThanhTien.Name = "lblThanhTien";
            lblThanhTien.Text = "Thành tiền thanh toán:";
            //
            // txtThanhTien
            //
            txtThanhTien.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            txtThanhTien.Location = new System.Drawing.Point(210, 110);
            txtThanhTien.Name = "txtThanhTien";
            txtThanhTien.ReadOnly = true;
            txtThanhTien.Size = new System.Drawing.Size(200, 27);
            txtThanhTien.TabIndex = 9;
            txtThanhTien.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            //
            // frmDichVu
            //
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(712, 470);
            Controls.Add(lblLoai);
            Controls.Add(cboCategory);
            Controls.Add(lblCoSan);
            Controls.Add(lstAvailableServices);
            Controls.Add(btnSelect);
            Controls.Add(btnRemove);
            Controls.Add(btnClearAll);
            Controls.Add(lblDaChon);
            Controls.Add(lstSelectedServices);
            Controls.Add(grpTinhTien);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "frmDichVu";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Bảng tính tiền dịch vụ";
            Load += frmDichVu_Load;
            grpTinhTien.ResumeLayout(false);
            grpTinhTien.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudChietKhau).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Label lblLoai;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.Label lblCoSan;
        private System.Windows.Forms.ListBox lstAvailableServices;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Label lblDaChon;
        private System.Windows.Forms.ListBox lstSelectedServices;
        private System.Windows.Forms.GroupBox grpTinhTien;
        private System.Windows.Forms.Label lblTongTien;
        private System.Windows.Forms.TextBox txtTongTien;
        private System.Windows.Forms.Label lblChietKhau;
        private System.Windows.Forms.NumericUpDown nudChietKhau;
        private System.Windows.Forms.Label lblThanhTien;
        private System.Windows.Forms.TextBox txtThanhTien;
    }
}
