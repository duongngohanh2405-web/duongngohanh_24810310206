namespace Bai5_4
{
    partial class frmQuanLyNhanVien
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
            pnlTop = new System.Windows.Forms.Panel();
            lblView = new System.Windows.Forms.Label();
            cboView = new System.Windows.Forms.ComboBox();
            lblInfo = new System.Windows.Forms.Label();
            spcMain = new System.Windows.Forms.SplitContainer();
            tvDepartments = new System.Windows.Forms.TreeView();
            lsvEmployees = new System.Windows.Forms.ListView();
            colMaNV = new System.Windows.Forms.ColumnHeader();
            colHoTen = new System.Windows.Forms.ColumnHeader();
            colChucVu = new System.Windows.Forms.ColumnHeader();
            colNgayVaoLam = new System.Windows.Forms.ColumnHeader();
            imgSmall = new System.Windows.Forms.ImageList(components);
            imgLarge = new System.Windows.Forms.ImageList(components);
            pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)spcMain).BeginInit();
            spcMain.Panel1.SuspendLayout();
            spcMain.Panel2.SuspendLayout();
            spcMain.SuspendLayout();
            SuspendLayout();
            //
            // pnlTop
            //
            pnlTop.Controls.Add(lblView);
            pnlTop.Controls.Add(cboView);
            pnlTop.Controls.Add(lblInfo);
            pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTop.Location = new System.Drawing.Point(0, 0);
            pnlTop.Name = "pnlTop";
            pnlTop.Size = new System.Drawing.Size(900, 46);
            pnlTop.TabIndex = 0;
            //
            // lblView
            //
            lblView.AutoSize = true;
            lblView.Location = new System.Drawing.Point(12, 13);
            lblView.Name = "lblView";
            lblView.Text = "Chế độ xem:";
            //
            // cboView
            //
            cboView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboView.FormattingEnabled = true;
            cboView.Location = new System.Drawing.Point(110, 10);
            cboView.Name = "cboView";
            cboView.Size = new System.Drawing.Size(150, 28);
            cboView.TabIndex = 0;
            cboView.SelectedIndexChanged += cboView_SelectedIndexChanged;
            //
            // lblInfo
            //
            lblInfo.AutoSize = true;
            lblInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblInfo.Location = new System.Drawing.Point(290, 13);
            lblInfo.Name = "lblInfo";
            lblInfo.Text = "";
            //
            // spcMain
            //
            spcMain.Dock = System.Windows.Forms.DockStyle.Fill;
            spcMain.Location = new System.Drawing.Point(0, 46);
            spcMain.Name = "spcMain";
            spcMain.Panel1.Controls.Add(tvDepartments);
            spcMain.Panel2.Controls.Add(lsvEmployees);
            spcMain.Size = new System.Drawing.Size(900, 514);
            spcMain.SplitterDistance = 260;
            spcMain.TabIndex = 1;
            //
            // tvDepartments
            //
            tvDepartments.Dock = System.Windows.Forms.DockStyle.Fill;
            tvDepartments.HideSelection = false;
            tvDepartments.Location = new System.Drawing.Point(0, 0);
            tvDepartments.Name = "tvDepartments";
            tvDepartments.TabIndex = 2;
            tvDepartments.AfterSelect += tvDepartments_AfterSelect;
            //
            // lsvEmployees
            //
            lsvEmployees.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                colMaNV, colHoTen, colChucVu, colNgayVaoLam });
            lsvEmployees.Dock = System.Windows.Forms.DockStyle.Fill;
            lsvEmployees.FullRowSelect = true;
            lsvEmployees.GridLines = true;
            lsvEmployees.HideSelection = false;
            lsvEmployees.Location = new System.Drawing.Point(0, 0);
            lsvEmployees.MultiSelect = false;
            lsvEmployees.Name = "lsvEmployees";
            lsvEmployees.TabIndex = 3;
            lsvEmployees.UseCompatibleStateImageBehavior = false;
            lsvEmployees.View = System.Windows.Forms.View.Details;
            //
            // colMaNV
            //
            colMaNV.Text = "Mã NV";
            colMaNV.Width = 90;
            //
            // colHoTen
            //
            colHoTen.Text = "Họ Tên";
            colHoTen.Width = 190;
            //
            // colChucVu
            //
            colChucVu.Text = "Chức vụ";
            colChucVu.Width = 180;
            //
            // colNgayVaoLam
            //
            colNgayVaoLam.Text = "Ngày vào làm";
            colNgayVaoLam.Width = 120;
            //
            // frmQuanLyNhanVien
            //
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 560);
            Controls.Add(spcMain);
            Controls.Add(pnlTop);
            Name = "frmQuanLyNhanVien";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Quản lý nhân viên theo phòng ban";
            Load += frmQuanLyNhanVien_Load;
            pnlTop.ResumeLayout(false);
            pnlTop.PerformLayout();
            spcMain.Panel1.ResumeLayout(false);
            spcMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)spcMain).EndInit();
            spcMain.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblView;
        private System.Windows.Forms.ComboBox cboView;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.SplitContainer spcMain;
        private System.Windows.Forms.TreeView tvDepartments;
        private System.Windows.Forms.ListView lsvEmployees;
        private System.Windows.Forms.ColumnHeader colMaNV;
        private System.Windows.Forms.ColumnHeader colHoTen;
        private System.Windows.Forms.ColumnHeader colChucVu;
        private System.Windows.Forms.ColumnHeader colNgayVaoLam;
        private System.Windows.Forms.ImageList imgSmall;
        private System.Windows.Forms.ImageList imgLarge;
    }
}
