using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProjectTaskManager.BUS;
using ProjectTaskManager.DTO;

namespace ProjectTaskManager.GUI
{
    public class FormMain : Form
    {
        // ---------- BUS ----------
        private readonly DuAnBUS duAnBus = new DuAnBUS();
        private readonly ThanhVienBUS tvBus = new ThanhVienBUS();
        private readonly CongViecBUS cvBus = new CongViecBUS();
        private readonly PhanCongBUS pcBus = new PhanCongBUS();
        private readonly ThongKeBUS tkBus = new ThongKeBUS();

        private bool loading;   // chặn event SelectedIndexChanged khi đang nạp dữ liệu

        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };

        // ---------- Tab Dự án ----------
        private DataGridView dgvDuAn;
        private TextBox txtTenDuAn;
        private DateTimePicker dtpBD, dtpKT;
        private ComboBox cboTTDuAn;

        // ---------- Tab Công việc (List view) ----------
        private ComboBox cboDuAnTask, cboLocTT, cboUuTien, cboChuyen;
        private DataGridView dgvTask;
        private TextBox txtTenTask, txtMoTa;
        private DateTimePicker dtpHan;
        private CheckedListBox clbThanhVien;

        // ---------- Tab Kanban ----------
        private ComboBox cboDuAnKanban;
        private readonly ListBox[] lstKanban = new ListBox[4];
        private ListBox lstDangChon;

        // ---------- Tab Báo cáo ----------
        private ComboBox cboDuAnReport;
        private DataGridView dgvQuaHan, dgvThongKe;
        private Label lblTyLe;

        public FormMain()
        {
            Text = "Quản lý Công việc & Tiến độ Dự án";
            Size = new Size(1150, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(tabs);
            BuildDuAnTab();
            BuildTaskTab();
            BuildKanbanTab();
            BuildReportTab();

            Load += (s, e) => Run(() => { LoadDuAn(); LoadThanhVien(); });
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        private void Run(Action a)
        {
            try { a(); }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (InvalidOperationException ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static DataGridView NewGrid() => new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };

        private static FlowLayoutPanel Bar(DockStyle dock) => new FlowLayoutPanel
        {
            Dock = dock, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Padding = new Padding(6), WrapContents = true
        };

        private static Label Lbl(string t) => new Label { Text = t, AutoSize = true, Margin = new Padding(6, 8, 2, 0) };

        private static Button Btn(string t, EventHandler h)
        {
            var b = new Button { Text = t, AutoSize = true, Margin = new Padding(4) };
            b.Click += h;
            return b;
        }

        private static ComboBox Combo(int width, params string[] items)
        {
            var c = new ComboBox { Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
            if (items.Length > 0) { c.Items.AddRange(items); c.SelectedIndex = 0; }
            return c;
        }

        private static void BindProjects(ComboBox c, List<DuAnDTO> src)
        {
            c.DataSource = src.ToList();      // mỗi combo 1 bản sao riêng để không bị đồng bộ vị trí chọn
            c.DisplayMember = "TenDuAn";
            c.ValueMember = "MaDuAn";
        }

        private static int? SelectedId(ComboBox c) => c.SelectedValue is int v ? v : (int?)null;

        private static void HideCols(DataGridView g, params string[] names)
        {
            foreach (var n in names) if (g.Columns.Contains(n)) g.Columns[n].Visible = false;
        }

        // =====================================================================
        // TAB 1 - DỰ ÁN
        // =====================================================================
        private void BuildDuAnTab()
        {
            var page = new TabPage("Dự án");
            dgvDuAn = NewGrid();
            dgvDuAn.SelectionChanged += (s, e) =>
            {
                if (loading || !(dgvDuAn.CurrentRow?.DataBoundItem is DuAnDTO d)) return;
                txtTenDuAn.Text = d.TenDuAn;
                dtpBD.Value = d.NgayBatDau ?? DateTime.Today;
                dtpKT.Value = d.NgayKetThuc ?? DateTime.Today;
                cboTTDuAn.SelectedItem = d.TrangThai;
            };
            page.Controls.Add(dgvDuAn);

            var bar = Bar(DockStyle.Top);
            txtTenDuAn = new TextBox { Width = 220 };
            dtpBD = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
            dtpKT = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
            cboTTDuAn = Combo(110, TrangThaiDuAn.All);
            bar.Controls.AddRange(new Control[]
            {
                Lbl("Tên dự án:"), txtTenDuAn, Lbl("Bắt đầu:"), dtpBD, Lbl("Kết thúc:"), dtpKT,
                Lbl("Trạng thái:"), cboTTDuAn,
                Btn("Thêm", (s, e) => Run(ThemDuAn)),
                Btn("Sửa", (s, e) => Run(SuaDuAn)),
                Btn("Xóa", (s, e) => Run(XoaDuAn))
            });
            page.Controls.Add(bar);
            tabs.TabPages.Add(page);
        }

        private DuAnDTO ReadDuAnForm() => new DuAnDTO
        {
            TenDuAn = txtTenDuAn.Text.Trim(),
            NgayBatDau = dtpBD.Value.Date,
            NgayKetThuc = dtpKT.Value.Date,
            TrangThai = cboTTDuAn.SelectedItem as string
        };

        private void ThemDuAn()
        {
            duAnBus.Them(ReadDuAnForm());
            LoadDuAn();
        }

        private void SuaDuAn()
        {
            if (!(dgvDuAn.CurrentRow?.DataBoundItem is DuAnDTO cur)) throw new InvalidOperationException("Hãy chọn một dự án.");
            var d = ReadDuAnForm();
            d.MaDuAn = cur.MaDuAn;
            duAnBus.Sua(d);
            LoadDuAn();
        }

        private void XoaDuAn()
        {
            if (!(dgvDuAn.CurrentRow?.DataBoundItem is DuAnDTO cur)) throw new InvalidOperationException("Hãy chọn một dự án.");
            if (MessageBox.Show($"Xóa dự án \"{cur.TenDuAn}\" cùng toàn bộ công việc và phân công của nó?",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            duAnBus.Xoa(cur.MaDuAn);
            LoadDuAn();
        }

        private void LoadDuAn()
        {
            loading = true;
            try
            {
                var ds = duAnBus.GetAll();
                dgvDuAn.DataSource = ds;
                BindProjects(cboDuAnTask, ds);
                BindProjects(cboDuAnKanban, ds);
                BindProjects(cboDuAnReport, ds);
            }
            finally { loading = false; }
            LoadTasks();
            LoadKanban();
            LoadReport();
        }

        // =====================================================================
        // TAB 2 - CÔNG VIỆC (LIST VIEW)
        // =====================================================================
        private void BuildTaskTab()
        {
            var page = new TabPage("Công việc (List)");

            dgvTask = NewGrid();
            dgvTask.SelectionChanged += (s, e) => { if (!loading) TickThanhVienCuaTask(); };
            page.Controls.Add(dgvTask);

            // --- Thanh lọc (trên cùng)
            var filter = Bar(DockStyle.Top);
            cboDuAnTask = Combo(240);
            cboDuAnTask.SelectedIndexChanged += (s, e) => { if (!loading) Run(LoadTasks); };
            cboLocTT = Combo(130, new[] { "Tất cả" }.Concat(TrangThaiTask.All).ToArray());
            cboLocTT.SelectedIndexChanged += (s, e) => { if (!loading) Run(LoadTasks); };
            filter.Controls.AddRange(new Control[] { Lbl("Dự án:"), cboDuAnTask, Lbl("Trạng thái:"), cboLocTT });
            page.Controls.Add(filter);

            // --- Khu nhập liệu & thao tác (dưới cùng)
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6), FlowDirection = FlowDirection.TopDown };

            var row1 = Bar(DockStyle.None);
            txtTenTask = new TextBox { Width = 220 };
            txtMoTa = new TextBox { Width = 260 };
            dtpHan = new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 110 };
            cboUuTien = Combo(100, DoUuTienTask.All);
            row1.Controls.AddRange(new Control[]
            {
                Lbl("Tên task:"), txtTenTask, Lbl("Mô tả:"), txtMoTa, Lbl("Hạn nộp:"), dtpHan,
                Lbl("Ưu tiên:"), cboUuTien,
                Btn("Thêm task", (s, e) => Run(ThemTask)),
                Btn("Sửa task", (s, e) => Run(SuaTask)),
                Btn("Xóa task", (s, e) => Run(XoaTask))
            });

            var row2 = Bar(DockStyle.None);
            clbThanhVien = new CheckedListBox { Width = 260, Height = 90, CheckOnClick = true };
            cboChuyen = Combo(120, TrangThaiTask.All);
            row2.Controls.AddRange(new Control[]
            {
                Lbl("Thành viên:"), clbThanhVien,
                Btn("Giao việc cho người đã chọn", (s, e) => Run(GiaoViec)),
                Btn("Hủy phân công người đã chọn", (s, e) => Run(HuyPhanCong)),
                Lbl("Chuyển sang:"), cboChuyen,
                Btn("Cập nhật trạng thái", (s, e) => Run(ChuyenTrangThaiTuList))
            });

            bottom.Controls.Add(row1);
            bottom.Controls.Add(row2);
            page.Controls.Add(bottom);
            tabs.TabPages.Add(page);
        }

        private void LoadThanhVien()
        {
            clbThanhVien.DataSource = tvBus.GetAll();
        }

        private void LoadTasks()
        {
            int? maDA = SelectedId(cboDuAnTask);
            if (maDA == null) { dgvTask.DataSource = null; return; }
            string tt = cboLocTT.SelectedIndex <= 0 ? null : (string)cboLocTT.SelectedItem;

            loading = true;
            try
            {
                dgvTask.DataSource = cvBus.GetByDuAn(maDA.Value, tt);
                HideCols(dgvTask, "MaDuAn", "MoTa", "TenDuAn");
            }
            finally { loading = false; }
            TickThanhVienCuaTask();
        }

        private CongViecDTO CurrentTask() =>
            dgvTask.CurrentRow?.DataBoundItem as CongViecDTO ?? throw new InvalidOperationException("Hãy chọn một công việc trong danh sách.");

        private void TickThanhVienCuaTask()
        {
            if (clbThanhVien.Items.Count == 0) return;
            var t = dgvTask.CurrentRow?.DataBoundItem as CongViecDTO;
            var assigned = t == null ? new List<int>() : pcBus.GetThanhVienCuaTask(t.MaTask);
            for (int i = 0; i < clbThanhVien.Items.Count; i++)
                clbThanhVien.SetItemChecked(i, assigned.Contains(((ThanhVienDTO)clbThanhVien.Items[i]).MaThanhVien));
            if (t != null) cboChuyen.SelectedItem = t.TrangThai;
        }

        private void ThemTask()
        {
            int ma = SelectedId(cboDuAnTask) ?? throw new InvalidOperationException("Hãy chọn dự án trước.");
            cvBus.Them(new CongViecDTO
            {
                MaDuAn = ma,
                TenTask = txtTenTask.Text.Trim(),
                MoTa = txtMoTa.Text.Trim(),
                HanNop = dtpHan.Value.Date,
                DoUuTien = cboUuTien.SelectedItem as string
            });
            txtTenTask.Clear(); txtMoTa.Clear();
            RefreshTaskViews();
        }

        private void SuaTask()
        {
            var t = CurrentTask();
            t.TenTask = txtTenTask.Text.Trim();
            t.MoTa = txtMoTa.Text.Trim();
            t.HanNop = dtpHan.Value.Date;
            t.DoUuTien = cboUuTien.SelectedItem as string;
            cvBus.Sua(t);
            RefreshTaskViews();
        }

        private void XoaTask()
        {
            var t = CurrentTask();
            if (MessageBox.Show($"Xóa công việc \"{t.TenTask}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            cvBus.Xoa(t.MaTask);
            RefreshTaskViews();
        }

        private void GiaoViec()
        {
            var t = CurrentTask();
            var ids = clbThanhVien.CheckedItems.Cast<ThanhVienDTO>().Select(x => x.MaThanhVien);
            int n = pcBus.GiaoViec(t.MaTask, ids);
            MessageBox.Show($"Đã giao thêm cho {n} thành viên.", "Phân công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshTaskViews();
        }

        private void HuyPhanCong()
        {
            var t = CurrentTask();
            var ds = clbThanhVien.CheckedItems.Cast<ThanhVienDTO>().ToList();
            if (ds.Count == 0) throw new ArgumentException("Hãy tick thành viên cần hủy phân công.");
            foreach (var tv in ds) pcBus.HuyPhanCong(t.MaTask, tv.MaThanhVien);
            RefreshTaskViews();
        }

        private void ChuyenTrangThaiTuList()
        {
            var t = CurrentTask();
            cvBus.ChuyenTrangThai(t.MaTask, (string)cboChuyen.SelectedItem);
            RefreshTaskViews();
        }

        private void RefreshTaskViews()
        {
            LoadTasks(); LoadKanban(); LoadReport();
        }

        // =====================================================================
        // TAB 3 - KANBAN
        // =====================================================================
        private void BuildKanbanTab()
        {
            var page = new TabPage("Kanban");

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(6) };
            for (int c = 0; c < 4; c++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            for (int i = 0; i < 4; i++)
            {
                table.Controls.Add(new Label
                {
                    Text = TrangThaiTask.All[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(Font, FontStyle.Bold), BackColor = Color.Gainsboro
                }, i, 0);

                var lb = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false, Margin = new Padding(3) };
                lb.SelectedIndexChanged += KanbanSelectionChanged;
                lstKanban[i] = lb;
                table.Controls.Add(lb, i, 1);
            }
            page.Controls.Add(table);

            var bar = Bar(DockStyle.Top);
            cboDuAnKanban = Combo(240);
            cboDuAnKanban.SelectedIndexChanged += (s, e) => { if (!loading) Run(LoadKanban); };
            bar.Controls.AddRange(new Control[]
            {
                Lbl("Dự án:"), cboDuAnKanban,
                Btn("← Lùi cột", (s, e) => Run(() => DiChuyenKanban(-1))),
                Btn("Tiến cột →", (s, e) => Run(() => DiChuyenKanban(+1)))
            });
            page.Controls.Add(bar);
            tabs.TabPages.Add(page);
        }

        private void KanbanSelectionChanged(object sender, EventArgs e)
        {
            var lb = (ListBox)sender;
            if (loading || lb.SelectedItem == null) return;
            lstDangChon = lb;
            foreach (var other in lstKanban.Where(x => x != lb)) other.ClearSelected();
        }

        private void LoadKanban()
        {
            int? maDA = SelectedId(cboDuAnKanban);
            loading = true;
            try
            {
                lstDangChon = null;
                var all = maDA == null ? new List<CongViecDTO>() : cvBus.GetByDuAn(maDA.Value);
                for (int i = 0; i < 4; i++)
                {
                    lstKanban[i].DataSource = all.Where(t => t.TrangThai == TrangThaiTask.All[i]).ToList();
                    lstKanban[i].ClearSelected();
                }
            }
            finally { loading = false; }
        }

        private void DiChuyenKanban(int buoc)
        {
            if (!(lstDangChon?.SelectedItem is CongViecDTO t)) throw new InvalidOperationException("Hãy chọn một thẻ công việc.");
            cvBus.ChuyenKe(t, buoc);                // UPDATE có tham số ở DAL
            RefreshTaskViews();
        }

        // =====================================================================
        // TAB 4 - BÁO CÁO & CẢNH BÁO TRỄ HẠN
        // =====================================================================
        private void BuildReportTab()
        {
            var page = new TabPage("Báo cáo & Trễ hạn");

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            table.Controls.Add(new Label { Text = "⚠ Công việc quá hạn (HanNop < hiện tại và chưa Done)", AutoSize = true, ForeColor = Color.Firebrick, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(6, 6, 0, 0) }, 0, 0);
            dgvQuaHan = NewGrid();
            table.Controls.Add(dgvQuaHan, 0, 1);

            lblTyLe = new Label { Text = "Tỷ lệ hoàn thành theo thành viên", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(6, 6, 0, 0) };
            table.Controls.Add(lblTyLe, 0, 2);
            dgvThongKe = NewGrid();
            dgvThongKe.SelectionChanged += (s, e) => { if (!loading) Run(HienTyLeScalar); };
            table.Controls.Add(dgvThongKe, 0, 3);

            page.Controls.Add(table);

            var bar = Bar(DockStyle.Top);
            cboDuAnReport = Combo(240);
            cboDuAnReport.SelectedIndexChanged += (s, e) => { if (!loading) Run(LoadReport); };
            bar.Controls.AddRange(new Control[] { Lbl("Dự án:"), cboDuAnReport, Btn("Làm mới", (s, e) => Run(LoadReport)) });
            page.Controls.Add(bar);
            tabs.TabPages.Add(page);
        }

        private void LoadReport()
        {
            int? maDA = SelectedId(cboDuAnReport);
            loading = true;
            try
            {
                // Quá hạn của dự án đang chọn (SqlDataReader trong DAL)
                dgvQuaHan.DataSource = cvBus.GetOverdue(maDA);
                HideCols(dgvQuaHan, "MaDuAn", "MoTa");

                dgvThongKe.DataSource = maDA == null ? null : tkBus.TheoThanhVien(maDA.Value);
                HideCols(dgvThongKe, "MaThanhVien");
                if (dgvThongKe.Columns.Contains("TyLeHoanThanh")) dgvThongKe.Columns["TyLeHoanThanh"].HeaderText = "% hoàn thành";
            }
            finally { loading = false; }

            if (maDA != null)
                lblTyLe.Text = $"Tỷ lệ hoàn thành toàn dự án: {tkBus.TyLeDuAn(maDA.Value)}%  (theo từng thành viên bên dưới)";
        }

        // Minh họa ExecuteScalar: tỷ lệ của thành viên đang chọn
        private void HienTyLeScalar()
        {
            int? maDA = SelectedId(cboDuAnReport);
            if (maDA == null || !(dgvThongKe.CurrentRow?.DataBoundItem is ThongKeThanhVienDTO tv)) return;
            lblTyLe.Text = $"{tv.HoTen}: hoàn thành {tkBus.TyLeThanhVien(maDA.Value, tv.MaThanhVien)}% | " +
                           $"Toàn dự án: {tkBus.TyLeDuAn(maDA.Value)}%";
        }
    }
}
