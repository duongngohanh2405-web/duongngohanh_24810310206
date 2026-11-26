namespace PowerGridMonitor
{
    public partial class frmPowerGridMonitor : Form
    {
        private const double OverloadThreshold = 85.0;   // ngưỡng an toàn (%)
        private const int ScanIntervalMs = 2000;          // 2 giây / chu kỳ

        private CancellationTokenSource _cts;
        private int _overloadCount;                       // chỉ đọc/ghi trên UI Thread

        public frmPowerGridMonitor()
        {
            InitializeComponent();
        }

        private void frmPowerGridMonitor_Load(object sender, EventArgs e)
        {
            cboStation.Items.AddRange(new object[]
            {
                "Trạm 110kV Hà Đông",
                "Trạm 220kV Chèm",
                "Trạm 110kV Mai Động",
                "Trạm 220kV Thanh Xuân"
            });
            cboStation.SelectedIndex = 0;

            ConfigureGrid();
            ResetDashboard();
            lblState.Text = "Trạng thái: Chưa kết nối";
            lblLastPacket.Text = "Gói tin cuối: --:--:--";
            lblCycles.Text = "Tổng chu kỳ quét: 0";
            lblOverloads.Text = "Số lần quá tải: 0";
        }

        // ================= DataGridView cấu hình thủ công bằng code =================
        private void ConfigureGrid()
        {
            dgvGridLogs.AutoGenerateColumns = false;
            dgvGridLogs.AllowUserToAddRows = false;
            dgvGridLogs.AllowUserToDeleteRows = false;
            dgvGridLogs.ReadOnly = true;
            dgvGridLogs.RowHeadersVisible = false;
            dgvGridLogs.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGridLogs.MultiSelect = false;
            dgvGridLogs.BackgroundColor = SystemColors.Window;

            dgvGridLogs.Columns.Clear();
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSTT", HeaderText = "STT", Width = 55 });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTime", HeaderText = "Thời gian", Width = 150 });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStation", HeaderText = "Trạm biến áp", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVoltage", HeaderText = "Điện áp (kV)", Width = 110 });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLoad", HeaderText = "Mức tải (%)", Width = 100 });
            dgvGridLogs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "Trạng thái", Width = 150 });
        }

        // ================= ĐIỀU KHIỂN =================
        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_cts != null) return; // đang giám sát

            // Đọc ComboBox NGAY TẠI UI Thread rồi truyền giá trị vào Task (tuyệt đối không đọc control trong Thread phụ)
            string station = cboStation.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(station))
            {
                MessageBox.Show("Vui lòng chọn trạm biến áp.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            _overloadCount = 0;
            lblOverloads.Text = "Số lần quá tải: 0";
            dgvGridLogs.Rows.Clear();
            SetMonitoring(true, station);

            try
            {
                // Vòng lặp SCADA chạy ngầm trên ThreadPool
                await Task.Run(() => ScadaLoopAsync(station, token));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giám sát bị gián đoạn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Đã quay lại UI Thread (await) nên được phép đụng vào control
                _cts?.Dispose();
                _cts = null;
                if (!IsDisposed)
                {
                    SetMonitoring(false, station);
                    ResetDashboard();
                }
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            btnDisconnect.Enabled = false;
            lblState.Text = "Trạng thái: Đang ngắt kết nối...";
            _cts?.Cancel();
        }

        private void SetMonitoring(bool running, string station)
        {
            btnStart.Enabled = !running;
            btnDisconnect.Enabled = running;
            cboStation.Enabled = !running;
            lblState.Text = running
                ? "Trạng thái: Đang giám sát - " + station
                : "Trạng thái: Đã ngắt kết nối";
        }

        private void ResetDashboard()
        {
            lblVoltage.Text = "-- kV";
            lblVoltage.ForeColor = Color.DimGray;
            lblLoadPercent.Text = "-- %";
            lblLoadPercent.ForeColor = Color.DimGray;
            lblWarning.Visible = false;
        }

        // ================= TIẾN TRÌNH NGẦM (Background Thread) =================
        /// <summary>
        /// Chạy trên ThreadPool. Hàm này CHỈ sinh dữ liệu; mọi thao tác lên giao diện
        /// đều phải đi qua UpdateDashboard() (có InvokeRequired + Invoke).
        /// </summary>
        private async Task ScadaLoopAsync(string station, CancellationToken token)
        {
            int cycle = 0;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Giả lập gói tin SCADA
                    double voltage = Math.Round(108.0 + Random.Shared.NextDouble() * 7.0, 2);   // 108 - 115 kV
                    double load = Math.Round(50.0 + Random.Shared.NextDouble() * 48.0, 1);      // 50 - 98 %
                    DateTime receivedAt = DateTime.Now;
                    cycle++;

                    try
                    {
                        UpdateDashboard(station, receivedAt, voltage, load, cycle);
                    }
                    catch (ObjectDisposedException) { break; }                 // form đã đóng
                    catch (InvalidOperationException) when (IsDisposed || Disposing) { break; }

                    await Task.Delay(ScanIntervalMs, token);
                }
            }
            catch (OperationCanceledException)
            {
                // Người dùng bấm "Ngắt Kết nối": thoát vòng lặp êm đẹp
            }
        }

        // ================= CẬP NHẬT GIAO DIỆN AN TOÀN ĐA LUỒNG =================
        private void UpdateDashboard(string station, DateTime receivedAt, double voltage, double load, int cycle)
        {
            // Đang ở Thread phụ -> chuyển ngữ cảnh về Main UI Thread
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateDashboard(station, receivedAt, voltage, load, cycle)));
                return;
            }

            // ----- Từ đây trở xuống chắc chắn đang chạy trên UI Thread -----
            lblVoltage.Text = $"{voltage:0.00} kV";
            lblVoltage.ForeColor = Color.RoyalBlue;

            lblLoadPercent.Text = $"{load:0.0} %";
            bool overload = load > OverloadThreshold;

            if (overload)
            {
                lblLoadPercent.ForeColor = Color.Red;
                lblWarning.Text = $"⚠ CẢNH BÁO: {station} QUÁ TẢI ({load:0.0}% > {OverloadThreshold:0}%)!";
                lblWarning.Visible = true;

                _overloadCount++;
                int idx = dgvGridLogs.Rows.Add(
                    _overloadCount,
                    receivedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                    station,
                    voltage.ToString("0.00"),
                    load.ToString("0.0"),
                    "QUÁ TẢI");
                dgvGridLogs.Rows[idx].DefaultCellStyle.ForeColor = Color.Firebrick;
                dgvGridLogs.FirstDisplayedScrollingRowIndex = idx; // cuộn tới dòng mới nhất
                lblOverloads.Text = $"Số lần quá tải: {_overloadCount}";
            }
            else
            {
                lblLoadPercent.ForeColor = Color.Green;
                lblWarning.Visible = false;
            }

            lblLastPacket.Text = "Gói tin cuối: " + receivedAt.ToString("HH:mm:ss");
            lblCycles.Text = "Tổng chu kỳ quét: " + cycle;
        }

        private void frmPowerGridMonitor_FormClosing(object sender, FormClosingEventArgs e)
        {
            _cts?.Cancel(); // dừng tiến trình ngầm khi đóng form
        }
    }
}
