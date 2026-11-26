namespace PowerGridMonitor
{
    partial class frmPowerGridMonitor
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            grpControl = new GroupBox();
            lblStation = new Label();
            cboStation = new ComboBox();
            btnStart = new Button();
            btnDisconnect = new Button();
            grpDashboard = new GroupBox();
            lblVoltageCap = new Label();
            lblVoltage = new Label();
            lblLoadCap = new Label();
            lblLoadPercent = new Label();
            lblWarning = new Label();
            grpLog = new GroupBox();
            dgvGridLogs = new DataGridView();
            statusStrip = new StatusStrip();
            lblState = new ToolStripStatusLabel();
            lblLastPacket = new ToolStripStatusLabel();
            lblCycles = new ToolStripStatusLabel();
            lblOverloads = new ToolStripStatusLabel();

            grpControl.SuspendLayout();
            grpDashboard.SuspendLayout();
            grpLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvGridLogs).BeginInit();
            statusStrip.SuspendLayout();
            SuspendLayout();

            // grpControl
            grpControl.Dock = DockStyle.Top;
            grpControl.Height = 80;
            grpControl.Text = "Khu vực điều khiển";
            grpControl.Controls.AddRange(new Control[] { lblStation, cboStation, btnStart, btnDisconnect });

            lblStation.Text = "Trạm biến áp:";
            lblStation.SetBounds(15, 36, 100, 22);
            cboStation.SetBounds(120, 32, 270, 28);
            cboStation.DropDownStyle = ComboBoxStyle.DropDownList;

            btnStart.Text = "Bắt đầu Giám sát SCADA";
            btnStart.SetBounds(410, 24, 220, 42);
            btnStart.Click += btnStart_Click;

            btnDisconnect.Text = "Ngắt Kết nối";
            btnDisconnect.SetBounds(640, 24, 170, 42);
            btnDisconnect.Enabled = false;
            btnDisconnect.Click += btnDisconnect_Click;

            // grpDashboard
            grpDashboard.Dock = DockStyle.Top;
            grpDashboard.Height = 190;
            grpDashboard.Text = "Bảng thông số vận hành (Real-time Dashboard)";
            grpDashboard.Controls.AddRange(new Control[] { lblVoltageCap, lblVoltage, lblLoadCap, lblLoadPercent, lblWarning });

            lblVoltageCap.Text = "ĐIỆN ÁP HIỆN TẠI";
            lblVoltageCap.SetBounds(20, 28, 400, 22);
            lblVoltageCap.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            lblVoltage.Name = "lblVoltage";
            lblVoltage.Text = "-- kV";
            lblVoltage.SetBounds(20, 55, 400, 75);
            lblVoltage.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblVoltage.ForeColor = Color.DimGray;
            lblVoltage.TextAlign = ContentAlignment.MiddleLeft;

            lblLoadCap.Text = "MỨC MANG TẢI";
            lblLoadCap.SetBounds(450, 28, 400, 22);
            lblLoadCap.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

            lblLoadPercent.Name = "lblLoadPercent";
            lblLoadPercent.Text = "-- %";
            lblLoadPercent.SetBounds(450, 55, 400, 75);
            lblLoadPercent.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            lblLoadPercent.ForeColor = Color.DimGray;
            lblLoadPercent.TextAlign = ContentAlignment.MiddleLeft;

            lblWarning.Text = "";
            lblWarning.SetBounds(20, 140, 830, 36);
            lblWarning.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblWarning.ForeColor = Color.Red;
            lblWarning.TextAlign = ContentAlignment.MiddleLeft;
            lblWarning.Visible = false;

            // grpLog
            grpLog.Dock = DockStyle.Fill;
            grpLog.Text = "Nhật ký sự cố (các mốc Quá tải)";
            grpLog.Controls.Add(dgvGridLogs);

            dgvGridLogs.Name = "dgvGridLogs";
            dgvGridLogs.Dock = DockStyle.Fill;

            // statusStrip
            statusStrip.Items.AddRange(new ToolStripItem[] { lblState, lblLastPacket, lblCycles, lblOverloads });
            lblState.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblState.Padding = new Padding(0, 0, 8, 0);
            lblLastPacket.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblLastPacket.Padding = new Padding(0, 0, 8, 0);
            lblCycles.BorderSides = ToolStripStatusLabelBorderSides.Right;
            lblCycles.Padding = new Padding(0, 0, 8, 0);

            // frmPowerGridMonitor (Fill thêm trước, Top/Bottom thêm sau để dock đúng thứ tự)
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(880, 640);
            MinimumSize = new Size(760, 520);
            Controls.Add(grpLog);
            Controls.Add(grpDashboard);
            Controls.Add(grpControl);
            Controls.Add(statusStrip);
            Name = "frmPowerGridMonitor";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Trung tâm điều độ & giám sát lưới điện thời gian thực (SCADA)";
            Load += frmPowerGridMonitor_Load;
            FormClosing += frmPowerGridMonitor_FormClosing;

            grpControl.ResumeLayout(false);
            grpDashboard.ResumeLayout(false);
            grpLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvGridLogs).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private GroupBox grpControl, grpDashboard, grpLog;
        private Label lblStation, lblVoltageCap, lblVoltage, lblLoadCap, lblLoadPercent, lblWarning;
        private ComboBox cboStation;
        private Button btnStart, btnDisconnect;
        private DataGridView dgvGridLogs;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblState, lblLastPacket, lblCycles, lblOverloads;
    }
}
