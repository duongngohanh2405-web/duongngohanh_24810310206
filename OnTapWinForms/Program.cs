using System.ComponentModel;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;

namespace OnTapWinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    public MainForm()
    {
        Text = "Ôn tập Windows Forms - C#";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(950, 650);
        Size = new Size(1180, 780);
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.White;

        var tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 7) };
        tabs.TabPages.Add(BuildCalculatorTab());
        tabs.TabPages.Add(BuildIncidentTab());
        tabs.TabPages.Add(BuildInventoryTab());
        tabs.TabPages.Add(BuildSeatTab());
        tabs.TabPages.Add(BuildDeliveryTab());
        Controls.Add(tabs);
    }

    private static TabPage NewPage(string title)
    {
        return new TabPage(title) { BackColor = Color.White, Padding = new Padding(12) };
    }

    private static Label MakeLabel(string text, int width = 150)
    {
        return new Label
        {
            Text = text,
            Width = width,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(5, 5, 8, 5)
        };
    }

    private static TextBox MakeTextBox(string placeholder = "")
    {
        return new TextBox { Width = 240, PlaceholderText = placeholder, Margin = new Padding(4, 6, 10, 6) };
    }

    private static Button MakeButton(string text, EventHandler handler, int width = 130)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 34,
            Margin = new Padding(5),
            FlatStyle = FlatStyle.System
        };
        button.Click += handler;
        return button;
    }

    private static FlowLayoutPanel MakeRow()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(2),
            Margin = new Padding(0)
        };
    }

    private static DataGridView MakeGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D
        };
    }

    private TabPage BuildCalculatorTab()
    {
        var page = NewPage("1. Tính cước & giảm giá");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(12)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.Controls.Add(MakeLabel("Đơn giá dịch vụ (đ):", 195), 0, 0);
        var txtPrice = MakeTextBox("Ví dụ: 100000");
        txtPrice.Dock = DockStyle.Fill;
        layout.Controls.Add(txtPrice, 1, 0);
        layout.Controls.Add(MakeLabel("Số lượng:", 195), 0, 1);
        var txtQuantity = MakeTextBox("Nhập số lượng");
        txtQuantity.Dock = DockStyle.Fill;
        layout.Controls.Add(txtQuantity, 1, 1);
        layout.Controls.Add(MakeLabel("Giảm giá (%):", 195), 0, 2);
        var txtDiscount = MakeTextBox("Từ 0 đến 100");
        txtDiscount.Dock = DockStyle.Fill;
        txtDiscount.Text = "0";
        layout.Controls.Add(txtDiscount, 1, 2);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10) };
        var result = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            Font = new Font(Font.FontFamily, 12F),
            Text = "Nhập thông tin, sau đó nhấn Tính tiền.",
            AutoSize = false
        };

        actions.Controls.Add(MakeButton("Tính tiền", (_, _) =>
        {
            if (!decimal.TryParse(txtPrice.Text, NumberStyles.Number, ViCulture, out var price) || price < 0)
            {
                MessageBox.Show("Đơn giá phải là số không âm.", "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out var quantity) || quantity <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0.", "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return;
            }

            if (!decimal.TryParse(txtDiscount.Text, NumberStyles.Number, ViCulture, out var discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Phần trăm giảm giá phải nằm trong khoảng 0 đến 100.", "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDiscount.Focus();
                return;
            }

            var subtotal = price * quantity;
            var discountAmount = subtotal * discount / 100m;
            var total = subtotal - discountAmount;
            result.Text = $"Tiền trước giảm giá: {subtotal.ToString("N0", ViCulture)} đ\r\n" +
                          $"Số tiền giảm: {discountAmount.ToString("N0", ViCulture)} đ\r\n" +
                          $"TỔNG THANH TOÁN: {total.ToString("N0", ViCulture)} đ";
        }));
        actions.Controls.Add(MakeButton("Làm mới", (_, _) =>
        {
            txtPrice.Clear();
            txtQuantity.Clear();
            txtDiscount.Text = "0";
            result.Text = "Nhập thông tin, sau đó nhấn Tính tiền.";
            txtPrice.Focus();
        }));
        page.Controls.Add(result);
        page.Controls.Add(actions);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage BuildIncidentTab()
    {
        var page = NewPage("2. Báo cáo sự cố IT");
        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12)
        };

        var txtCode = MakeTextBox("Để trống để tự tạo mã");
        var txtRequester = MakeTextBox("Họ tên người báo cáo");
        var date = new DateTimePicker { Width = 240, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
        var cboType = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        cboType.Items.AddRange(new object[] { "Máy tính không khởi động", "Lỗi phần mềm", "Mạng Internet", "Máy in", "Tài khoản / mật khẩu", "Khác" });
        cboType.SelectedIndex = 0;

        var priorityPanel = new FlowLayoutPanel { Width = 520, Height = 36, WrapContents = false };
        var rbLow = new RadioButton { Text = "Thấp", AutoSize = true, Margin = new Padding(4, 7, 18, 4) };
        var rbMedium = new RadioButton { Text = "Trung bình", AutoSize = true, Checked = true, Margin = new Padding(4, 7, 18, 4) };
        var rbHigh = new RadioButton { Text = "Cao", AutoSize = true, Margin = new Padding(4, 7, 18, 4) };
        priorityPanel.Controls.AddRange(new Control[] { rbLow, rbMedium, rbHigh });

        var devicePanel = new FlowLayoutPanel { Width = 600, Height = 36, WrapContents = false };
        var cbLaptop = new CheckBox { Text = "Laptop / PC", AutoSize = true, Margin = new Padding(4, 7, 18, 4) };
        var cbNetwork = new CheckBox { Text = "Mạng", AutoSize = true, Margin = new Padding(4, 7, 18, 4) };
        var cbPrinter = new CheckBox { Text = "Máy in", AutoSize = true, Margin = new Padding(4, 7, 18, 4) };
        var cbAccount = new CheckBox { Text = "Tài khoản", AutoSize = true, Margin = new Padding(4, 7, 18, 4) };
        devicePanel.Controls.AddRange(new Control[] { cbLaptop, cbNetwork, cbPrinter, cbAccount });

        var attachment = new Label { Text = "Chưa chọn tệp đính kèm", Width = 360, Height = 32, TextAlign = ContentAlignment.MiddleLeft };
        var txtDescription = new TextBox { Width = 520, Height = 78, Multiline = true, ScrollBars = ScrollBars.Vertical, PlaceholderText = "Mô tả tình trạng và cách tái hiện lỗi..." };
        string attachmentPath = "";

        void AddIncidentRow(string caption, Control control)
        {
            var row = MakeRow();
            row.Controls.Add(MakeLabel(caption, 165));
            row.Controls.Add(control);
            content.Controls.Add(row);
        }

        AddIncidentRow("Mã sự cố:", txtCode);
        AddIncidentRow("Người báo cáo:", txtRequester);
        AddIncidentRow("Ngày báo cáo:", date);
        AddIncidentRow("Loại sự cố:", cboType);
        AddIncidentRow("Mức độ ưu tiên:", priorityPanel);
        AddIncidentRow("Thiết bị liên quan:", devicePanel);
        AddIncidentRow("Mô tả:", txtDescription);

        var fileRow = MakeRow();
        fileRow.Controls.Add(MakeLabel("Ảnh / tệp minh họa:", 165));
        fileRow.Controls.Add(MakeButton("Chọn tệp", (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh hoặc tệp",
                Filter = "Tệp hình ảnh và tài liệu|*.png;*.jpg;*.jpeg;*.bmp;*.pdf;*.txt|Tất cả tệp|*.*"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                attachmentPath = dialog.FileName;
                attachment.Text = Path.GetFileName(attachmentPath);
            }
        }, 110));
        fileRow.Controls.Add(attachment);
        content.Controls.Add(fileRow);

        var buttonRow = MakeRow();
        buttonRow.Controls.Add(MakeButton("Tạo báo cáo", (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(txtRequester.Text))
            {
                MessageBox.Show("Vui lòng nhập người báo cáo.");
                txtRequester.Focus();
                return;
            }

            var code = string.IsNullOrWhiteSpace(txtCode.Text) ? $"INC-{DateTime.Now:yyyyMMddHHmmss}" : txtCode.Text.Trim();
            var priority = rbHigh.Checked ? "Cao" : rbLow.Checked ? "Thấp" : "Trung bình";
            var devices = new List<string>();
            if (cbLaptop.Checked) devices.Add("Laptop / PC");
            if (cbNetwork.Checked) devices.Add("Mạng");
            if (cbPrinter.Checked) devices.Add("Máy in");
            if (cbAccount.Checked) devices.Add("Tài khoản");
            if (devices.Count == 0) devices.Add("Chưa xác định");
            var summary = $"MÃ SỰ CỐ: {code}\r\nNgười báo cáo: {txtRequester.Text.Trim()}\r\n" +
                          $"Thời gian: {date.Value:dd/MM/yyyy HH:mm}\r\nLoại sự cố: {cboType.Text}\r\n" +
                          $"Ưu tiên: {priority}\r\nThiết bị: {string.Join(", ", devices)}\r\n" +
                          $"Mô tả: {txtDescription.Text.Trim()}\r\nTệp đính kèm: " +
                          (string.IsNullOrEmpty(attachmentPath) ? "Không có" : attachmentPath);
            MessageBox.Show(summary, "Tóm tắt báo cáo sự cố", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }));
        buttonRow.Controls.Add(MakeButton("Làm mới", (_, _) =>
        {
            txtCode.Clear();
            txtRequester.Clear();
            date.Value = DateTime.Now;
            cboType.SelectedIndex = 0;
            rbMedium.Checked = true;
            cbLaptop.Checked = cbNetwork.Checked = cbPrinter.Checked = cbAccount.Checked = false;
            txtDescription.Clear();
            attachmentPath = "";
            attachment.Text = "Chưa chọn tệp đính kèm";
        }));
        content.Controls.Add(buttonRow);

        page.Controls.Add(content);
        return page;
    }

    private TabPage BuildInventoryTab()
    {
        var page = NewPage("3. Quản lý hàng hóa");
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(4) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        var input = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(4) };
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        for (var i = 0; i < 2; i++) input.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var txtCode = MakeTextBox("Mã hàng");
        txtCode.Dock = DockStyle.Fill;
        var txtName = MakeTextBox("Tên hàng hóa");
        txtName.Dock = DockStyle.Fill;
        var txtQuantity = MakeTextBox("Số lượng");
        txtQuantity.Dock = DockStyle.Fill;
        var txtPrice = MakeTextBox("Đơn giá");
        txtPrice.Dock = DockStyle.Fill;
        input.Controls.Add(MakeLabel("Mã hàng:", 85), 0, 0);
        input.Controls.Add(txtCode, 1, 0);
        input.Controls.Add(MakeLabel("Tên hàng:", 85), 2, 0);
        input.Controls.Add(txtName, 3, 0);
        input.Controls.Add(MakeLabel("Số lượng:", 85), 0, 1);
        input.Controls.Add(txtQuantity, 1, 1);
        input.Controls.Add(MakeLabel("Đơn giá (đ):", 85), 2, 1);
        input.Controls.Add(txtPrice, 3, 1);

        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
        var grid = MakeGrid();
        var status = new Label { Dock = DockStyle.Fill, Text = "Chọn một dòng để sửa hoặc xóa.", TextAlign = ContentAlignment.MiddleLeft };
        var items = new List<InventoryItem>();
        string? selectedCode = null;

        void RefreshGrid()
        {
            grid.DataSource = null;
            grid.DataSource = items.Select(i => new
            {
                i.Code,
                i.Name,
                i.Quantity,
                UnitPrice = i.UnitPrice.ToString("N0", ViCulture) + " đ",
                Amount = (i.Quantity * i.UnitPrice).ToString("N0", ViCulture) + " đ"
            }).ToList();
            if (grid.Columns.Count >= 5)
            {
                grid.Columns[0].HeaderText = "Mã hàng";
                grid.Columns[1].HeaderText = "Tên hàng";
                grid.Columns[2].HeaderText = "Số lượng";
                grid.Columns[3].HeaderText = "Đơn giá";
                grid.Columns[4].HeaderText = "Thành tiền";
            }
            status.Text = $"Tổng số mặt hàng: {items.Count}";
        }

        void ResetInputs()
        {
            txtCode.Clear();
            txtName.Clear();
            txtQuantity.Clear();
            txtPrice.Clear();
            selectedCode = null;
            grid.ClearSelection();
            txtCode.Focus();
        }

        bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtCode.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Mã hàng và tên hàng không được để trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtQuantity.Text, out var quantity) || quantity < 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên không âm.");
                return false;
            }
            if (!decimal.TryParse(txtPrice.Text, NumberStyles.Number, ViCulture, out var price) || price < 0)
            {
                MessageBox.Show("Đơn giá phải là số không âm.");
                return false;
            }
            return true;
        }

        actions.Controls.Add(MakeButton("Thêm", (_, _) =>
        {
            if (!ValidateInputs()) return;
            var code = txtCode.Text.Trim();
            if (items.Any(i => i.Code.Equals(code, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã hàng đã tồn tại. Hãy nhập mã khác.");
                txtCode.Focus();
                return;
            }
            items.Add(new InventoryItem(code, txtName.Text.Trim(), int.Parse(txtQuantity.Text), decimal.Parse(txtPrice.Text, NumberStyles.Number, ViCulture)));
            RefreshGrid();
            ResetInputs();
        }, 100));
        actions.Controls.Add(MakeButton("Cập nhật", (_, _) =>
        {
            if (selectedCode is null)
            {
                MessageBox.Show("Hãy chọn một mặt hàng trong bảng trước.");
                return;
            }
            if (!ValidateInputs()) return;
            var newCode = txtCode.Text.Trim();
            if (items.Any(i => !i.Code.Equals(selectedCode, StringComparison.OrdinalIgnoreCase) && i.Code.Equals(newCode, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã hàng mới đã được sử dụng.");
                return;
            }
            var item = items.FirstOrDefault(i => i.Code.Equals(selectedCode, StringComparison.OrdinalIgnoreCase));
            if (item is null) return;
            item.Code = newCode;
            item.Name = txtName.Text.Trim();
            item.Quantity = int.Parse(txtQuantity.Text);
            item.UnitPrice = decimal.Parse(txtPrice.Text, NumberStyles.Number, ViCulture);
            RefreshGrid();
            ResetInputs();
        }, 115));
        actions.Controls.Add(MakeButton("Xóa", (_, _) =>
        {
            if (grid.CurrentRow is null)
            {
                MessageBox.Show("Hãy chọn mặt hàng cần xóa.");
                return;
            }
            var code = grid.CurrentRow.Cells[0].Value?.ToString();
            var item = items.FirstOrDefault(i => i.Code == code);
            if (item is not null) items.Remove(item);
            RefreshGrid();
            ResetInputs();
        }, 95));
        actions.Controls.Add(MakeButton("Làm mới", (_, _) => ResetInputs(), 105));

        grid.SelectionChanged += (_, _) =>
        {
            if (grid.CurrentRow is null || grid.CurrentRow.Cells.Count < 5 || grid.CurrentRow.Cells[0].Value is null) return;
            var code = grid.CurrentRow.Cells[0].Value.ToString();
            var item = items.FirstOrDefault(i => i.Code == code);
            if (item is null) return;
            selectedCode = item.Code;
            txtCode.Text = item.Code;
            txtName.Text = item.Name;
            txtQuantity.Text = item.Quantity.ToString();
            txtPrice.Text = item.UnitPrice.ToString("0.##", ViCulture);
        };

        root.Controls.Add(input, 0, 0);
        root.Controls.Add(actions, 0, 1);
        root.Controls.Add(grid, 0, 2);
        root.Controls.Add(status, 0, 3);
        page.Controls.Add(root);
        RefreshGrid();
        return page;
    }

    private TabPage BuildSeatTab()
    {
        var page = NewPage("4. Chọn ghế động");
        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 42,
            Text = "Chọn ghế cần đặt (màu xám là ghế không khả dụng).",
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font(Font.FontFamily, 11F, FontStyle.Bold)
        };
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false };
        toolbar.Controls.Add(MakeLabel("Khung giờ:", 95));
        var cboTime = new ComboBox { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
        cboTime.Items.AddRange(new object[] { "08:00 - 12:00 (50.000 đ)", "12:00 - 17:00 (70.000 đ)", "17:00 - 22:00 (90.000 đ)" });
        cboTime.SelectedIndex = 1;
        toolbar.Controls.Add(cboTime);

        var seatPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 260,
            Padding = new Padding(10),
            WrapContents = true,
            AutoScroll = true
        };
        var selected = new HashSet<int>();
        var seatButtons = new List<Button>();
        var summary = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            TextAlign = ContentAlignment.TopLeft,
            Font = new Font(Font.FontFamily, 11F),
            Text = "Chưa chọn ghế."
        };

        decimal GetPrice() => cboTime.SelectedIndex switch { 0 => 50000m, 2 => 90000m, _ => 70000m };

        void UpdateSummary()
        {
            var price = GetPrice();
            var seats = selected.OrderBy(n => n).ToList();
            summary.Text = seats.Count == 0
                ? "Chưa chọn ghế."
                : $"Ghế đã chọn: {string.Join(", ", seats.Select(n => $"G{n:00}"))}\r\n" +
                  $"Số lượng: {seats.Count}\r\nĐơn giá: {price.ToString("N0", ViCulture)} đ\r\n" +
                  $"TỔNG TIỀN: {(price * seats.Count).ToString("N0", ViCulture)} đ";
            foreach (var button in seatButtons)
            {
                var number = (int)button.Tag!;
                if (!button.Enabled) button.BackColor = Color.LightGray;
                else button.BackColor = selected.Contains(number) ? Color.LightGreen : SystemColors.Control;
            }
        }

        for (var i = 1; i <= 20; i++)
        {
            var number = i;
            var button = new Button
            {
                Text = $"Ghế {number:00}",
                Width = 100,
                Height = 48,
                Margin = new Padding(8),
                Tag = number,
                Enabled = number != 5 && number != 16
            };
            button.Click += (_, _) =>
            {
                if (!button.Enabled) return;
                if (!selected.Add(number)) selected.Remove(number);
                UpdateSummary();
            };
            seatButtons.Add(button);
            seatPanel.Controls.Add(button);
        }

        cboTime.SelectedIndexChanged += (_, _) => UpdateSummary();

        var footer = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(5) };
        footer.Controls.Add(MakeButton("Xác nhận đặt ghế", (_, _) =>
        {
            if (selected.Count == 0)
            {
                MessageBox.Show("Bạn chưa chọn ghế nào.");
                return;
            }
            MessageBox.Show($"Đã chọn {selected.Count} ghế.\r\n{summary.Text}", "Xác nhận", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }, 170));
        footer.Controls.Add(MakeButton("Bỏ chọn tất cả", (_, _) =>
        {
            selected.Clear();
            UpdateSummary();
        }, 145));

        page.Controls.Add(summary);
        page.Controls.Add(footer);
        page.Controls.Add(seatPanel);
        page.Controls.Add(toolbar);
        page.Controls.Add(title);
        UpdateSummary();
        return page;
    }

    private TabPage BuildDeliveryTab()
    {
        var page = NewPage("5. Dashboard giao hàng");
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 350,
            FixedPanel = FixedPanel.Panel1,
            Panel1MinSize = 300,
            Panel2MinSize = 400
        };

        var orders = new BindingList<DeliveryOrder>();
        var grid = MakeGrid();
        var source = new BindingSource { DataSource = orders };
        grid.DataSource = source;
        grid.Columns.Clear();

        var txtCustomer = MakeTextBox("Tên khách hàng");
        var txtAddress = MakeTextBox("Địa chỉ giao hàng");
        var txtProduct = MakeTextBox("Tên mặt hàng");
        var txtQuantity = MakeTextBox("Số lượng");
        var txtPrice = MakeTextBox("Đơn giá (đ)");
        var cboStatus = new ComboBox { Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
        cboStatus.Items.AddRange(new object[] { "Chờ xử lý", "Đang giao", "Đã giao", "Đã hủy" });
        cboStatus.SelectedIndex = 0;
        foreach (var textBox in new[] { txtCustomer, txtAddress, txtProduct, txtQuantity, txtPrice })
            textBox.Width = 240;

        var errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink, ContainerControl = this };
        var formPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(10)
        };
        var header = new Label
        {
            Text = "THÔNG TIN ĐƠN GIAO HÀNG",
            Width = 300,
            Height = 38,
            Font = new Font(Font.FontFamily, 11F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };
        formPanel.Controls.Add(header);

        void AddField(string caption, Control control)
        {
            formPanel.Controls.Add(new Label { Text = caption, Width = 290, Height = 23, Margin = new Padding(3, 5, 3, 0) });
            control.Margin = new Padding(3, 1, 3, 5);
            formPanel.Controls.Add(control);
        }

        AddField("Khách hàng", txtCustomer);
        AddField("Địa chỉ", txtAddress);
        AddField("Mặt hàng", txtProduct);
        AddField("Số lượng", txtQuantity);
        AddField("Đơn giá (đ)", txtPrice);
        AddField("Trạng thái", cboStatus);

        var addButton = MakeButton("Thêm đơn (F2)", (_, _) => AddOrder(), 240);
        var deleteButton = MakeButton("Xóa đơn (Delete)", (_, _) => DeleteSelectedOrder(), 240);
        formPanel.Controls.Add(addButton);
        formPanel.Controls.Add(deleteButton);
        formPanel.Controls.Add(new Label
        {
            Text = "Mẹo: nhấn F2 để thêm đơn, Delete để xóa dòng đang chọn.",
            Width = 290,
            Height = 55,
            ForeColor = Color.DimGray
        });

        var statusStrip = new StatusStrip();
        var clock = new ToolStripStatusLabel();
        var totals = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleRight };
        statusStrip.Items.Add(clock);
        statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusStrip.Items.Add(totals);
        var timer = new System.Windows.Forms.Timer { Interval = 1000 };
        timer.Tick += (_, _) =>
        {
            clock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            UpdateTotals();
        };
        timer.Start();

        void UpdateTotals()
        {
            var amount = orders.Sum(o => o.Total);
            totals.Text = $"Số đơn: {orders.Count}    Tổng giá trị: {amount.ToString("N0", ViCulture)} đ";
        }

        bool ValidateOrder()
        {
            errorProvider.Clear();
            var valid = true;
            if (string.IsNullOrWhiteSpace(txtCustomer.Text))
            {
                errorProvider.SetError(txtCustomer, "Nhập tên khách hàng.");
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                errorProvider.SetError(txtAddress, "Nhập địa chỉ giao hàng.");
                valid = false;
            }
            if (string.IsNullOrWhiteSpace(txtProduct.Text))
            {
                errorProvider.SetError(txtProduct, "Nhập tên mặt hàng.");
                valid = false;
            }
            if (!int.TryParse(txtQuantity.Text, out var quantity) || quantity <= 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng phải lớn hơn 0.");
                valid = false;
            }
            if (!decimal.TryParse(txtPrice.Text, NumberStyles.Number, ViCulture, out var price) || price < 0)
            {
                errorProvider.SetError(txtPrice, "Đơn giá phải là số không âm.");
                valid = false;
            }
            return valid;
        }

        void AddOrder()
        {
            if (!ValidateOrder())
            {
                MessageBox.Show("Vui lòng sửa các trường được đánh dấu.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            orders.Add(new DeliveryOrder
            {
                OrderId = $"DH{DateTime.Now:yyMMddHHmmss}",
                Customer = txtCustomer.Text.Trim(),
                Address = txtAddress.Text.Trim(),
                Product = txtProduct.Text.Trim(),
                Quantity = int.Parse(txtQuantity.Text),
                UnitPrice = decimal.Parse(txtPrice.Text, NumberStyles.Number, ViCulture),
                Status = cboStatus.Text,
                CreatedAt = DateTime.Now
            });
            source.ResetBindings(false);
            UpdateTotals();
            txtCustomer.Clear();
            txtAddress.Clear();
            txtProduct.Clear();
            txtQuantity.Clear();
            txtPrice.Clear();
            cboStatus.SelectedIndex = 0;
            errorProvider.Clear();
            txtCustomer.Focus();
        }

        void DeleteSelectedOrder()
        {
            if (grid.CurrentRow is null || grid.CurrentRow.DataBoundItem is not DeliveryOrder order)
            {
                MessageBox.Show("Chọn đơn hàng trong bảng trước khi xóa.");
                return;
            }
            orders.Remove(order);
            UpdateTotals();
        }

        split.Panel1.Controls.Add(formPanel);
        split.Panel2.Controls.Add(grid);
        Controls.Add(split);
        Controls.Add(statusStrip);

        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.F2)
            {
                AddOrder();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedOrder();
                e.Handled = true;
            }
        };
        grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedOrder();
                e.Handled = true;
            }
        };

        orders.Add(new DeliveryOrder
        {
            OrderId = "DH0001",
            Customer = "Nguyễn Văn An",
            Address = "Hà Nội",
            Product = "Bàn phím",
            Quantity = 1,
            UnitPrice = 350000m,
            Status = "Đang giao",
            CreatedAt = DateTime.Now
        });
        orders.Add(new DeliveryOrder
        {
            OrderId = "DH0002",
            Customer = "Trần Minh Hà",
            Address = "Bắc Ninh",
            Product = "Chuột máy tính",
            Quantity = 2,
            UnitPrice = 180000m,
            Status = "Chờ xử lý",
            CreatedAt = DateTime.Now
        });
        source.ResetBindings(false);
        UpdateTotals();
        clock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        page.Controls.Add(split);
        page.Controls.Add(statusStrip);
        page.Controls.SetChildIndex(statusStrip, 0);
        return page;
    }

    private sealed class InventoryItem(string code, string name, int quantity, decimal unitPrice)
    {
        public string Code { get; set; } = code;
        public string Name { get; set; } = name;
        public int Quantity { get; set; } = quantity;
        public decimal UnitPrice { get; set; } = unitPrice;
    }

    private sealed class DeliveryOrder
    {
        public string OrderId { get; set; } = "";
        public string Customer { get; set; } = "";
        public string Address { get; set; } = "";
        public string Product { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }
}
