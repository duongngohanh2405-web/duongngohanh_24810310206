using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ClinicAppointmentManager;

public class MainForm : Form
{
    // ---------- Dữ liệu ----------
    static readonly string[] Doctors =
        { "BS. Nguyễn Văn An", "BS. Trần Thị Bình", "BS. Lê Minh Châu", "BS. Phạm Quốc Dũng" };

    static readonly string[] Statuses = { "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy" };

    // SĐT di động VN: 0xxxxxxxxx hoặc +84xxxxxxxxx (đầu số 3,5,7,8,9)
    static readonly Regex PhoneRegex = new(@"^(0|\+84)(3|5|7|8|9)\d{8}$");

    readonly List<Appointment> _all = new();              // toàn bộ lịch hẹn
    readonly BindingList<Appointment> _view = new();      // danh sách sau khi lọc (nguồn của grid)
    readonly BindingSource _bs = new();
    Appointment? _selected;                               // lịch đang chọn (null = chế độ thêm mới)
    bool _loading;                                        // chặn SelectionChanged khi nạp lại lưới

    // ---------- Controls ----------
    readonly TextBox txtId = new();
    readonly TextBox txtName = new();
    readonly TextBox txtPhone = new();
    readonly DateTimePicker dtpTime = new();
    readonly ComboBox cboDoctor = new();
    readonly ComboBox cboStatus = new();
    readonly TextBox txtReason = new();

    readonly Button btnAdd = new() { Text = "Thêm" };
    readonly Button btnUpdate = new() { Text = "Cập nhật" };
    readonly Button btnDelete = new() { Text = "Xóa" };
    readonly Button btnNew = new() { Text = "Làm mới" };

    readonly TextBox txtSearch = new();
    readonly DateTimePicker dtpFilter = new();
    readonly ComboBox cboStatusFilter = new();
    readonly Button btnClearFilter = new() { Text = "Xóa lọc" };

    readonly DataGridView grid = new();
    readonly ErrorProvider errors = new();
    readonly ToolStripStatusLabel lblCount = new();

    public MainForm()
    {
        Text = "Clinic Appointment Manager";
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 700);
        MinimumSize = new Size(900, 600);

        errors.ContainerControl = this;
        errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        BuildUi();
        Seed();

        _bs.DataSource = _view;
        grid.DataSource = _bs;

        WireEvents();
        ApplyFilter();
        ClearForm();
    }

    // =====================================================================
    //  GIAO DIỆN
    // =====================================================================
    void BuildUi()
    {
        // ----- MenuStrip -----
        var menu = new MenuStrip();
        var mFile = new ToolStripMenuItem("&Tệp");
        var mExport = new ToolStripMenuItem("&Xuất CSV...", null, (_, _) => ExportCsv())
        { ShortcutKeys = Keys.Control | Keys.E };
        var mExit = new ToolStripMenuItem("T&hoát", null, (_, _) => Close());
        mFile.DropDownItems.AddRange(new ToolStripItem[] { mExport, new ToolStripSeparator(), mExit });
        menu.Items.Add(mFile);
        MainMenuStrip = menu;

        // ----- StatusStrip -----
        var status = new StatusStrip();
        status.Items.Add(lblCount);

        // ----- Layout gốc: 3 hàng (nhập liệu / lọc / lưới) -----
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildInputGroup(), 0, 0);
        root.Controls.Add(BuildFilterGroup(), 0, 1);
        BuildGrid();
        root.Controls.Add(grid, 0, 2);

        // Thứ tự add: control Fill trước, Menu/Status sau để dock đúng
        Controls.Add(root);
        Controls.Add(status);
        Controls.Add(menu);
    }

    GroupBox BuildInputGroup()
    {
        var t = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            Padding = new Padding(6)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        dtpTime.Format = DateTimePickerFormat.Custom;
        dtpTime.CustomFormat = "dd/MM/yyyy HH:mm";
        cboDoctor.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDoctor.Items.AddRange(Doctors);
        cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatus.Items.AddRange(Statuses);

        AddField(t, "Mã lịch hẹn", txtId, 0, 0);
        AddField(t, "Họ tên bệnh nhân", txtName, 0, 2);
        AddField(t, "Số điện thoại", txtPhone, 1, 0);
        AddField(t, "Ngày giờ hẹn", dtpTime, 1, 2);
        AddField(t, "Bác sĩ", cboDoctor, 2, 0);
        AddField(t, "Trạng thái", cboStatus, 2, 2);
        AddField(t, "Lý do khám", txtReason, 3, 0, span: 3);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        foreach (var b in new[] { btnAdd, btnUpdate, btnDelete, btnNew })
        {
            b.AutoSize = true;
            b.MinimumSize = new Size(90, 30);
            buttons.Controls.Add(b);
        }
        t.Controls.Add(buttons, 0, 4);
        t.SetColumnSpan(buttons, 4);

        var g = new GroupBox
        {
            Text = "Thông tin lịch hẹn",
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6)
        };
        g.Controls.Add(t);
        return g;
    }

    GroupBox BuildFilterGroup()
    {
        var t = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 7,
            RowCount = 1,
            Padding = new Padding(6)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        txtSearch.PlaceholderText = "Nhập tên bệnh nhân hoặc số điện thoại...";
        dtpFilter.Format = DateTimePickerFormat.Short;
        dtpFilter.ShowCheckBox = true;
        dtpFilter.Checked = false;
        cboStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatusFilter.Items.Add("Tất cả");
        cboStatusFilter.Items.AddRange(Statuses);
        cboStatusFilter.SelectedIndex = 0;
        btnClearFilter.AutoSize = true;

        txtSearch.Dock = DockStyle.Fill;
        dtpFilter.Dock = DockStyle.Fill;
        cboStatusFilter.Dock = DockStyle.Fill;

        t.Controls.Add(MakeLabel("Tìm kiếm"), 0, 0);
        t.Controls.Add(txtSearch, 1, 0);
        t.Controls.Add(MakeLabel("Ngày hẹn"), 2, 0);
        t.Controls.Add(dtpFilter, 3, 0);
        t.Controls.Add(MakeLabel("Trạng thái"), 4, 0);
        t.Controls.Add(cboStatusFilter, 5, 0);
        t.Controls.Add(btnClearFilter, 6, 0);

        var g = new GroupBox
        {
            Text = "Tìm kiếm & lọc",
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6)
        };
        g.Controls.Add(t);
        return g;
    }

    void BuildGrid()
    {
        grid.Dock = DockStyle.Fill;
        grid.AutoGenerateColumns = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.RowHeadersVisible = false;
        grid.BackgroundColor = SystemColors.Window;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.Margin = new Padding(3, 8, 3, 3);

        grid.Columns.Add(Col(nameof(Appointment.Id), "Mã lịch hẹn", 12));
        grid.Columns.Add(Col(nameof(Appointment.PatientName), "Họ tên bệnh nhân", 20));
        grid.Columns.Add(Col(nameof(Appointment.Phone), "Số điện thoại", 13));
        var colTime = Col(nameof(Appointment.AppointmentTime), "Ngày giờ hẹn", 15);
        colTime.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        grid.Columns.Add(colTime);
        grid.Columns.Add(Col(nameof(Appointment.Doctor), "Bác sĩ", 18));
        grid.Columns.Add(Col(nameof(Appointment.Reason), "Lý do khám", 22));
        grid.Columns.Add(Col(nameof(Appointment.Status), "Trạng thái", 12));
    }

    static DataGridViewTextBoxColumn Col(string prop, string header, float weight) => new()
    {
        DataPropertyName = prop,
        Name = "col" + prop,
        HeaderText = header,
        FillWeight = weight
    };

    static Label MakeLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(3, 6, 8, 6)
    };

    static void AddField(TableLayoutPanel t, string label, Control c, int row, int col, int span = 1)
    {
        t.Controls.Add(MakeLabel(label), col, row);
        c.Dock = DockStyle.Fill;
        c.Margin = new Padding(3, 3, 24, 3); // chừa chỗ cho biểu tượng ErrorProvider
        t.Controls.Add(c, col + 1, row);
        if (span > 1) t.SetColumnSpan(c, span);
    }

    // =====================================================================
    //  SỰ KIỆN
    // =====================================================================
    void WireEvents()
    {
        btnAdd.Click += (_, _) => AddAppointment();
        btnUpdate.Click += (_, _) => UpdateAppointment();
        btnDelete.Click += (_, _) => DeleteAppointment();
        btnNew.Click += (_, _) => NewEntry();

        grid.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            if (grid.CurrentRow?.DataBoundItem is Appointment a) LoadToForm(a);
        };
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0) return;
            if (grid.Rows[e.RowIndex].DataBoundItem is Appointment a)
                grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor =
                    a.Status == "Đã hủy" ? Color.Gray : grid.DefaultCellStyle.ForeColor;
        };

        // Tìm kiếm / lọc tức thời
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        dtpFilter.ValueChanged += (_, _) => ApplyFilter();
        cboStatusFilter.SelectedIndexChanged += (_, _) => ApplyFilter();
        btnClearFilter.Click += (_, _) =>
        {
            txtSearch.Clear();
            dtpFilter.Checked = false;
            cboStatusFilter.SelectedIndex = 0;
            ApplyFilter();
        };

        // Xóa lỗi của từng trường khi người dùng sửa lại
        txtId.TextChanged += (_, _) => errors.SetError(txtId, "");
        txtName.TextChanged += (_, _) => errors.SetError(txtName, "");
        txtPhone.TextChanged += (_, _) => errors.SetError(txtPhone, "");
        txtReason.TextChanged += (_, _) => errors.SetError(txtReason, "");
        dtpTime.ValueChanged += (_, _) => errors.SetError(dtpTime, "");
        cboDoctor.SelectedIndexChanged += (_, _) => errors.SetError(cboDoctor, "");
        cboStatus.SelectedIndexChanged += (_, _) => errors.SetError(cboStatus, "");
    }

    // =====================================================================
    //  LỌC / TÌM KIẾM
    // =====================================================================
    void ApplyFilter(string? keepId = null)
    {
        string? selId = keepId ?? _selected?.Id;
        Appointment? target = null;

        string q = txtSearch.Text.Trim();
        string qFold = Fold(q);
        string qPhone = CanonPhone(q);
        bool byDate = dtpFilter.Checked;
        DateTime date = dtpFilter.Value.Date;
        string? st = cboStatusFilter.SelectedIndex > 0 ? cboStatusFilter.SelectedItem?.ToString() : null;

        _loading = true;
        try
        {
            var result = _all
                .Where(a => q.Length == 0
                            || Fold(a.PatientName).Contains(qFold)
                            || (qPhone.Length > 0 && a.Phone.Contains(qPhone)))
                .Where(a => !byDate || a.AppointmentTime.Date == date)
                .Where(a => st == null || a.Status == st)
                .OrderBy(a => a.AppointmentTime)
                .ToList();

            _view.RaiseListChangedEvents = false;
            _view.Clear();
            foreach (var a in result) _view.Add(a);
            _view.RaiseListChangedEvents = true;
            _view.ResetBindings();

            grid.ClearSelection();
            grid.CurrentCell = null;

            if (selId != null)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.DataBoundItem is Appointment a && a.Id == selId)
                    {
                        grid.CurrentCell = row.Cells[0];
                        row.Selected = true;
                        target = a;
                        break;
                    }
                }
            }
        }
        finally
        {
            _loading = false;
        }

        if (selId != null)
        {
            if (target == null) ClearForm();              // dòng đang chọn bị lọc mất
            else if (keepId != null) LoadToForm(target);  // vừa thêm/sửa -> nạp lại
        }

        lblCount.Text = $"Hiển thị {_view.Count} / {_all.Count} lịch hẹn";
    }

    // =====================================================================
    //  THÊM / SỬA / XÓA
    // =====================================================================
    void AddAppointment()
    {
        if (!ValidateForm(isAdd: true)) return;
        if (!Confirm("Thêm lịch hẹn mới?")) return;

        var a = ReadForm();
        _all.Add(a);
        ApplyFilter(a.Id);

        if (!_view.Contains(a))
            MessageBox.Show(this, "Đã thêm lịch hẹn, nhưng lịch này đang bị ẩn bởi bộ lọc hiện tại.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void UpdateAppointment()
    {
        if (_selected == null) return;
        if (!ValidateForm(isAdd: false)) return;
        if (!Confirm($"Cập nhật lịch hẹn {_selected.Id}?")) return;

        var f = ReadForm();
        _selected.PatientName = f.PatientName;
        _selected.Phone = f.Phone;
        _selected.AppointmentTime = f.AppointmentTime;
        _selected.Doctor = f.Doctor;
        _selected.Reason = f.Reason;
        _selected.Status = f.Status;

        ApplyFilter(_selected.Id);   // lọc lại: số lượng + trạng thái trên lưới cập nhật
    }

    void DeleteAppointment()
    {
        if (_selected == null) return;
        if (!Confirm($"Xóa lịch hẹn {_selected.Id} của {_selected.PatientName}?")) return;

        _all.Remove(_selected);
        ClearForm();
        ApplyFilter();
    }

    void NewEntry()
    {
        ClearForm();
        _loading = true;
        grid.ClearSelection();
        grid.CurrentCell = null;
        _loading = false;
        txtName.Focus();
    }

    bool Confirm(string message) =>
        MessageBox.Show(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        == DialogResult.Yes;

    // =====================================================================
    //  FORM <-> DỮ LIỆU
    // =====================================================================
    Appointment ReadForm() => new()
    {
        Id = txtId.Text.Trim(),
        PatientName = txtName.Text.Trim(),
        Phone = CanonPhone(txtPhone.Text),
        AppointmentTime = TruncateToMinute(dtpTime.Value),
        Doctor = cboDoctor.SelectedItem?.ToString() ?? "",
        Reason = txtReason.Text.Trim(),
        Status = cboStatus.SelectedItem?.ToString() ?? Statuses[0]
    };

    void LoadToForm(Appointment a)
    {
        _selected = a;
        errors.Clear();
        txtId.Text = a.Id;
        txtId.ReadOnly = true;            // mã là khóa, không đổi khi sửa
        txtName.Text = a.PatientName;
        txtPhone.Text = a.Phone;
        dtpTime.Value = a.AppointmentTime;
        cboDoctor.SelectedItem = a.Doctor;
        txtReason.Text = a.Reason;
        cboStatus.SelectedItem = a.Status;
        errors.Clear();
        UpdateButtons();
    }

    void ClearForm()
    {
        _selected = null;
        txtId.ReadOnly = false;
        txtId.Text = NextId();
        txtName.Clear();
        txtPhone.Clear();
        dtpTime.Value = TruncateToMinute(DateTime.Now.AddHours(1));
        cboDoctor.SelectedIndex = -1;
        cboStatus.SelectedIndex = 0;
        txtReason.Clear();
        errors.Clear();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        btnAdd.Enabled = _selected == null;
        btnUpdate.Enabled = _selected != null;
        btnDelete.Enabled = _selected != null;
    }

    string NextId()
    {
        int max = 0;
        foreach (var a in _all)
        {
            var m = Regex.Match(a.Id, @"^LH(\d+)$", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int n)) max = Math.Max(max, n);
        }
        return $"LH{max + 1:000}";
    }

    // =====================================================================
    //  KIỂM TRA DỮ LIỆU (ErrorProvider)
    // =====================================================================
    bool ValidateForm(bool isAdd)
    {
        errors.Clear();
        Control? first = null;

        void Err(Control c, string msg)
        {
            errors.SetError(c, msg);
            first ??= c;
        }

        // Mã lịch hẹn: bắt buộc + duy nhất
        string id = txtId.Text.Trim();
        if (id.Length == 0)
            Err(txtId, "Mã lịch hẹn không được để trống.");
        else if (_all.Any(a => !ReferenceEquals(a, _selected)
                               && string.Equals(a.Id, id, StringComparison.OrdinalIgnoreCase)))
            Err(txtId, "Mã lịch hẹn đã tồn tại.");

        // Họ tên
        if (txtName.Text.Trim().Length == 0)
            Err(txtName, "Vui lòng nhập họ tên bệnh nhân.");

        // Số điện thoại
        string phone = Regex.Replace(txtPhone.Text, @"[\s.\-()]", "");
        if (phone.Length == 0)
            Err(txtPhone, "Vui lòng nhập số điện thoại.");
        else if (!PhoneRegex.IsMatch(phone))
            Err(txtPhone, "Số điện thoại không hợp lệ (VD: 0912345678 hoặc +84912345678).");

        // Ngày giờ hẹn không ở quá khứ.
        // Khi SỬA, chỉ kiểm tra nếu người dùng đổi ngày giờ (để vẫn đổi được trạng thái
        // của lịch hôm nay đã qua giờ hẹn, vd: Đang khám -> Hoàn tất).
        var t = TruncateToMinute(dtpTime.Value);
        bool timeChanged = isAdd || _selected == null || t != TruncateToMinute(_selected.AppointmentTime);
        if (timeChanged && t < TruncateToMinute(DateTime.Now))
            Err(dtpTime, "Ngày giờ hẹn không được nằm trong quá khứ.");

        // Bác sĩ, trạng thái
        if (cboDoctor.SelectedIndex < 0) Err(cboDoctor, "Vui lòng chọn bác sĩ.");
        if (cboStatus.SelectedIndex < 0) Err(cboStatus, "Vui lòng chọn trạng thái.");

        // Lý do khám
        if (txtReason.Text.Trim().Length == 0)
            Err(txtReason, "Vui lòng nhập lý do khám.");

        first?.Focus();
        return first == null;
    }

    // =====================================================================
    //  XUẤT CSV
    // =====================================================================
    void ExportCsv()
    {
        using var dlg = new SaveFileDialog
        {
            Title = "Xuất danh sách lịch hẹn",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"lich-hen-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("Mã lịch hẹn,Họ tên bệnh nhân,Số điện thoại,Ngày giờ hẹn,Bác sĩ,Lý do khám,Trạng thái");
        foreach (var a in _view) // chỉ xuất các dòng đang hiển thị
        {
            sb.AppendLine(string.Join(",",
                Csv(a.Id), Csv(a.PatientName), Csv(a.Phone),
                Csv(a.AppointmentTime.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)),
                Csv(a.Doctor), Csv(a.Reason), Csv(a.Status)));
        }

        try
        {
            // UTF-8 có BOM để Excel hiển thị đúng tiếng Việt
            File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show(this, $"Đã xuất {_view.Count} lịch hẹn.", "Xuất CSV",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(this, "Không thể ghi file: " + ex.Message, "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    static string Csv(string s) =>
        s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

    // =====================================================================
    //  TIỆN ÍCH
    // =====================================================================
    static DateTime TruncateToMinute(DateTime d) =>
        new(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0);

    /// Bỏ khoảng trắng/dấu, đổi +84 -> 0 để lưu và tìm kiếm thống nhất.
    static string CanonPhone(string s)
    {
        string p = Regex.Replace(s, @"[\s.\-()]", "");
        return p.StartsWith("+84") ? "0" + p[3..] : p;
    }

    /// Bỏ dấu tiếng Việt + chữ thường, để gõ "nguyen" vẫn tìm ra "Nguyễn".
    static string Fold(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s.Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        return sb.ToString().Replace('đ', 'd').Replace('Đ', 'D').ToLowerInvariant();
    }

    void Seed()
    {
        var today = DateTime.Today;
        _all.AddRange(new[]
        {
            new Appointment { Id = "LH001", PatientName = "Nguyễn Văn Hùng", Phone = "0912345678",
                AppointmentTime = today.AddHours(9), Doctor = Doctors[0], Reason = "Đau dạ dày", Status = "Hoàn tất" },
            new Appointment { Id = "LH002", PatientName = "Trần Thị Mai", Phone = "0987654321",
                AppointmentTime = today.AddHours(10).AddMinutes(30), Doctor = Doctors[1], Reason = "Khám thai định kỳ", Status = "Đang khám" },
            new Appointment { Id = "LH003", PatientName = "Lê Quang Minh", Phone = "0345678901",
                AppointmentTime = today.AddHours(15), Doctor = Doctors[2], Reason = "Sốt cao, ho kéo dài", Status = "Chờ khám" },
            new Appointment { Id = "LH004", PatientName = "Phạm Thu Hà", Phone = "0778889990",
                AppointmentTime = today.AddDays(1).AddHours(8).AddMinutes(30), Doctor = Doctors[3], Reason = "Tái khám tim mạch", Status = "Chờ khám" },
            new Appointment { Id = "LH005", PatientName = "Đỗ Anh Tuấn", Phone = "0903123456",
                AppointmentTime = today.AddDays(1).AddHours(14), Doctor = Doctors[0], Reason = "Đau lưng", Status = "Đã hủy" },
        });
    }
}
