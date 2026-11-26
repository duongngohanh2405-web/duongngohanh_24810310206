using System.ComponentModel;
using BankAtmMdi.Models;

namespace BankAtmMdi
{
    public partial class frmAtmStations : Form
    {
        private const decimal LowCashThreshold = 50_000_000M;
        private readonly BindingList<AtmStation> _stations = new BindingList<AtmStation>();
        private string _currentImagePath;

        public frmAtmStations()
        {
            InitializeComponent();
        }

        private void frmAtmStations_Load(object sender, EventArgs e)
        {
            cboTrangThai.Items.AddRange(new object[] { "Hoạt động", "Bảo trì", "Ngừng hoạt động" });
            cboTrangThai.SelectedIndex = 0;

            ConfigureGrid();
            SeedData();
            dgvStations.DataSource = _stations;
            ClearForm();
        }

        private void ConfigureGrid()
        {
            dgvStations.AutoGenerateColumns = false;
            dgvStations.Columns.Clear();
            dgvStations.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "MaTram", HeaderText = "Mã trạm", Width = 90 });
            dgvStations.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "DiaChi", HeaderText = "Địa chỉ đặt máy", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvStations.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "TonQuy", HeaderText = "Tồn quỹ (VNĐ)", Width = 130,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
            });
            dgvStations.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "TrangThai", HeaderText = "Trạng thái", Width = 120 });
            dgvStations.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "AnhHienTruong", HeaderText = "Ảnh hiện trường", Width = 130 });
        }

        private void SeedData()
        {
            _stations.Add(new AtmStation { MaTram = "ATM001", DiaChi = "123 Xuân Thủy, Cầu Giấy, Hà Nội", TonQuy = 450_000_000M, TrangThai = "Hoạt động" });
            _stations.Add(new AtmStation { MaTram = "ATM002", DiaChi = "45 Trần Duy Hưng, Cầu Giấy, Hà Nội", TonQuy = 32_000_000M, TrangThai = "Hoạt động" });
            _stations.Add(new AtmStation { MaTram = "ATM003", DiaChi = "10 Láng Hạ, Đống Đa, Hà Nội", TonQuy = 780_000_000M, TrangThai = "Hoạt động" });
            _stations.Add(new AtmStation { MaTram = "ATM004", DiaChi = "88 Giải Phóng, Hai Bà Trưng, Hà Nội", TonQuy = 0M, TrangThai = "Bảo trì" });
            _stations.Add(new AtmStation { MaTram = "ATM005", DiaChi = "5 Nguyễn Trãi, Thanh Xuân, Hà Nội", TonQuy = 215_500_000M, TrangThai = "Ngừng hoạt động" });
        }

        // Tô đỏ tồn quỹ thấp (< 50 triệu) để giao dịch viên chú ý tiếp quỹ
        private void dgvStations_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgvStations.Columns[e.ColumnIndex].DataPropertyName != "TonQuy") return;
            if (dgvStations.Rows[e.RowIndex].DataBoundItem is AtmStation s && s.TonQuy < LowCashThreshold)
            {
                e.CellStyle.ForeColor = Color.Firebrick;
                e.CellStyle.Font = new Font(dgvStations.Font, FontStyle.Bold);
            }
        }

        private void dgvStations_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStations.CurrentRow?.DataBoundItem is not AtmStation s) return;

            txtMa.Text = s.MaTram;
            txtMa.ReadOnly = true; // không cho sửa khóa chính
            txtDiaChi.Text = s.DiaChi;
            numTonQuy.Value = s.TonQuy;
            cboTrangThai.SelectedItem = s.TrangThai;
            _currentImagePath = s.ImagePath;
            LoadImage(_currentImagePath);
        }

        // ================= ẢNH HIỆN TRƯỜNG (OpenFileDialog) =================
        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh chụp hiện trường trạm ATM",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Tất cả tệp (*.*)|*.*"
            };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                LoadImage(ofd.FileName);
                _currentImagePath = ofd.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadImage(string path)
        {
            picAnh.Image?.Dispose();
            picAnh.Image = null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            // Đọc qua MemoryStream + clone Bitmap để không khóa file ảnh gốc
            byte[] bytes = File.ReadAllBytes(path);
            using var ms = new MemoryStream(bytes);
            using var img = Image.FromStream(ms);
            picAnh.Image = new Bitmap(img);
        }

        // ================= CRUD =================
        private bool ValidateInput(bool isNew)
        {
            string ma = txtMa.Text.Trim();
            if (ma.Length == 0)
            {
                MessageBox.Show("Vui lòng nhập mã trạm.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMa.Focus();
                return false;
            }
            if (isNew && _stations.Any(x => string.Equals(x.MaTram, ma, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã trạm đã tồn tại.", "Trùng mã", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMa.Focus();
                return false;
            }
            if (txtDiaChi.Text.Trim().Length == 0)
            {
                MessageBox.Show("Vui lòng nhập địa chỉ đặt máy.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiaChi.Focus();
                return false;
            }
            return true;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (txtMa.ReadOnly)
            {
                MessageBox.Show("Bấm \"Làm mới / Nhập mới\" trước khi thêm trạm mới.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput(true)) return;

            _stations.Add(new AtmStation
            {
                MaTram = txtMa.Text.Trim().ToUpper(),
                DiaChi = txtDiaChi.Text.Trim(),
                TonQuy = numTonQuy.Value,
                TrangThai = cboTrangThai.SelectedItem.ToString(),
                ImagePath = _currentImagePath
            });
            ClearForm();
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvStations.CurrentRow?.DataBoundItem is not AtmStation s || !txtMa.ReadOnly)
            {
                MessageBox.Show("Vui lòng chọn một trạm trong danh sách để sửa.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput(false)) return;

            s.DiaChi = txtDiaChi.Text.Trim();
            s.TonQuy = numTonQuy.Value;
            s.TrangThai = cboTrangThai.SelectedItem.ToString();
            s.ImagePath = _currentImagePath;
            _stations.ResetItem(_stations.IndexOf(s));
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (dgvStations.CurrentRow?.DataBoundItem is not AtmStation s || !txtMa.ReadOnly)
            {
                MessageBox.Show("Vui lòng chọn một trạm trong danh sách để xóa.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show($"Xóa trạm {s.MaTram}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            _stations.Remove(s);
            ClearForm();
        }

        private void btnLamMoi_Click(object sender, EventArgs e) => ClearForm();

        private void ClearForm()
        {
            dgvStations.ClearSelection();
            dgvStations.CurrentCell = null;
            txtMa.ReadOnly = false;
            txtMa.Clear();
            txtDiaChi.Clear();
            numTonQuy.Value = 0;
            cboTrangThai.SelectedIndex = 0;
            _currentImagePath = null;
            picAnh.Image?.Dispose();
            picAnh.Image = null;
            txtMa.Focus();
        }

        private void frmAtmStations_FormClosed(object sender, FormClosedEventArgs e)
        {
            picAnh.Image?.Dispose();
        }
    }
}
