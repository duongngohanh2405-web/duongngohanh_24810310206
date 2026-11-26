namespace BankAtmMdi
{
    partial class frmBankAtmMain
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip();
            mnuHeThong = new ToolStripMenuItem("Hệ thống");
            mnuDangXuat = new ToolStripMenuItem("Đăng xuất");
            mnuKhoaManHinh = new ToolStripMenuItem("Khóa màn hình");
            sepHeThong = new ToolStripSeparator();
            mnuThoat = new ToolStripMenuItem("Thoát");
            mnuQuanLyAtm = new ToolStripMenuItem("Quản lý ATM");
            mnuDsTram = new ToolStripMenuItem("Danh sách Trạm ATM && Tồn quỹ");
            mnuDoiSoat = new ToolStripMenuItem("Đối soát Giao dịch Cuối ngày");
            mnuCuaSo = new ToolStripMenuItem("Cửa sổ");
            mnuCascade = new ToolStripMenuItem("Xếp tầng (Cascade)");
            mnuTileH = new ToolStripMenuItem("Xếp ngang (Tile Horizontal)");
            mnuTileV = new ToolStripMenuItem("Xếp dọc (Tile Vertical)");
            sepCuaSo = new ToolStripSeparator();
            mnuDongTatCa = new ToolStripMenuItem("Đóng tất cả");

            statusStrip = new StatusStrip();
            lblTeller = new ToolStripStatusLabel();
            lblCore = new ToolStripStatusLabel();
            lblMsg = new ToolStripStatusLabel();
            progressBar = new ToolStripProgressBar();

            menuStrip.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // menuStrip
            menuStrip.Items.AddRange(new ToolStripItem[] { mnuHeThong, mnuQuanLyAtm, mnuCuaSo });
            menuStrip.MdiWindowListItem = mnuCuaSo;
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";

            mnuHeThong.DropDownItems.AddRange(new ToolStripItem[] { mnuDangXuat, mnuKhoaManHinh, sepHeThong, mnuThoat });
            mnuDangXuat.Click += mnuDangXuat_Click;
            mnuKhoaManHinh.ShortcutKeys = Keys.Control | Keys.L;
            mnuKhoaManHinh.Click += mnuKhoaManHinh_Click;
            mnuThoat.ShortcutKeys = Keys.Alt | Keys.F4;
            mnuThoat.Click += mnuThoat_Click;

            mnuQuanLyAtm.DropDownItems.AddRange(new ToolStripItem[] { mnuDsTram, mnuDoiSoat });
            mnuDsTram.ShortcutKeys = Keys.Control | Keys.D1;
            mnuDsTram.Click += mnuDsTram_Click;
            mnuDoiSoat.ShortcutKeys = Keys.Control | Keys.D2;
            mnuDoiSoat.Click += mnuDoiSoat_Click;

            mnuCuaSo.DropDownItems.AddRange(new ToolStripItem[] { mnuCascade, mnuTileH, mnuTileV, sepCuaSo, mnuDongTatCa });
            mnuCascade.Click += mnuCascade_Click;
            mnuTileH.Click += mnuTileH_Click;
            mnuTileV.Click += mnuTileV_Click;
            mnuDongTatCa.Click += mnuDongTatCa_Click;

            // statusStrip
            statusStrip.Items.AddRange(new ToolStripItem[] { lblTeller, lblCore, lblMsg, progressBar });
            lblTeller.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblTeller.Padding = new Padding(0, 0, 8, 0);
            lblCore.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblCore.Padding = new Padding(0, 0, 8, 0);
            lblMsg.Spring = true;
            lblMsg.TextAlign = ContentAlignment.MiddleLeft;
            progressBar.Size = new Size(220, 18);
            progressBar.Visible = false;

            // frmBankAtmMain
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 750);
            Controls.Add(menuStrip);
            Controls.Add(statusStrip);
            IsMdiContainer = true;
            MainMenuStrip = menuStrip;
            Name = "frmBankAtmMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hệ thống điều hành mạng lưới cây ATM - Ngân hàng";
            WindowState = FormWindowState.Maximized;
            FormClosing += frmBankAtmMain_FormClosing;
            Load += frmBankAtmMain_Load;

            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private MenuStrip menuStrip;
        private ToolStripMenuItem mnuHeThong, mnuDangXuat, mnuKhoaManHinh, mnuThoat;
        private ToolStripSeparator sepHeThong, sepCuaSo;
        private ToolStripMenuItem mnuQuanLyAtm, mnuDsTram, mnuDoiSoat;
        private ToolStripMenuItem mnuCuaSo, mnuCascade, mnuTileH, mnuTileV, mnuDongTatCa;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblTeller, lblCore, lblMsg;
        private ToolStripProgressBar progressBar;
    }
}
