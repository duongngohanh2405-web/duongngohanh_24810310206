using System.Diagnostics;
using BankAtmMdi.Models;

namespace BankAtmMdi
{
    public partial class frmAtmReconciliation : Form
    {
        private CancellationTokenSource _cts;

        private static readonly string[] Phases =
        {
            "Tải sao kê giao dịch từ máy chủ Core Banking",
            "Đối chiếu giao dịch rút tiền ATM",
            "Đối chiếu giao dịch chuyển khoản liên ngân hàng",
            "Kiểm tra chênh lệch tồn quỹ theo từng trạm",
            "Tổng hợp kết quả đối soát"
        };

        public frmAtmReconciliation()
        {
            InitializeComponent();
        }

        private frmBankAtmMain ParentMain => MdiParent as frmBankAtmMain;

        // ================= CHẠY ĐỐI SOÁT (async/await) =================
        private async void btnRun_Click(object sender, EventArgs e)
        {
            if (_cts != null) return; // đang chạy

            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;

            ResetResult();
            SetRunning(true);
            ParentMain?.StartProgress("Đang đối soát giao dịch ATM...");
            AppendLog("=== BẮT ĐẦU ĐỐI SOÁT ===");

            // Progress<T> được tạo trên UI Thread nên callback tự động chạy trên UI Thread
            var progress = new Progress<ReconProgress>(OnProgress);

            try
            {
                // Công việc nặng chạy trên ThreadPool, UI Thread vẫn rảnh để xử lý sự kiện
                ReconResult result = await Task.Run(() => ReconcileAsync(progress, token), token);

                if (IsDisposed) return;
                ShowResult(result);
                AppendLog("=== HOÀN TẤT ĐỐI SOÁT ===");
                lblState.Text = "Đối soát hoàn tất.";
                ParentMain?.EndProgress("Đối soát hoàn tất.");
            }
            catch (OperationCanceledException)
            {
                if (IsDisposed) { ParentMain?.EndProgress("Đã hủy đối soát."); return; }
                AppendLog("=== ĐÃ DỪNG ĐỐI SOÁT THEO YÊU CẦU ===");
                lblState.Text = "Đã dừng đối soát.";
                progressLocal.Value = 0;
                ParentMain?.EndProgress("Đã dừng đối soát.");
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    AppendLog("LỖI: " + ex.Message);
                    lblState.Text = "Đối soát thất bại.";
                }
                ParentMain?.EndProgress("Đối soát thất bại.");
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                if (!IsDisposed) SetRunning(false);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_cts == null) return;
            btnStop.Enabled = false;
            lblState.Text = "Đang dừng...";
            _cts.Cancel();
        }

        /// <summary>
        /// Giả lập tải + đối chiếu hàng trăm nghìn giao dịch trong 4-6 giây.
        /// Chạy trên ThreadPool, KHÔNG được đụng vào control UI trong hàm này.
        /// </summary>
        private static async Task<ReconResult> ReconcileAsync(IProgress<ReconProgress> progress, CancellationToken token)
        {
            var rnd = new Random();
            var sw = Stopwatch.StartNew();

            int totalMs = rnd.Next(4000, 6001);           // 4 - 6 giây
            const int steps = 40;
            int delayPerStep = totalMs / steps;
            int totalTx = rnd.Next(250_000, 450_001);     // hàng trăm nghìn giao dịch
            int perStep = totalTx / steps;

            var result = new ReconResult();
            int lastPhase = -1;

            for (int i = 1; i <= steps; i++)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(delayPerStep, token);

                int count = (i == steps) ? totalTx - perStep * (steps - 1) : perStep;
                int mismatch = rnd.Next(0, Math.Max(1, count / 400));
                int pending = rnd.Next(0, Math.Max(1, count / 600));

                result.TotalTransactions += count;
                result.Mismatched += mismatch;
                result.Pending += pending;
                result.Matched += count - mismatch - pending;
                result.TotalWithdrawn += (decimal)count * rnd.Next(2, 25) * 100_000M;

                int phase = (i - 1) * Phases.Length / steps;
                string msg = null;
                if (phase != lastPhase)
                {
                    lastPhase = phase;
                    msg = Phases[phase];
                }
                progress.Report(new ReconProgress(i * 100 / steps, msg, result.TotalTransactions));
            }

            result.Elapsed = sw.Elapsed;
            return result;
        }

        // ================= CẬP NHẬT UI (luôn chạy trên UI Thread) =================
        private void OnProgress(ReconProgress p)
        {
            if (IsDisposed) return;

            progressLocal.Value = p.Percent;
            lblTotalVal.Text = p.Processed.ToString("N0");
            lblState.Text = $"Đang xử lý... {p.Percent}%";
            if (p.Message != null) AppendLog(p.Message);

            // Thanh tiến trình ToolStripProgressBar trên Form cha
            ParentMain?.UpdateProgress(p.Percent, p.Message);
        }

        private void ShowResult(ReconResult r)
        {
            lblTotalVal.Text = r.TotalTransactions.ToString("N0");
            lblMatchedVal.Text = r.Matched.ToString("N0");
            lblMismatchVal.Text = r.Mismatched.ToString("N0");
            lblPendingVal.Text = r.Pending.ToString("N0");
            lblAmountVal.Text = r.TotalWithdrawn.ToString("N0");
            lblTimeVal.Text = r.Elapsed.TotalSeconds.ToString("0.0") + " giây";
            AppendLog($"Tổng: {r.TotalTransactions:N0} | Khớp: {r.Matched:N0} | Lệch: {r.Mismatched:N0} | Chờ xử lý: {r.Pending:N0}");
        }

        private void ResetResult()
        {
            progressLocal.Value = 0;
            lblTotalVal.Text = lblMatchedVal.Text = lblMismatchVal.Text = lblPendingVal.Text = "0";
            lblAmountVal.Text = "0";
            lblTimeVal.Text = "-";
        }

        private void SetRunning(bool running)
        {
            btnRun.Enabled = !running;
            btnStop.Enabled = running;
        }

        private void AppendLog(string text)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            txtLog.ScrollToCaret();
        }

        // Đóng form khi đang chạy: hủy tiến trình để tránh chạy ngầm vô chủ
        private void frmAtmReconciliation_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel();
        }
    }
}
