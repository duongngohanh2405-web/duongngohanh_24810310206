using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LabEquipmentCheckout;

public class MainForm : Form
{
    // ---------- Dữ liệu ----------
    static readonly string[] EquipmentList =
    {
        "Oscilloscope", "Đồng hồ vạn năng", "Máy hàn thiếc", "Kit Arduino Uno",
        "Raspberry Pi 4", "Nguồn một chiều (DC)", "Máy phát xung"
    };

    static readonly string[] StatusFilters = { "Tất cả", "Đang mượn", "Quá hạn", "Đã trả" };

    readonly List<Loan> _all = new();                 // toàn bộ phiếu
    readonly BindingList<Loan> _view = new();         // phiếu sau khi lọc (nguồn của lưới)
    readonly BindingSource _bs = new();
    Loan? _selected;                                  // phiếu đang chọn (null = thêm mới)
    bool _loading;                                    // chặn SelectionChanged khi nạp lại lưới
    DateTime _today = DateTime.Today;                 // để phát hiện qua ngày mới

    // ---------- Controls ----------
    readonly TextBox txtId = new();
    readonly TextBox txtStudentId = new();
    readonly TextBox txtStudentName = new();
    readonly ComboBox cboEquipment = new();
    readonly NumericUpDown nudQty = new();
    readonly TextBox txtStatus = new();
    readonly DateTimePicker dtpBorrow = new();
    readonly DateTimePicker dtpDue = new();
    readonly DateTimePicker dtpReturn = new();

    readonly Button btnAdd = new() { Text = "Thêm" };
    readonly Button btnUpdate = new() { Text = "Cập nhật" };
    readonly Button btnDelete = new() { Text = "Xóa" };
    readonly Button btnReturned = new() { Text = "Đánh dấu đã trả" };
    readonly Button btnNew = new() { Text = "Làm mới" };

    readonly TextBox txtSearch = new();
    readonly ComboBox cboStatusFilter = new();
    readonly Button btnClearFilter = new() { Text = "Xóa lọc" };

    readonly DataGridView grid = new();
    readonly ErrorProvider errors = new();
    readonly ToolStripStatusLabel lblTotal = new();
    readonly ToolStripStatusLabel lblOverdue = new();
    readonly System.Windows.Forms.Timer _timer = new() { Interval = 60_000 };

    public MainForm()
    {
        Text = "Lab Equipment Checkout";
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1150, 720);
        MinimumSize = new Size(950, 620);

        errors.ContainerControl = this;
        errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        BuildUi();
        Seed();

        _bs.DataSource = _view;
        grid.DataSource = _bs;

        WireEvents();
        ApplyFilter();
        ClearForm();

        // Qua nửa đêm thì tình trạng "Quá hạn" có thể thay đổi -> tính lại
        _timer.Tick += (_, _) =>
        {
            if (_today == DateTime.Today) return;
            _today = DateTime.Today;
            ApplyFilter();
            UpdateStatusPreview();
        };
        _timer.Start();
        FormClosed += (_, _) => _timer.Dispose();
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
        lblOverdue.ForeColor = Color.DarkRed;
        status.Items.Add(lblTotal);
        status.Items.Add(new ToolStripStatusLabel("|"));
        status.Items.Add(lblOverdue);

        // ----- Layout gốc: nhập phiếu / tìm kiếm-lọc / lưới -----
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

        // Control Fill add trước, Menu/Status add sau để dock đúng
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

        cboEquipment.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEquipment.Items.AddRange(EquipmentList);

        nudQty.Minimum = 0;          // cho phép nhập 0 để chương trình báo lỗi "phải > 0"
        nudQty.Maximum = 1000;
        nudQty.Value = 1;

        txtStatus.ReadOnly = true;   // tình trạng do chương trình tự tính
        txtStatus.TabStop = false;

        foreach (var d in new[] { dtpBorrow, dtpDue, dtpReturn })
        {
            d.Format = DateTimePickerFormat.Custom;
            d.CustomFormat = "dd/MM/yyyy";
        }
        dtpReturn.ShowCheckBox = true;   // tick = đã trả; bỏ tick = chưa trả
        dtpReturn.Checked = false;

        AddField(t, "Mã phiếu", txtId, 0, 0);
        AddField(t, "Mã sinh viên", txtStudentId, 0, 2);
        AddField(t, "Họ tên sinh viên", txtStudentName, 1, 0);
        AddField(t, "Thiết bị", cboEquipment, 1, 2);
        AddField(t, "Số lượng", nudQty, 2, 0);
        AddField(t, "Tình trạng", txtStatus, 2, 2);
        AddField(t, "Ngày mượn", dtpBorrow, 3, 0);
        AddField(t, "Hạn trả", dtpDue, 3, 2);
        AddField(t, "Ngày trả thực tế", dtpReturn, 4, 0);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        foreach (var b in new[] { btnAdd, btnUpdate, btnDelete, btnReturned, btnNew })
        {
            b.AutoSize = true;
            b.MinimumSize = new Size(90, 30);
            buttons.Controls.Add(b);
        }
        t.Controls.Add(buttons, 0, 5);
        t.SetColumnSpan(buttons, 4);

        var g = new GroupBox
        {
            Text = "Phiếu mượn",
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
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(6)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        txtSearch.PlaceholderText = "Nhập mã sinh viên hoặc tên thiết bị...";
        cboStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatusFilter.Items.AddRange(StatusFilters);
        cboStatusFilter.SelectedIndex = 0;
        btnClearFilter.AutoSize = true;

        txtSearch.Dock = DockStyle.Fill;
        cboStatusFilter.Dock = DockStyle.Fill;

        t.Controls.Add(MakeLabel("Tìm kiếm"), 0, 0);
        t.Controls.Add(txtSearch, 1, 0);
        t.Controls.Add(MakeLabel("Tình trạng"), 2, 0);
        t.Controls.Add(cboStatusFilter, 3, 0);
        t.Controls.Add(btnClearFilter, 4, 0);

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

        grid.Columns.Add(Col(nameof(Loan.Id), "Mã phiếu", 9));
        grid.Columns.Add(Col(nameof(Loan.StudentId), "Mã SV", 10));
        grid.Columns.Add(Col(nameof(Loan.StudentName), "Họ tên sinh viên", 17));
        grid.Columns.Add(Col(nameof(Loan.Equipment), "Thiết bị", 18));
        grid.Columns.Add(Col(nameof(Loan.Quantity), "SL", 5));
        grid.Columns.Add(DateCol(nameof(Loan.BorrowDate), "Ngày mượn"));
        grid.Columns.Add(DateCol(nameof(Loan.DueDate), "Hạn trả"));
        grid.Columns.Add(DateCol(nameof(Loan.ReturnDate), "Ngày trả thực tế"));
        grid.Columns.Add(Col(nameof(Loan.Status), "Tình trạng", 10));
    }

    static DataGridViewTextBoxColumn Col(string prop, string header, float weight) => new()
    {
        DataPropertyName = prop,
        Name = "col" + prop,
        HeaderText = header,
        FillWeight = weight
    };

    static DataGridViewTextBoxColumn DateCol(string prop, string header)
    {
        var c = Col(prop, header, 11);
        c.DefaultCellStyle.Format = "dd/MM/yyyy";
        c.DefaultCellStyle.NullValue = "";   // ngày trả trống khi chưa trả
        return c;
    }

    static Label MakeLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Anchor = AnchorStyles.Left,
        Margin = new Padding(3, 6, 8, 6)
    };

    static void AddField(TableLayoutPanel t, string label, Control c, int row, int col)
    {
        t.Controls.Add(MakeLabel(label), col, row);
        c.Dock = DockStyle.Fill;
        c.Margin = new Padding(3, 3, 24, 3); // chừa chỗ cho biểu tượng ErrorProvider
        t.Controls.Add(c, col + 1, row);
    }

    // =====================================================================
    //  SỰ KIỆN
    // =====================================================================
    void WireEvents()
    {
        btnAdd.Click += (_, _) => AddLoan();
        btnUpdate.Click += (_, _) => UpdateLoan();
        btnDelete.Click += (_, _) => DeleteLoan();
        btnReturned.Click += (_, _) => MarkReturned();
        btnNew.Click += (_, _) => NewEntry();

        grid.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            if (grid.CurrentRow?.DataBoundItem is Loan l) LoadToForm(l);
        };
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || grid.Rows[e.RowIndex].DataBoundItem is not Loan l) return;
            var style = grid.Rows[e.RowIndex].DefaultCellStyle;
            switch (l.Status)
            {
                case "Quá hạn":
                    style.ForeColor = Color.DarkRed;
                    style.BackColor = Color.MistyRose;
                    break;
                case "Đã trả":
                    style.ForeColor = Color.Gray;
                    style.BackColor = grid.DefaultCellStyle.BackColor;
                    break;
                default:
                    style.ForeColor = grid.DefaultCellStyle.ForeColor;
                    style.BackColor = grid.DefaultCellStyle.BackColor;
                    break;
            }
        };

        // Tìm kiếm / lọc tức thời
        txtSearch.TextChanged += (_, _) => ApplyFilter();
        cboStatusFilter.SelectedIndexChanged += (_, _) => ApplyFilter();
        btnClearFilter.Click += (_, _) =>
        {
            txtSearch.Clear();
            cboStatusFilter.SelectedIndex = 0;
            ApplyFilter();
        };

        // Xóa lỗi của từng trường khi người dùng sửa lại
        txtId.TextChanged += (_, _) => errors.SetError(txtId, "");
        txtStudentId.TextChanged += (_, _) => errors.SetError(txtStudentId, "");
        cboEquipment.SelectedIndexChanged += (_, _) => errors.SetError(cboEquipment, "");
        nudQty.ValueChanged += (_, _) => errors.SetError(nudQty, "");
        dtpBorrow.ValueChanged += (_, _) =>
        {
            errors.SetError(dtpBorrow, ""); errors.SetError(dtpDue, ""); errors.SetError(dtpReturn, "");
        };
        dtpDue.ValueChanged += (_, _) => { errors.SetError(dtpDue, ""); UpdateStatusPreview(); };
        dtpReturn.ValueChanged += (_, _) => { errors.SetError(dtpReturn, ""); UpdateStatusPreview(); };
    }

    // =====================================================================
    //  LỌC / TÌM KIẾM
    // =====================================================================
    void ApplyFilter(string? keepId = null)
    {
        string? selId = keepId ?? _selected?.Id;
        Loan? target = null;

        string q = txtSearch.Text.Trim();
        string qFold = Fold(q);
        string? st = cboStatusFilter.SelectedIndex > 0 ? cboStatusFilter.SelectedItem?.ToString() : null;

        _loading = true;
        try
        {
            var result = _all
                .Where(l => q.Length == 0
                            || Fold(l.StudentId).Contains(qFold)
                            || Fold(l.Equipment).Contains(qFold)
                            || Fold(l.StudentName).Contains(qFold))
                .Where(l => st == null || l.Status == st)
                .OrderBy(l => l.DueDate)
                .ThenBy(l => l.Id)
                .ToList();

            _view.RaiseListChangedEvents = false;
            _view.Clear();
            foreach (var l in result) _view.Add(l);
            _view.RaiseListChangedEvents = true;
            _view.ResetBindings();

            grid.ClearSelection();
            grid.CurrentCell = null;

            if (selId != null)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.DataBoundItem is Loan l && l.Id == selId)
                    {
                        grid.CurrentCell = row.Cells[0];
                        row.Selected = true;
                        target = l;
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
            if (target == null) ClearForm();              // phiếu đang chọn bị lọc mất
            else if (keepId != null) LoadToForm(target);  // vừa thêm/sửa -> nạp lại
            else UpdateButtons();                         // tình trạng có thể đã đổi
        }

        int overdue = _all.Count(l => l.Status == "Quá hạn");
        lblTotal.Text = $"Tổng số phiếu: {_all.Count} (đang hiển thị {_view.Count})";
        lblOverdue.Text = $"Quá hạn: {overdue}";
    }

    // =====================================================================
    //  THÊM / SỬA / XÓA / TRẢ
    // =====================================================================
    void AddLoan()
    {
        if (!ValidateForm()) return;
        if (!Confirm("Thêm phiếu mượn mới?")) return;

        var l = ReadForm();
        _all.Add(l);
        ApplyFilter(l.Id);

        if (!_view.Contains(l))
            MessageBox.Show(this, "Đã thêm phiếu, nhưng phiếu này đang bị ẩn bởi bộ lọc hiện tại.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void UpdateLoan()
    {
        if (_selected == null) return;
        if (!ValidateForm()) return;
        if (!Confirm($"Cập nhật phiếu {_selected.Id}?")) return;

        var f = ReadForm();
        _selected.StudentId = f.StudentId;
        _selected.StudentName = f.StudentName;
        _selected.Equipment = f.Equipment;
        _selected.Quantity = f.Quantity;
        _selected.BorrowDate = f.BorrowDate;
        _selected.DueDate = f.DueDate;
        _selected.ReturnDate = f.ReturnDate;

        ApplyFilter(_selected.Id);
    }

    void DeleteLoan()
    {
        if (_selected == null) return;
        if (!Confirm($"Xóa phiếu {_selected.Id} của sinh viên {_selected.StudentId}?")) return;

        _all.Remove(_selected);
        ClearForm();
        ApplyFilter();
    }

    /// Đánh dấu đã trả: ghi ngày trả thực tế = hôm nay, tình trạng tự chuyển thành "Đã trả".
    void MarkReturned()
    {
        if (_selected == null || _selected.ReturnDate != null) return;

        DateTime today = DateTime.Today;
        if (today < _selected.BorrowDate.Date)
        {
            MessageBox.Show(this, "Ngày mượn nằm sau hôm nay nên không thể ghi nhận trả.",
                "Không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!Confirm($"Xác nhận phiếu {_selected.Id} đã được trả vào hôm nay ({today:dd/MM/yyyy})?")) return;

        _selected.ReturnDate = today;
        ApplyFilter(_selected.Id);
    }

    void NewEntry()
    {
        ClearForm();
        _loading = true;
        grid.ClearSelection();
        grid.CurrentCell = null;
        _loading = false;
        txtStudentId.Focus();
    }

    bool Confirm(string message) =>
        MessageBox.Show(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        == DialogResult.Yes;

    // =====================================================================
    //  FORM <-> DỮ LIỆU
    // =====================================================================
    Loan ReadForm() => new()
    {
        Id = txtId.Text.Trim(),
        StudentId = txtStudentId.Text.Trim(),
        StudentName = txtStudentName.Text.Trim(),
        Equipment = cboEquipment.SelectedItem?.ToString() ?? "",
        Quantity = (int)nudQty.Value,
        BorrowDate = dtpBorrow.Value.Date,
        DueDate = dtpDue.Value.Date,
        ReturnDate = dtpReturn.Checked ? dtpReturn.Value.Date : null
    };

    void LoadToForm(Loan l)
    {
        _selected = l;
        txtId.Text = l.Id;
        txtId.ReadOnly = true;              // mã phiếu là khóa, không đổi khi sửa
        txtStudentId.Text = l.StudentId;
        txtStudentName.Text = l.StudentName;
        cboEquipment.SelectedItem = l.Equipment;
        nudQty.Value = Math.Clamp(l.Quantity, (int)nudQty.Minimum, (int)nudQty.Maximum);
        dtpBorrow.Value = l.BorrowDate;
        dtpDue.Value = l.DueDate;
        if (l.ReturnDate.HasValue)
        {
            dtpReturn.Value = l.ReturnDate.Value;
            dtpReturn.Checked = true;
        }
        else
        {
            dtpReturn.Value = DateTime.Today;
            dtpReturn.Checked = false;
        }
        errors.Clear();
        UpdateStatusPreview();
        UpdateButtons();
    }

    void ClearForm()
    {
        _selected = null;
        txtId.ReadOnly = false;
        txtId.Text = NextId();
        txtStudentId.Clear();
        txtStudentName.Clear();
        cboEquipment.SelectedIndex = -1;
        nudQty.Value = 1;
        dtpBorrow.Value = DateTime.Today;
        dtpDue.Value = DateTime.Today.AddDays(7);
        dtpReturn.Value = DateTime.Today;
        dtpReturn.Checked = false;
        errors.Clear();
        UpdateStatusPreview();
        UpdateButtons();
    }

    void UpdateButtons()
    {
        btnAdd.Enabled = _selected == null;
        btnUpdate.Enabled = _selected != null;
        btnDelete.Enabled = _selected != null;
        btnReturned.Enabled = _selected != null && _selected.ReturnDate == null;
    }

    /// Hiển thị tình trạng theo giá trị đang nhập trên form.
    void UpdateStatusPreview()
    {
        string s = Loan.StatusOf(dtpDue.Value, dtpReturn.Checked ? dtpReturn.Value : null);
        txtStatus.Text = s;
        txtStatus.ForeColor = s == "Quá hạn" ? Color.DarkRed : SystemColors.WindowText;
    }

    string NextId()
    {
        int max = 0;
        foreach (var l in _all)
        {
            var m = Regex.Match(l.Id, @"^PM(\d+)$", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int n)) max = Math.Max(max, n);
        }
        return $"PM{max + 1:000}";
    }

    // =====================================================================
    //  KIỂM TRA DỮ LIỆU (ErrorProvider)
    // =====================================================================
    bool ValidateForm()
    {
        errors.Clear();
        Control? first = null;

        void Err(Control c, string msg)
        {
            errors.SetError(c, msg);
            first ??= c;
        }

        // Mã phiếu: bắt buộc + không trùng
        string id = txtId.Text.Trim();
        if (id.Length == 0)
            Err(txtId, "Mã phiếu không được để trống.");
        else if (_all.Any(l => !ReferenceEquals(l, _selected)
                               && string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase)))
            Err(txtId, "Mã phiếu đã tồn tại.");

        // Mã sinh viên, thiết bị
        if (txtStudentId.Text.Trim().Length == 0)
            Err(txtStudentId, "Mã sinh viên không được để trống.");
        if (cboEquipment.SelectedIndex < 0)
            Err(cboEquipment, "Vui lòng chọn thiết bị.");

        // Số lượng > 0
        if (nudQty.Value <= 0)
            Err(nudQty, "Số lượng phải lớn hơn 0.");

        // Hạn trả và ngày trả không trước ngày mượn
        DateTime borrow = dtpBorrow.Value.Date;
        if (dtpDue.Value.Date < borrow)
            Err(dtpDue, "Hạn trả không được trước ngày mượn.");
        if (dtpReturn.Checked && dtpReturn.Value.Date < borrow)
            Err(dtpReturn, "Ngày trả thực tế không được trước ngày mượn.");

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
            Title = "Xuất danh sách phiếu mượn",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"phieu-muon-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("Mã phiếu,Mã sinh viên,Họ tên sinh viên,Thiết bị,Số lượng,Ngày mượn,Hạn trả,Ngày trả thực tế,Tình trạng");
        foreach (var l in _view) // chỉ xuất các phiếu đang hiển thị (sau lọc)
        {
            sb.AppendLine(string.Join(",",
                Csv(l.Id), Csv(l.StudentId), Csv(l.StudentName), Csv(l.Equipment),
                l.Quantity.ToString(CultureInfo.InvariantCulture),
                Csv(l.BorrowDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                Csv(l.DueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                Csv(l.ReturnDate?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? ""),
                Csv(l.Status)));
        }

        try
        {
            // UTF-8 có BOM để Excel hiển thị đúng tiếng Việt
            File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show(this, $"Đã xuất {_view.Count} phiếu.", "Xuất CSV",
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
    /// Bỏ dấu tiếng Việt + chữ thường, để gõ "may han" vẫn tìm ra "Máy hàn thiếc".
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
        var d = DateTime.Today;
        _all.AddRange(new[]
        {
            new Loan { Id = "PM001", StudentId = "SV001", StudentName = "Nguyễn Văn An", Equipment = "Oscilloscope",
                Quantity = 1, BorrowDate = d.AddDays(-10), DueDate = d.AddDays(-3) },                       // Quá hạn
            new Loan { Id = "PM002", StudentId = "SV002", StudentName = "Trần Thị Bình", Equipment = "Kit Arduino Uno",
                Quantity = 2, BorrowDate = d.AddDays(-2), DueDate = d.AddDays(5) },                          // Đang mượn
            new Loan { Id = "PM003", StudentId = "SV003", StudentName = "Lê Minh Cường", Equipment = "Đồng hồ vạn năng",
                Quantity = 1, BorrowDate = d.AddDays(-7), DueDate = d.AddDays(-1), ReturnDate = d.AddDays(-1) }, // Đã trả
            new Loan { Id = "PM004", StudentId = "SV004", StudentName = "Phạm Thu Dung", Equipment = "Máy hàn thiếc",
                Quantity = 1, BorrowDate = d.AddDays(-5), DueDate = d },                                     // Hạn là hôm nay -> chưa quá hạn
            new Loan { Id = "PM005", StudentId = "SV005", StudentName = "Hoàng Gia Huy", Equipment = "Raspberry Pi 4",
                Quantity = 3, BorrowDate = d.AddDays(-14), DueDate = d.AddDays(-7) },                        // Quá hạn
        });
    }
}
