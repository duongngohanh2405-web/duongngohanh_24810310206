using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClinicAppointmentManager
{
    public class FrmClinicAppointment : Form
    {
        private readonly BindingList<LichHen> danhSach = new BindingList<LichHen>();
        private readonly BindingSource bindingSource = new BindingSource();
        private readonly ErrorProvider errorProvider = new ErrorProvider();

        private TextBox txtMaLichHen, txtHoTen, txtSoDienThoai, txtLyDoKham, txtTimKiem;
        private ComboBox cboBacSi, cboTrangThai, cboLocTrangThai;
        private DateTimePicker dtpNgayHen, dtpLocNgay;
        private CheckBox chkLocNgay;
        private DataGridView dgvLichHen;
        private ToolStripStatusLabel lblSoLuong;
        private Button btnThem, btnSua, btnXoa, btnLamMoi;

        private bool dangNapDuLieu;

        public FrmClinicAppointment()
        {
            Text = "Clinic Appointment Manager - Quản lý lịch hẹn phòng khám";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1050, 680);
            Size = new Size(1250, 780);
            Font = new Font("Segoe UI", 9F);
            BuildInterface();
            ConfigureControls();
            SetupGrid();
            bindingSource.DataSource = danhSach;
            dgvLichHen.DataSource = bindingSource;
            CapNhatSoLuong();
        }

        private void BuildInterface()
        {
            var menu = new MenuStrip();
            var menuFile = new ToolStripMenuItem("Tệp");
            var menuExport = new ToolStripMenuItem("Xuất danh sách đang hiển thị ra CSV");
            var menuExit = new ToolStripMenuItem("Thoát");
            menuExport.Click += XuatCSV_Click;
            menuExit.Click += (s, e) => Close();
            menuFile.DropDownItems.Add(menuExport);
            menuFile.DropDownItems.Add(new ToolStripSeparator());
            menuFile.DropDownItems.Add(menuExit);
            menu.Items.Add(menuFile);
            MainMenuStrip = menu;
            Controls.Add(menu);

            var status = new StatusStrip();
            lblSoLuong = new ToolStripStatusLabel("Tổng số lịch hẹn: 0");
            status.Items.Add(lblSoLuong);
            Controls.Add(status);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12),
                BackColor = Color.White
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 250));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);
            root.BringToFront();

            var title = new Label
            {
                Text = "QUẢN LÝ LỊCH HẸN PHÒNG KHÁM",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 75, 120),
                TextAlign = ContentAlignment.MiddleLeft
            };
            root.Controls.Add(title, 0, 0);

            var inputGroup = new GroupBox
            {
                Text = "Thông tin lịch hẹn",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            root.Controls.Add(inputGroup, 0, 1);

            var input = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(4)
            };
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 4; i++)
                input.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            inputGroup.Controls.Add(input);

            txtMaLichHen = NewTextBox();
            txtHoTen = NewTextBox();
            txtSoDienThoai = NewTextBox();
            txtLyDoKham = NewTextBox();
            cboBacSi = NewComboBox();
            cboTrangThai = NewComboBox();
            dtpNgayHen = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", ShowUpDown = true };

            AddField(input, "Mã lịch hẹn:", txtMaLichHen, 0, 0);
            AddField(input, "Họ tên bệnh nhân:", txtHoTen, 2, 0);
            AddField(input, "Số điện thoại:", txtSoDienThoai, 0, 1);
            AddField(input, "Ngày giờ hẹn:", dtpNgayHen, 2, 1);
            AddField(input, "Bác sĩ:", cboBacSi, 0, 2);
            AddField(input, "Trạng thái:", cboTrangThai, 2, 2);
            AddField(input, "Lý do khám:", txtLyDoKham, 0, 3, 3);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 5, 0, 0)
            };
            btnThem = NewButton("Thêm", Color.FromArgb(39, 142, 85));
            btnSua = NewButton("Cập nhật", Color.FromArgb(45, 110, 175));
            btnXoa = NewButton("Xóa", Color.FromArgb(190, 65, 65));
            btnLamMoi = NewButton("Làm mới", Color.FromArgb(100, 100, 100));
            buttonPanel.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnLamMoi });

            var listPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };
            listPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            listPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(listPanel, 0, 2);

            var filter = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(0, 5, 0, 5)
            };
            filter.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Padding = new Padding(0, 7, 4, 0) });
            txtTimKiem = new TextBox { Width = 210 };
            filter.Controls.Add(txtTimKiem);
            filter.Controls.Add(new Label { Text = "Ngày hẹn:", AutoSize = true, Padding = new Padding(12, 7, 4, 0) });
            chkLocNgay = new CheckBox { Text = "Lọc ngày", AutoSize = true, Checked = false, Padding = new Padding(0, 5, 0, 0) };
            dtpLocNgay = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Enabled = false };
            filter.Controls.Add(chkLocNgay);
            filter.Controls.Add(dtpLocNgay);
            filter.Controls.Add(new Label { Text = "Trạng thái:", AutoSize = true, Padding = new Padding(12, 7, 4, 0) });
            cboLocTrangThai = NewComboBox();
            cboLocTrangThai.Width = 140;
            filter.Controls.Add(cboLocTrangThai);
            var btnHienTatCa = NewButton("Bỏ lọc", Color.FromArgb(100, 100, 100));
            btnHienTatCa.Width = 85;
            filter.Controls.Add(btnHienTatCa);
            listPanel.Controls.Add(filter, 0, 0);

            dgvLichHen = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.Fixed3D
            };
            dgvLichHen.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvLichHen.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(225, 235, 245);
            dgvLichHen.EnableHeadersVisualStyles = false;
            listPanel.Controls.Add(dgvLichHen, 0, 1);

            btnThem.Click += Them_Click;
            btnSua.Click += Sua_Click;
            btnXoa.Click += Xoa_Click;
            btnLamMoi.Click += (s, e) => LamMoi();
            txtTimKiem.TextChanged += (s, e) => LocDuLieu();
            chkLocNgay.CheckedChanged += (s, e) => { dtpLocNgay.Enabled = chkLocNgay.Checked; LocDuLieu(); };
            dtpLocNgay.ValueChanged += (s, e) => LocDuLieu();
            cboLocTrangThai.SelectedIndexChanged += (s, e) => LocDuLieu();
            dgvLichHen.CellClick += dgvLichHen_CellClick;
            btnHienTatCa.Click += (s, e) =>
            {
                txtTimKiem.Clear();
                chkLocNgay.Checked = false;
                cboLocTrangThai.SelectedIndex = 0;
                LocDuLieu();
            };
        }

        private void ConfigureControls()
        {
            cboBacSi.Items.AddRange(new object[] { "Nguyễn Văn An", "Trần Thị Bình", "Lê Văn Cường", "Phạm Thu Hà" });
            cboTrangThai.Items.AddRange(new object[] { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cboLocTrangThai.Items.AddRange(new object[] { "Tất cả", "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" });
            cboTrangThai.SelectedIndex = 0;
            cboLocTrangThai.SelectedIndex = 0;
            dtpNgayHen.MinDate = DateTime.Today;
            dtpNgayHen.Value = DateTime.Now.AddMinutes(5);
            errorProvider.ContainerControl = this;
        }

        private TextBox NewTextBox()
        {
            return new TextBox { Dock = DockStyle.Fill, Margin = new Padding(3, 5, 10, 5) };
        }

        private ComboBox NewComboBox()
        {
            return new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(3, 5, 10, 5) };
        }

        private Button NewButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 105,
                Height = 32,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(4),
                Cursor = Cursors.Hand
            };
        }

        private void AddField(TableLayoutPanel panel, string label, Control control, int labelColumn, int row, int span = 1)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            }, labelColumn, row);
            panel.Controls.Add(control, labelColumn + 1, row);
            if (span > 1)
                panel.SetColumnSpan(control, span);
        }

        private void AddColumn(string property, string header)
        {
            dgvLichHen.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = property,
                DataPropertyName = property,
                HeaderText = header,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void SetupGrid()
        {
            dgvLichHen.Columns.Clear();
            AddColumn("MaLichHen", "Mã lịch hẹn");
            AddColumn("HoTen", "Họ tên bệnh nhân");
            AddColumn("SoDienThoai", "Số điện thoại");
            AddColumn("NgayHen", "Ngày giờ hẹn");
            AddColumn("BacSi", "Bác sĩ");
            AddColumn("LyDoKham", "Lý do khám");
            AddColumn("TrangThai", "Trạng thái");
        }

        private void Them_Click(object sender, EventArgs e)
        {
            if (!KiemTraDuLieu()) return;

            string ma = txtMaLichHen.Text.Trim();
            if (danhSach.Any(x => string.Equals(x.MaLichHen, ma, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtMaLichHen, "Mã lịch hẹn đã tồn tại.");
                return;
            }

            danhSach.Add(LayDuLieuTuForm());
            LamMoi();
            LocDuLieu();
        }

        private void Sua_Click(object sender, EventArgs e)
        {
            LichHen selected = bindingSource.Current as LichHen;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!KiemTraDuLieu()) return;

            string ma = txtMaLichHen.Text.Trim();
            if (danhSach.Any(x => x != selected && string.Equals(x.MaLichHen, ma, StringComparison.OrdinalIgnoreCase)))
            {
                errorProvider.SetError(txtMaLichHen, "Mã lịch hẹn đã tồn tại.");
                return;
            }

            LichHen updated = LayDuLieuTuForm();
            selected.MaLichHen = updated.MaLichHen;
            selected.HoTen = updated.HoTen;
            selected.SoDienThoai = updated.SoDienThoai;
            selected.NgayHen = updated.NgayHen;
            selected.BacSi = updated.BacSi;
            selected.LyDoKham = updated.LyDoKham;
            selected.TrangThai = updated.TrangThai;

            LocDuLieu();
            LamMoi();
        }

        private void Xoa_Click(object sender, EventArgs e)
        {
            LichHen selected = bindingSource.Current as LichHen;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa lịch hẹn " + selected.MaLichHen + " không?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                danhSach.Remove(selected);
                LamMoi();
                LocDuLieu();
            }
        }

        private bool KiemTraDuLieu()
        {
            errorProvider.Clear();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtMaLichHen.Text))
            {
                errorProvider.SetError(txtMaLichHen, "Vui lòng nhập mã lịch hẹn.");
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                errorProvider.SetError(txtHoTen, "Vui lòng nhập họ tên bệnh nhân.");
                valid = false;
            }
            if (!Regex.IsMatch(txtSoDienThoai.Text.Trim(), @"^0\d{9}$"))
            {
                errorProvider.SetError(txtSoDienThoai, "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.");
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(txtLyDoKham.Text))
            {
                errorProvider.SetError(txtLyDoKham, "Vui lòng nhập lý do khám.");
                valid = false;
            }
            if (dtpNgayHen.Value < DateTime.Now)
            {
                errorProvider.SetError(dtpNgayHen, "Ngày giờ hẹn không được nằm trong quá khứ.");
                valid = false;
            }
            if (cboBacSi.SelectedIndex < 0)
            {
                errorProvider.SetError(cboBacSi, "Vui lòng chọn bác sĩ.");
                valid = false;
            }
            if (cboTrangThai.SelectedIndex < 0)
            {
                errorProvider.SetError(cboTrangThai, "Vui lòng chọn trạng thái.");
                valid = false;
            }
            return valid;
        }

        private LichHen LayDuLieuTuForm()
        {
            return new LichHen
            {
                MaLichHen = txtMaLichHen.Text.Trim(),
                HoTen = txtHoTen.Text.Trim(),
                SoDienThoai = txtSoDienThoai.Text.Trim(),
                NgayHen = dtpNgayHen.Value,
                BacSi = cboBacSi.Text,
                LyDoKham = txtLyDoKham.Text.Trim(),
                TrangThai = cboTrangThai.Text
            };
        }

        private void dgvLichHen_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            LichHen selected = dgvLichHen.Rows[e.RowIndex].DataBoundItem as LichHen;
            if (selected == null) return;

            dangNapDuLieu = true;
            txtMaLichHen.Text = selected.MaLichHen;
            txtHoTen.Text = selected.HoTen;
            txtSoDienThoai.Text = selected.SoDienThoai;
            DateTime safeDate = selected.NgayHen < dtpNgayHen.MinDate ? dtpNgayHen.MinDate : selected.NgayHen;
            dtpNgayHen.Value = safeDate;
            cboBacSi.Text = selected.BacSi;
            txtLyDoKham.Text = selected.LyDoKham;
            cboTrangThai.Text = selected.TrangThai;
            errorProvider.Clear();
            dangNapDuLieu = false;
        }

        private void LocDuLieu()
        {
            if (dangNapDuLieu) return;

            string keyword = txtTimKiem.Text.Trim();
            string status = cboLocTrangThai.SelectedItem == null ? "Tất cả" : cboLocTrangThai.Text;

            var query = danhSach.AsEnumerable();

            if (keyword.Length > 0)
            {
                query = query.Where(x =>
                    (x.HoTen ?? "").IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (x.SoDienThoai ?? "").IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            if (chkLocNgay.Checked)
                query = query.Where(x => x.NgayHen.Date == dtpLocNgay.Value.Date);
            if (status != "Tất cả")
                query = query.Where(x => x.TrangThai == status);

            bindingSource.DataSource = new BindingList<LichHen>(query.ToList());
            CapNhatSoLuong();
        }

        private void LamMoi()
        {
            errorProvider.Clear();
            txtMaLichHen.Clear();
            txtHoTen.Clear();
            txtSoDienThoai.Clear();
            txtLyDoKham.Clear();
            cboBacSi.SelectedIndex = -1;
            cboTrangThai.SelectedIndex = 0;
            dtpNgayHen.Value = DateTime.Now.AddMinutes(5);
            dgvLichHen.ClearSelection();
            txtMaLichHen.Focus();
        }

        private void CapNhatSoLuong()
        {
            lblSoLuong.Text = "Số lịch hẹn đang hiển thị: " + dgvLichHen.Rows.Count;
        }

        private void XuatCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog save = new SaveFileDialog())
            {
                save.Filter = "CSV (*.csv)|*.csv";
                save.Title = "Xuất danh sách lịch hẹn";
                save.FileName = "DanhSachLichHen.csv";

                if (save.ShowDialog() != DialogResult.OK) return;

                StringBuilder csv = new StringBuilder();
                csv.AppendLine("MaLichHen,HoTen,SoDienThoai,NgayHen,BacSi,LyDoKham,TrangThai");

                foreach (DataGridViewRow row in dgvLichHen.Rows)
                {
                    if (row.IsNewRow) continue;
                    string[] values = new string[dgvLichHen.Columns.Count];
                    for (int i = 0; i < dgvLichHen.Columns.Count; i++)
                    {
                        string value = Convert.ToString(row.Cells[i].Value) ?? "";
                        if (i == 3 && row.Cells[i].Value is DateTime)
                            value = ((DateTime)row.Cells[i].Value).ToString("dd/MM/yyyy HH:mm");
                        values[i] = "\"" + value.Replace("\"", "\"\"") + "\"";
                    }
                    csv.AppendLine(string.Join(",", values));
                }

                File.WriteAllText(save.FileName, "\uFEFF" + csv.ToString(), Encoding.UTF8);
                MessageBox.Show("Đã xuất danh sách đang hiển thị ra CSV.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
