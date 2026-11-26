namespace BankAtmMdi
{
    partial class frmAtmReconciliation
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTop = new Panel();
            btnRun = new Button();
            btnStop = new Button();
            lblState = new Label();
            progressLocal = new ProgressBar();
            gbResult = new GroupBox();
            lblTotalCap = new Label(); lblTotalVal = new Label();
            lblMatchedCap = new Label(); lblMatchedVal = new Label();
            lblMismatchCap = new Label(); lblMismatchVal = new Label();
            lblPendingCap = new Label(); lblPendingVal = new Label();
            lblAmountCap = new Label(); lblAmountVal = new Label();
            lblTimeCap = new Label(); lblTimeVal = new Label();
            txtLog = new RichTextBox();

            panelTop.SuspendLayout();
            gbResult.SuspendLayout();
            SuspendLayout();

            // panelTop
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 70;
            panelTop.Controls.AddRange(new Control[] { btnRun, btnStop, lblState });

            btnRun.Text = "Chạy Đối Soát Tự Động";
            btnRun.SetBounds(15, 14, 220, 42);
            btnRun.Click += btnRun_Click;

            btnStop.Text = "Dừng Đối Soát";
            btnStop.SetBounds(245, 14, 160, 42);
            btnStop.Enabled = false;
            btnStop.Click += btnStop_Click;

            lblState.Text = "Chưa chạy đối soát.";
            lblState.AutoSize = true;
            lblState.Location = new Point(425, 27);

            // progressLocal
            progressLocal.Dock = DockStyle.Top;
            progressLocal.Height = 22;

            // gbResult
            gbResult.Dock = DockStyle.Top;
            gbResult.Height = 130;
            gbResult.Text = "Kết quả đối soát";
            gbResult.Controls.AddRange(new Control[] {
                lblTotalCap, lblTotalVal, lblMatchedCap, lblMatchedVal, lblMismatchCap, lblMismatchVal,
                lblPendingCap, lblPendingVal, lblAmountCap, lblAmountVal, lblTimeCap, lblTimeVal });

            lblTotalCap.Text = "Tổng giao dịch đã tải:"; lblTotalCap.SetBounds(15, 28, 180, 22);
            lblTotalVal.Text = "0"; lblTotalVal.SetBounds(200, 28, 180, 22);
            lblMatchedCap.Text = "Giao dịch khớp:"; lblMatchedCap.SetBounds(15, 58, 180, 22);
            lblMatchedVal.Text = "0"; lblMatchedVal.SetBounds(200, 58, 180, 22);
            lblMismatchCap.Text = "Giao dịch lệch:"; lblMismatchCap.SetBounds(15, 88, 180, 22);
            lblMismatchVal.Text = "0"; lblMismatchVal.SetBounds(200, 88, 180, 22);

            lblPendingCap.Text = "Chờ xử lý:"; lblPendingCap.SetBounds(430, 28, 150, 22);
            lblPendingVal.Text = "0"; lblPendingVal.SetBounds(590, 28, 250, 22);
            lblAmountCap.Text = "Tổng tiền rút (VNĐ):"; lblAmountCap.SetBounds(430, 58, 150, 22);
            lblAmountVal.Text = "0"; lblAmountVal.SetBounds(590, 58, 250, 22);
            lblTimeCap.Text = "Thời gian xử lý:"; lblTimeCap.SetBounds(430, 88, 150, 22);
            lblTimeVal.Text = "-"; lblTimeVal.SetBounds(590, 88, 250, 22);

            // txtLog
            txtLog.Dock = DockStyle.Fill;
            txtLog.ReadOnly = true;
            txtLog.BackColor = Color.White;
            txtLog.Font = new Font("Consolas", 9.5F);

            // frmAtmReconciliation (Fill thêm trước, Top thêm sau để dock đúng thứ tự)
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 560);
            Controls.Add(txtLog);
            Controls.Add(gbResult);
            Controls.Add(progressLocal);
            Controls.Add(panelTop);
            Name = "frmAtmReconciliation";
            Text = "Đối soát Giao dịch Cuối ngày";
            FormClosing += frmAtmReconciliation_FormClosing;

            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            gbResult.ResumeLayout(false);
            ResumeLayout(false);
        }

        private Panel panelTop;
        private Button btnRun, btnStop;
        private Label lblState;
        private ProgressBar progressLocal;
        private GroupBox gbResult;
        private Label lblTotalCap, lblTotalVal, lblMatchedCap, lblMatchedVal, lblMismatchCap, lblMismatchVal;
        private Label lblPendingCap, lblPendingVal, lblAmountCap, lblAmountVal, lblTimeCap, lblTimeVal;
        private RichTextBox txtLog;
    }
}
