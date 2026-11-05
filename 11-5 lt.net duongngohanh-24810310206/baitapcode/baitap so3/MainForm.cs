using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RepairServiceTracker;

public class MainForm : Form
{
    // ---------- Dữ liệu ----------
    static readonly string[] DeviceTypes =
        { "Điện thoại", "Laptop", "Máy tính bảng", "Tivi", "Máy in", "Thiết bị gia dụng", "Khác" };

    static readonly string[] Statuses =
        { "Mới tiếp nhận", "Đang sửa", "Chờ linh kiện", "Hoàn tất", "Đã hủy" };

    // Đơn "chưa hoàn tất" = đang trong quy trình sửa; không tính đơn Hoàn tất và Đã hủy.
    // (Nếu muốn tính cả đơn Đã hủy, sửa điều kiện trong IsOpen.)
    static bool IsOpen(RepairOrder o) => o.Status != "Hoàn tất" && o.Status != "Đã hủy";

    // Số nguyên không âm, cho phép dấu phân cách hàng nghìn: 1500000 | 1.500.000 | 1,500,000 | 1 500 000
    static readonly Regex CostRegex = new(@"^-?(\d+|\d{1,3}([.,\s]\d{3})+)$");

    readonly List<RepairOrder> _all = new();               // toàn bộ đơn
    readonly BindingList<RepairOrder> _view = new();       // đơn sau khi lọc (nguồn của lưới)
    readonly BindingSource _bs = new();
    RepairOrder? _selected;                                // đơn đang chọn (null = thêm mới)
    bool _loading;                                         // chặn SelectionChanged khi nạp lại lưới
    string _imagePath = "";                                // ảnh đang hiển thị trên form

    // ---------- Controls ----------
    readonly TextBox txtId = new();
    readonly TextBox txtName = new();
    readonly TextBox txtPhone = new();
    readonly ComboBox cboDevice = new();
    readonly DateTimePicker dtpReceived = new();
    readonly ComboBox cboStatus = new();
    readonly TextBox txtCost = new();
    readonly TextBox txtDesc = new();

    readonly PictureBox pic = new();
    readonly Label lblImage = new();
    readonly Button btnPickImage = new() { Text = "Chọn ảnh..." };
    readonly Button btnClearImage = new() { Text = "Bỏ ảnh" };

    readonly Button btnAdd = new() { Text = "Thêm" };
    readonly Button btnUpdate = new() { Text = "Cập nhật" };
    readonly Button btnDelete = new() { Text = "Xóa" };
    readonly Button btnNew = new() { Text = "Làm mới" };

    readonly TextBox txtSearch = new();
    readonly ComboBox cboStatusFilter = new();
    readonly Button btnClearFilter = new() { Text = "Xóa lọc" };

    readonly DataGridView grid = new();
    readonly ErrorProvider errors = new();
    readonly ToolStripStatusLabel lblCount = new();
    readonly ToolStripStatusLabel lblCost = new();

    public MainForm()
    {
        Text = "Repair Service Tracker";
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1200, 760);
        MinimumSize = new Size(980, 660);

        errors.ContainerControl = this;
        errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

        BuildUi();
        Seed();

        _bs.DataSource = _view;
        grid.DataSource = _bs;

        WireEvents();
        ApplyFilter();
        ClearForm();

        FormClosed += (_, _) => { pic.Image?.Dispose(); pic.Image = null; };
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
        status.Items.Add(new ToolStripStatusLabel("|"));
        status.Items.Add(lblCost);

        // ----- Layout gốc: thông tin đơn / tìm kiếm-lọc / lưới -----
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

        root.Controls.Add(BuildInfoGroup(), 0, 0);
        root.Controls.Add(BuildFilterGroup(), 0, 1);
        BuildGrid();
        root.Controls.Add(grid, 0, 2);

        // Control Fill add trước, Menu/Status add sau để dock đúng
        Controls.Add(root);
        Controls.Add(status);
        Controls.Add(menu);
    }

    GroupBox BuildInfoGroup()
    {
        // --- Bảng nhập liệu (bên trái) ---
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

        cboDevice.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDevice.Items.AddRange(DeviceTypes);
        cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatus.Items.AddRange(Statuses);

        dtpReceived.Format = DateTimePickerFormat.Custom;
        dtpReceived.CustomFormat = "dd/MM/yyyy";

        txtCost.TextAlign = HorizontalAlignment.Right;

        txtDesc.Multiline = true;
        txtDesc.ScrollBars = ScrollBars.Vertical;
        txtDesc.MinimumSize = new Size(0, 64);

        AddField(t, "Mã đơn", txtId, 0, 0);
        AddField(t, "Tên khách hàng", txtName, 0, 2);
        AddField(t, "Số điện thoại", txtPhone, 1, 0);
        AddField(t, "Loại thiết bị", cboDevice, 1, 2);
        AddField(t, "Ngày tiếp nhận", dtpReceived, 2, 0);
        AddField(t, "Trạng thái", cboStatus, 2, 2);
        AddField(t, "Chi phí dự kiến (VNĐ)", txtCost, 3, 0);
        AddField(t, "Mô tả lỗi", txtDesc, 4, 0, span: 3);

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
        t.Controls.Add(buttons, 0, 5);
        t.SetColumnSpan(buttons, 4);

        // --- Khu vực ảnh (bên phải) ---
        var picGroup = new GroupBox
        {
            Text = "Ảnh thiết bị",
            Dock = DockStyle.Fill,
            MinimumSize = new Size(0, 230),
            Padding = new Padding(6)
        };
        var pt = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        pt.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        pt.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pt.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        pic.Dock = DockStyle.Fill;
        pic.SizeMode = PictureBoxSizeMode.Zoom;       // giữ đúng tỷ lệ, không méo
        pic.BorderStyle = BorderStyle.FixedSingle;
        pic.BackColor = Color.WhiteSmoke;

        lblImage.AutoSize = false;
        lblImage.Dock = DockStyle.Fill;
        lblImage.Height = 22;
        lblImage.TextAlign = ContentAlignment.MiddleCenter;
        lblImage.AutoEllipsis = true;
        lblImage.ForeColor = SystemColors.GrayText;

        var picButtons = new FlowLayoutPanel
        {
            AutoSize = true,
            Anchor = AnchorStyles.None,
            WrapContents = false
        };
        btnPickImage.AutoSize = true;
        btnClearImage.AutoSize = true;
        picButtons.Controls.Add(btnPickImage);
        picButtons.Controls.Add(btnClearImage);

        pt.Controls.Add(pic, 0, 0);
        pt.Controls.Add(lblImage, 0, 1);
        pt.Controls.Add(picButtons, 0, 2);
        picGroup.Controls.Add(pt);

        // --- Ghép hai bên ---
        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 1
        };
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        outer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 260));
        outer.Controls.Add(t, 0, 0);
        outer.Controls.Add(picGroup, 1, 0);

        var g = new GroupBox
        {
            Text = "Thông tin đơn",
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6)
        };
        g.Controls.Add(outer);
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
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        txtSearch.PlaceholderText = "Nhập mã đơn, tên khách hàng hoặc số điện thoại...";
        cboStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatusFilter.Items.Add("Tất cả");
        cboStatusFilter.Items.AddRange(Statuses);
        cboStatusFilter.SelectedIndex = 0;
        btnClearFilter.AutoSize = true;

        txtSearch.Dock = DockStyle.Fill;
        cboStatusFilter.Dock = DockStyle.Fill;

        t.Controls.Add(MakeLabel("Tìm kiếm"), 0, 0);
        t.Controls.Add(txtSearch, 1, 0);
        t.Controls.Add(MakeLabel("Trạng thái"), 2, 0);
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

        grid.Columns.Add(Col(nameof(RepairOrder.Id), "Mã đơn", 9));
        grid.Columns.Add(Col(nameof(RepairOrder.CustomerName), "Tên khách hàng", 16));
        grid.Columns.Add(Col(nameof(RepairOrder.Phone), "Số điện thoại", 12));
        grid.Columns.Add(Col(nameof(RepairOrder.DeviceType), "Loại thiết bị", 12));
        grid.Columns.Add(Col(nameof(RepairOrder.Description), "Mô tả lỗi", 22));

        var colDate = Col(nameof(RepairOrder.ReceivedDate), "Ngày tiếp nhận", 11);
        colDate.DefaultCellStyle.Format = "dd/MM/yyyy";
        grid.Columns.Add(colDate);

        var colCost = Col(nameof(RepairOrder.EstimatedCost), "Chi phí dự kiến", 12);
        colCost.DefaultCellStyle.Format = "N0";                       // định dạng chi phí
        colCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        grid.Columns.Add(colCost);

        grid.Columns.Add(Col(nameof(RepairOrder.Status), "Trạng thái", 12));
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
        var lb = MakeLabel(label);
        if (c is TextBox { Multiline: true }) lb.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        t.Controls.Add(lb, col, row);
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
        btnAdd.Click += (_, _) => AddOrder();
        btnUpdate.Click += (_, _) => UpdateOrder();
        btnDelete.Click += (_, _) => DeleteOrder();
        btnNew.Click += (_, _) => NewEntry();
        btnPickImage.Click += (_, _) => PickImage();
        btnClearImage.Click += (_, _) => SetImage("", userPicked: false);

        grid.SelectionChanged += (_, _) =>
        {
            if (_loading) return;
            if (grid.CurrentRow?.DataBoundItem is RepairOrder o) LoadToForm(o);
        };
        grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || grid.Rows[e.RowIndex].DataBoundItem is not RepairOrder o) return;
            grid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = o.Status switch
            {
                "Đã hủy" => Color.Gray,
                "Hoàn tất" => Color.SeaGreen,
                _ => grid.DefaultCellStyle.ForeColor
            };
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

        // Chi phí: định dạng lại "N0" khi rời ô nếu hợp lệ
        txtCost.Leave += (_, _) =>
        {
            if (TryParseCost(txtCost.Text, out decimal v) && v >= 0)
                txtCost.Text = v.ToString("N0");
        };

        // Xóa lỗi của từng trường khi người dùng sửa lại
        txtId.TextChanged += (_, _) => errors.SetError(txtId, "");
        txtName.TextChanged += (_, _) => errors.SetError(txtName, "");
        txtPhone.TextChanged += (_, _) => errors.SetError(txtPhone, "");
        cboDevice.SelectedIndexChanged += (_, _) => errors.SetError(cboDevice, "");
        txtCost.TextChanged += (_, _) => errors.SetError(txtCost, "");
        txtDesc.TextChanged += (_, _) => errors.SetError(txtDesc, "");
    }

    // =====================================================================
    //  LỌC / TÌM KIẾM
    // =====================================================================
    void ApplyFilter(string? keepId = null)
    {
        string? selId = keepId ?? _selected?.Id;
        RepairOrder? target = null;

        string q = txtSearch.Text.Trim();
        string qFold = Fold(q);
        string qDigits = Regex.Replace(q, @"\D", "");
        string? st = cboStatusFilter.SelectedIndex > 0 ? cboStatusFilter.SelectedItem?.ToString() : null;

        _loading = true;
        try
        {
            var result = _all
                .Where(o => q.Length == 0
                            || Fold(o.Id).Contains(qFold)
                            || Fold(o.CustomerName).Contains(qFold)
                            || (qDigits.Length > 0 && Regex.Replace(o.Phone, @"\D", "").Contains(qDigits)))
                .Where(o => st == null || o.Status == st)
                .OrderByDescending(o => o.ReceivedDate)
                .ThenBy(o => o.Id)
                .ToList();

            _view.RaiseListChangedEvents = false;
            _view.Clear();
            foreach (var o in result) _view.Add(o);
            _view.RaiseListChangedEvents = true;
            _view.ResetBindings();

            grid.ClearSelection();
            grid.CurrentCell = null;

            if (selId != null)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.DataBoundItem is RepairOrder o && o.Id == selId)
                    {
                        grid.CurrentCell = row.Cells[0];
                        row.Selected = true;
                        target = o;
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
            if (target == null) ClearForm();              // đơn đang chọn bị lọc mất
            else if (keepId != null) LoadToForm(target);  // vừa thêm/sửa -> nạp lại
        }

        // StatusStrip: số đơn theo bộ lọc + tổng chi phí dự kiến của đơn chưa hoàn tất
        decimal openTotal = _all.Where(IsOpen).Sum(o => o.EstimatedCost);
        lblCount.Text = $"Số đơn theo bộ lọc: {_view.Count} / {_all.Count}";
        lblCost.Text = $"Tổng chi phí dự kiến (đơn chưa hoàn tất): {openTotal:N0} VNĐ";
    }

    // =====================================================================
    //  THÊM / SỬA / XÓA
    // =====================================================================
    void AddOrder()
    {
        if (!ValidateForm()) return;
        if (!Confirm("Thêm đơn sửa chữa mới?")) return;

        var o = ReadForm();
        _all.Add(o);
        ApplyFilter(o.Id);

        if (!_view.Contains(o))
            MessageBox.Show(this, "Đã thêm đơn, nhưng đơn này đang bị ẩn bởi bộ lọc hiện tại.",
                "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    void UpdateOrder()
    {
        if (_selected == null) return;
        if (!ValidateForm()) return;
        if (!Confirm($"Cập nhật đơn {_selected.Id}?")) return;

        var f = ReadForm();
        _selected.CustomerName = f.CustomerName;
        _selected.Phone = f.Phone;
        _selected.DeviceType = f.DeviceType;
        _selected.Description = f.Description;
        _selected.ReceivedDate = f.ReceivedDate;
        _selected.EstimatedCost = f.EstimatedCost;
        _selected.Status = f.Status;
        _selected.ImagePath = f.ImagePath;

        ApplyFilter(_selected.Id);   // cập nhật lưới + số đơn + tổng chi phí
    }

    void DeleteOrder()
    {
        if (_selected == null) return;
        if (!Confirm($"Xóa đơn {_selected.Id} của khách hàng {_selected.CustomerName}?")) return;

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
    //  ẢNH THIẾT BỊ
    // =====================================================================
    void PickImage()
    {
        using var dlg = new OpenFileDialog
        {
            Title = "Chọn ảnh thiết bị",
            Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Tất cả các tệp (*.*)|*.*",
            CheckFileExists = true
        };
        if (dlg.ShowDialog(this) == DialogResult.OK) SetImage(dlg.FileName, userPicked: true);
    }

    /// Nạp ảnh vào PictureBox (Zoom). Đọc qua bản sao trong bộ nhớ để không khóa file gốc.
    void SetImage(string path, bool userPicked)
    {
        if (string.IsNullOrEmpty(path))
        {
            ReplaceImage(null);
            _imagePath = "";
            lblImage.Text = "Chưa có ảnh";
            return;
        }

        try
        {
            using var fs = File.OpenRead(path);
            using var img = Image.FromStream(fs);
            ReplaceImage(new Bitmap(img));
            _imagePath = path;
            lblImage.Text = Path.GetFileName(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                      or ArgumentException or OutOfMemoryException)
        {
            if (userPicked)
            {
                // File người dùng vừa chọn không phải ảnh hợp lệ: giữ nguyên ảnh cũ
                MessageBox.Show(this, "Không thể mở tệp này như một ảnh hợp lệ.", "Lỗi ảnh",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                // Ảnh đã lưu trong đơn nhưng tệp bị di chuyển/xóa: vẫn giữ đường dẫn
                ReplaceImage(null);
                _imagePath = path;
                lblImage.Text = "Không tìm thấy ảnh: " + Path.GetFileName(path);
            }
        }
    }

    void ReplaceImage(Image? img)
    {
        var old = pic.Image;
        pic.Image = img;
        old?.Dispose();
    }

    // =====================================================================
    //  FORM <-> DỮ LIỆU
    // =====================================================================
    RepairOrder ReadForm()
    {
        TryParseCost(txtCost.Text, out decimal cost);
        return new RepairOrder
        {
            Id = txtId.Text.Trim(),
            CustomerName = txtName.Text.Trim(),
            Phone = txtPhone.Text.Trim(),
            DeviceType = cboDevice.SelectedItem?.ToString() ?? "",
            Description = txtDesc.Text.Trim(),
            ReceivedDate = dtpReceived.Value.Date,
            EstimatedCost = cost,
            Status = cboStatus.SelectedItem?.ToString() ?? Statuses[0],
            ImagePath = _imagePath
        };
    }

    void LoadToForm(RepairOrder o)
    {
        _selected = o;
        txtId.Text = o.Id;
        txtId.ReadOnly = true;              // mã đơn là khóa, không đổi khi sửa
        txtName.Text = o.CustomerName;
        txtPhone.Text = o.Phone;
        cboDevice.SelectedItem = o.DeviceType;
        dtpReceived.Value = o.ReceivedDate;
        cboStatus.SelectedItem = o.Status;
        txtCost.Text = o.EstimatedCost.ToString("N0");
        txtDesc.Text = o.Description;
        SetImage(o.ImagePath, userPicked: false);
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
        cboDevice.SelectedIndex = -1;
        dtpReceived.Value = DateTime.Today;
        cboStatus.SelectedIndex = 0;
        txtCost.Text = "0";
        txtDesc.Clear();
        SetImage("", userPicked: false);
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
        foreach (var o in _all)
        {
            var m = Regex.Match(o.Id, @"^SC(\d+)$", RegexOptions.IgnoreCase);
            if (m.Success && int.TryParse(m.Groups[1].Value, out int n)) max = Math.Max(max, n);
        }
        return $"SC{max + 1:000}";
    }

    // =====================================================================
    //  KIỂM TRA DỮ LIỆU (ErrorProvider)
    // =====================================================================
    /// Chi phí VNĐ là số nguyên; chấp nhận dấu phân cách hàng nghìn (. , hoặc khoảng trắng).
    /// Trả về false nếu không đúng định dạng số (VD: "abc", "1.5").
    static bool TryParseCost(string text, out decimal value)
    {
        value = 0;
        string s = text.Trim();
        if (!CostRegex.IsMatch(s)) return false;
        return decimal.TryParse(Regex.Replace(s, @"[.,\s]", ""), NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture, out value);
    }

    bool ValidateForm()
    {
        errors.Clear();
        Control? first = null;

        void Err(Control c, string msg)
        {
            errors.SetError(c, msg);
            first ??= c;
        }

        // Mã đơn: bắt buộc + không trùng
        string id = txtId.Text.Trim();
        if (id.Length == 0)
            Err(txtId, "Mã đơn không được để trống.");
        else if (_all.Any(o => !ReferenceEquals(o, _selected)
                               && string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase)))
            Err(txtId, "Mã đơn đã tồn tại.");

        // Tên và số điện thoại không để trống
        if (txtName.Text.Trim().Length == 0)
            Err(txtName, "Tên khách hàng không được để trống.");
        if (txtPhone.Text.Trim().Length == 0)
            Err(txtPhone, "Số điện thoại không được để trống.");

        if (cboDevice.SelectedIndex < 0)
            Err(cboDevice, "Vui lòng chọn loại thiết bị.");

        // Chi phí: số không âm
        if (txtCost.Text.Trim().Length == 0)
            Err(txtCost, "Vui lòng nhập chi phí dự kiến (nhập 0 nếu chưa có).");
        else if (!TryParseCost(txtCost.Text, out decimal cost))
            Err(txtCost, "Chi phí phải là số nguyên (VD: 1500000 hoặc 1.500.000).");
        else if (cost < 0)
            Err(txtCost, "Chi phí dự kiến không được là số âm.");

        // Mô tả lỗi bắt buộc
        if (txtDesc.Text.Trim().Length == 0)
            Err(txtDesc, "Vui lòng nhập mô tả lỗi.");

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
            Title = "Xuất danh sách đơn sửa chữa",
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = "csv",
            FileName = $"don-sua-chua-{DateTime.Now:yyyyMMdd}.csv"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("Mã đơn,Tên khách hàng,Số điện thoại,Loại thiết bị,Mô tả lỗi,Ngày tiếp nhận,Chi phí dự kiến,Trạng thái");
        foreach (var o in _view) // chỉ xuất các đơn đang hiển thị (sau lọc)
        {
            sb.AppendLine(string.Join(",",
                Csv(o.Id), Csv(o.CustomerName), Csv(o.Phone), Csv(o.DeviceType), Csv(o.Description),
                Csv(o.ReceivedDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                o.EstimatedCost.ToString("0", CultureInfo.InvariantCulture),
                Csv(o.Status)));
        }

        try
        {
            // UTF-8 có BOM để Excel hiển thị đúng tiếng Việt
            File.WriteAllText(dlg.FileName, sb.ToString(), new UTF8Encoding(true));
            MessageBox.Show(this, $"Đã xuất {_view.Count} đơn.", "Xuất CSV",
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
        var d = DateTime.Today;
        _all.AddRange(new[]
        {
            new RepairOrder { Id = "SC001", CustomerName = "Nguyễn Văn Hùng", Phone = "0912345678",
                DeviceType = "Điện thoại", Description = "Vỡ màn hình, cần thay màn mới",
                ReceivedDate = d.AddDays(-3), EstimatedCost = 1_500_000, Status = "Đang sửa" },
            new RepairOrder { Id = "SC002", CustomerName = "Trần Thị Mai", Phone = "0987654321",
                DeviceType = "Laptop", Description = "Không lên nguồn, nghi hỏng mainboard",
                ReceivedDate = d.AddDays(-5), EstimatedCost = 2_800_000, Status = "Chờ linh kiện" },
            new RepairOrder { Id = "SC003", CustomerName = "Lê Quang Minh", Phone = "0345678901",
                DeviceType = "Máy in", Description = "Kẹt giấy liên tục",
                ReceivedDate = d.AddDays(-10), EstimatedCost = 350_000, Status = "Hoàn tất" },
            new RepairOrder { Id = "SC004", CustomerName = "Phạm Thu Hà", Phone = "0778889990",
                DeviceType = "Tivi", Description = "Có tiếng nhưng không có hình",
                ReceivedDate = d.AddDays(-1), EstimatedCost = 900_000, Status = "Mới tiếp nhận" },
            new RepairOrder { Id = "SC005", CustomerName = "Đỗ Anh Tuấn", Phone = "0903123456",
                DeviceType = "Máy tính bảng", Description = "Pin chai, sạc không vào",
                ReceivedDate = d.AddDays(-7), EstimatedCost = 600_000, Status = "Đã hủy" },
        });
    }
}
