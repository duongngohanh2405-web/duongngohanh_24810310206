using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GarageManager.BUS;
using GarageManager.DTO;

namespace GarageManager.GUI
{
    public class FormMain : Form
    {
        private readonly XeBUS xeBus = new XeBUS();
        private readonly PhuTungBUS ptBus = new PhuTungBUS();
        private readonly PhieuSuaChuaBUS phieuBus = new PhieuSuaChuaBUS();

        private bool loading;
        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };

        // ----- Tab 1: Tiếp nhận xe -----
        private DataGridView dgvXe;
        private TextBox txtTimBS, txtBS, txtTenKH, txtDT, txtDiaChi, txtHieu;
        private NumericUpDown nudNam;
        private Label lblXe;

        // ----- Tab 2: Phụ tùng -----
        private DataGridView dgvPT;
        private TextBox txtTenPT;
        private NumericUpDown nudGia, nudTon;

        // ----- Tab 3: Lập phiếu -----
        private TextBox txtBSPhieu, txtNoiDung;
        private Label lblChuXe, lblTamTinh;
        private ComboBox cboPT;
        private NumericUpDown nudSL, nudCong;
        private DataGridView dgvCT;
        private readonly BindingList<ChiTietSuaChuaDTO> gioHang = new BindingList<ChiTietSuaChuaDTO>();

        // ----- Tab 4: Thanh toán & hóa đơn -----
        private ComboBox cboLoc;
        private DataGridView dgvPhieu, dgvHoaDon;
        private Label lblTong;
        private HoaDonDTO hoaDonHienTai;

        public FormMain()
        {
            Text = "Quản lý Garage & Bảo dưỡng Ô tô";
            Size = new Size(1200, 740);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            Controls.Add(tabs);
            BuildXeTab();
            BuildPhuTungTab();
            BuildPhieuTab();
            BuildThanhToanTab();

            Load += (s, e) => Run(() => { LoadXe(); LoadPhuTung(); LoadPhieu(); });
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        private void Run(Action a)
        {
            try { a(); }
            catch (KhoKhongDuException ex) { MessageBox.Show(ex.Message, "Không đủ phụ tùng trong kho", MessageBoxButtons.OK, MessageBoxIcon.Stop); }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (InvalidOperationException ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static DataGridView NewGrid() => new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
            MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White,
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

        private static NumericUpDown Nud(decimal min, decimal max, decimal inc, int width = 100) =>
            new NumericUpDown { Minimum = min, Maximum = max, Increment = inc, Width = width, ThousandsSeparator = true };

        private static void HideCols(DataGridView g, params string[] names)
        {
            foreach (var n in names) if (g.Columns.Contains(n)) g.Columns[n].Visible = false;
        }

        private static void Money(DataGridView g, params string[] names)
        {
            foreach (var n in names) if (g.Columns.Contains(n)) g.Columns[n].DefaultCellStyle.Format = "N0";
        }

        // =====================================================================
        // TAB 1 - TIẾP NHẬN XE & CHỦ XE
        // =====================================================================
        private void BuildXeTab()
        {
            var page = new TabPage("Tiếp nhận xe & Chủ xe");

            dgvXe = NewGrid();
            dgvXe.SelectionChanged += (s, e) =>
            {
                if (!loading && dgvXe.CurrentRow?.DataBoundItem is XeDTO x) FillXeForm(x);
            };
            page.Controls.Add(dgvXe);

            var form = Bar(DockStyle.Top);
            txtBS = new TextBox { Width = 110 };
            txtTenKH = new TextBox { Width = 160 };
            txtDT = new TextBox { Width = 110 };
            txtDiaChi = new TextBox { Width = 200 };
            txtHieu = new TextBox { Width = 90 };
            nudNam = Nud(1950, 2100, 1, 70); nudNam.Value = 2020; nudNam.ThousandsSeparator = false;
            form.Controls.AddRange(new Control[]
            {
                Lbl("Biển số:"), txtBS, Lbl("Chủ xe:"), txtTenKH, Lbl("Điện thoại:"), txtDT,
                Lbl("Địa chỉ:"), txtDiaChi, Lbl("Hiệu xe:"), txtHieu, Lbl("Năm SX:"), nudNam
            });
            page.Controls.Add(form);

            var act = Bar(DockStyle.Top);
            act.Controls.AddRange(new Control[]
            {
                Btn("Tiếp nhận xe mới", (s, e) => Run(TiepNhanXeMoi)),
                Btn("Cập nhật xe & chủ xe", (s, e) => Run(CapNhatXe)),
                Btn("Xóa xe", (s, e) => Run(XoaXe)),
                Btn("Lập phiếu sửa chữa cho xe này →", (s, e) =>
                {
                    txtBSPhieu.Text = txtBS.Text.Trim().ToUpperInvariant();
                    tabs.SelectedIndex = 2;
                    Run(KiemTraXeChoPhieu);
                })
            });
            page.Controls.Add(act);

            var search = Bar(DockStyle.Top);
            txtTimBS = new TextBox { Width = 140 };
            txtTimBS.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Run(TraCuuXe); } };
            lblXe = new Label { AutoSize = true, Margin = new Padding(10, 8, 0, 0), ForeColor = Color.DarkBlue };
            search.Controls.AddRange(new Control[] { Lbl("Tra cứu biển số:"), txtTimBS, Btn("Tra cứu", (s, e) => Run(TraCuuXe)), lblXe });
            page.Controls.Add(search);

            tabs.TabPages.Add(page);
        }

        private void FillXeForm(XeDTO x)
        {
            txtBS.Text = x.BienSo; txtTenKH.Text = x.TenKH; txtDT.Text = x.DienThoai;
            txtDiaChi.Text = x.DiaChi; txtHieu.Text = x.HieuXe;
            nudNam.Value = x.NamSanXuat.HasValue ? Math.Min(Math.Max(x.NamSanXuat.Value, 1950), 2100) : 2020;
        }

        private void TraCuuXe()
        {
            var x = xeBus.TraCuu(txtTimBS.Text);        // truy vấn bằng SqlParameter
            if (x != null)
            {
                FillXeForm(x);
                lblXe.Text = $"✔ Xe đã có trong hệ thống - chủ xe: {x.TenKH} ({x.DienThoai}).";
            }
            else
            {
                txtTenKH.Clear(); txtDT.Clear(); txtDiaChi.Clear(); txtHieu.Clear();
                txtBS.Text = XeBUS.ChuanHoaBienSo(txtTimBS.Text);
                lblXe.Text = "✚ Xe mới: nhập thông tin chủ xe và xe bên dưới rồi bấm \"Tiếp nhận xe mới\".";
                txtTenKH.Focus();
            }
        }

        private (KhachHangDTO, XeDTO) ReadXeForm() => (
            new KhachHangDTO { TenKH = txtTenKH.Text, DienThoai = txtDT.Text, DiaChi = txtDiaChi.Text },
            new XeDTO { BienSo = txtBS.Text, HieuXe = txtHieu.Text, NamSanXuat = (int)nudNam.Value });

        private void TiepNhanXeMoi()
        {
            var (kh, xe) = ReadXeForm();
            xeBus.TiepNhanXeMoi(kh, xe);
            lblXe.Text = $"✔ Đã tiếp nhận xe {xe.BienSo}.";
            LoadXe();
        }

        private void CapNhatXe()
        {
            var (kh, xe) = ReadXeForm();
            xeBus.CapNhat(kh, xe);
            LoadXe();
        }

        private void XoaXe()
        {
            string bs = txtBS.Text.Trim();
            if (bs.Length == 0) throw new InvalidOperationException("Hãy chọn xe cần xóa.");
            if (MessageBox.Show($"Xóa xe {bs}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            xeBus.Xoa(bs);
            LoadXe();
        }

        private void LoadXe()
        {
            loading = true;
            try
            {
                dgvXe.DataSource = xeBus.GetAll();
                HideCols(dgvXe, "MaKH");
            }
            finally { loading = false; }
        }

        // =====================================================================
        // TAB 2 - PHỤ TÙNG
        // =====================================================================
        private void BuildPhuTungTab()
        {
            var page = new TabPage("Phụ tùng & Kho");

            dgvPT = NewGrid();
            dgvPT.SelectionChanged += (s, e) =>
            {
                if (loading || !(dgvPT.CurrentRow?.DataBoundItem is PhuTungDTO p)) return;
                txtTenPT.Text = p.TenPhuTung; nudGia.Value = p.DonGia; nudTon.Value = p.SoLuongTon;
            };
            page.Controls.Add(dgvPT);

            var bar = Bar(DockStyle.Top);
            txtTenPT = new TextBox { Width = 240 };
            nudGia = Nud(0, 1_000_000_000, 1000, 120);
            nudTon = Nud(0, 1_000_000, 1, 80);
            bar.Controls.AddRange(new Control[]
            {
                Lbl("Tên phụ tùng:"), txtTenPT, Lbl("Đơn giá:"), nudGia, Lbl("Tồn kho:"), nudTon,
                Btn("Thêm", (s, e) => Run(() => { ptBus.Them(ReadPT(0)); LoadPhuTung(); })),
                Btn("Cập nhật giá / tồn kho", (s, e) => Run(() =>
                {
                    if (!(dgvPT.CurrentRow?.DataBoundItem is PhuTungDTO cur)) throw new InvalidOperationException("Hãy chọn một phụ tùng.");
                    ptBus.Sua(ReadPT(cur.MaPhuTung)); LoadPhuTung();
                })),
                Btn("Xóa", (s, e) => Run(() =>
                {
                    if (!(dgvPT.CurrentRow?.DataBoundItem is PhuTungDTO cur)) throw new InvalidOperationException("Hãy chọn một phụ tùng.");
                    if (MessageBox.Show($"Xóa phụ tùng \"{cur.TenPhuTung}\"?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    ptBus.Xoa(cur.MaPhuTung); LoadPhuTung();
                }))
            });
            page.Controls.Add(bar);
            tabs.TabPages.Add(page);
        }

        private PhuTungDTO ReadPT(int ma) => new PhuTungDTO
        {
            MaPhuTung = ma, TenPhuTung = txtTenPT.Text, DonGia = nudGia.Value, SoLuongTon = (int)nudTon.Value
        };

        private void LoadPhuTung()
        {
            loading = true;
            try
            {
                var ds = ptBus.GetAll();
                dgvPT.DataSource = ds;
                HideCols(dgvPT, "MaPhuTung");
                Money(dgvPT, "DonGia");
                cboPT.DataSource = ds.ToList();       // combo ở tab Lập phiếu luôn thấy tồn kho mới nhất
            }
            finally { loading = false; }
        }

        // =====================================================================
        // TAB 3 - LẬP PHIẾU SỬA CHỮA (SqlTransaction)
        // =====================================================================
        private void BuildPhieuTab()
        {
            var page = new TabPage("Lập phiếu sửa chữa");

            dgvCT = NewGrid();
            dgvCT.DataSource = gioHang;
            dgvCT.DataBindingComplete += (s, e) =>
            {
                HideCols(dgvCT, "MaPhieu", "MaPhuTung");
                Money(dgvCT, "DonGia", "TienCong", "TienPhuTung", "ThanhTien");
            };
            page.Controls.Add(dgvCT);

            // Thanh dưới: tạm tính + xác nhận
            var bottom = Bar(DockStyle.Bottom);
            lblTamTinh = new Label { AutoSize = true, Margin = new Padding(6, 8, 20, 0), Font = new Font(Font, FontStyle.Bold) };
            bottom.Controls.AddRange(new Control[]
            {
                lblTamTinh,
                Btn("✔ Xác nhận hoàn tất phiếu (trừ kho)", (s, e) => Run(XacNhanPhieu)),
                Btn("Làm lại phiếu", (s, e) => { gioHang.Clear(); CapNhatTamTinh(); })
            });
            page.Controls.Add(bottom);

            // Dòng thêm phụ tùng
            var row3 = Bar(DockStyle.Top);
            cboPT = new ComboBox { Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
            nudSL = Nud(1, 10000, 1, 70); nudSL.Value = 1;
            nudCong = Nud(0, 1_000_000_000, 10000, 110);
            row3.Controls.AddRange(new Control[]
            {
                Lbl("Phụ tùng:"), cboPT, Lbl("Số lượng:"), nudSL, Lbl("Tiền công:"), nudCong,
                Btn("Thêm vào phiếu", (s, e) => Run(ThemDong)),
                Btn("Xóa dòng đã chọn", (s, e) =>
                {
                    if (dgvCT.CurrentRow?.DataBoundItem is ChiTietSuaChuaDTO c) { gioHang.Remove(c); CapNhatTamTinh(); }
                })
            });
            page.Controls.Add(row3);

            // Dòng nội dung
            var row2 = Bar(DockStyle.Top);
            txtNoiDung = new TextBox { Width = 700 };
            row2.Controls.AddRange(new Control[] { Lbl("Nội dung sửa:"), txtNoiDung });
            page.Controls.Add(row2);

            // Dòng biển số
            var row1 = Bar(DockStyle.Top);
            txtBSPhieu = new TextBox { Width = 130 };
            lblChuXe = new Label { AutoSize = true, Margin = new Padding(10, 8, 0, 0), ForeColor = Color.DarkBlue };
            row1.Controls.AddRange(new Control[]
            {
                Lbl("Biển số xe:"), txtBSPhieu, Btn("Kiểm tra xe", (s, e) => Run(KiemTraXeChoPhieu)), lblChuXe
            });
            page.Controls.Add(row1);

            tabs.TabPages.Add(page);
            CapNhatTamTinh();
        }

        private void KiemTraXeChoPhieu()
        {
            var x = xeBus.TraCuu(txtBSPhieu.Text);
            lblChuXe.Text = x == null
                ? "✘ Chưa có xe này - hãy tiếp nhận xe ở tab \"Tiếp nhận xe & Chủ xe\"."
                : $"✔ {x.HieuXe} {x.NamSanXuat} - chủ xe: {x.TenKH} ({x.DienThoai})";
        }

        private void ThemDong()
        {
            if (!(cboPT.SelectedItem is PhuTungDTO p)) throw new InvalidOperationException("Hãy chọn phụ tùng.");
            var cu = gioHang.FirstOrDefault(c => c.MaPhuTung == p.MaPhuTung);
            if (cu != null)
            {
                cu.SoLuong += (int)nudSL.Value;      // cùng phụ tùng -> cộng dồn (PK là MaPhieu + MaPhuTung)
                cu.TienCong += nudCong.Value;
                gioHang.ResetBindings();
            }
            else
            {
                gioHang.Add(new ChiTietSuaChuaDTO
                {
                    MaPhuTung = p.MaPhuTung, TenPhuTung = p.TenPhuTung, DonGia = p.DonGia,
                    SoLuong = (int)nudSL.Value, TienCong = nudCong.Value
                });
            }
            CapNhatTamTinh();
        }

        private void CapNhatTamTinh()
        {
            decimal tong = gioHang.Sum(c => c.ThanhTien);
            lblTamTinh.Text = $"Tạm tính: {tong:N0} đ";
        }

        private void XacNhanPhieu()
        {
            // BUS kiểm tra dữ liệu -> DAL chạy SqlTransaction (tạo phiếu, chi tiết, trừ kho; thiếu hàng thì Rollback)
            int ma = phieuBus.LapPhieuHoanTat(txtBSPhieu.Text, txtNoiDung.Text, gioHang);
            var hd = phieuBus.TaoHoaDon(ma);
            MessageBox.Show($"Đã lập phiếu #{ma} và trừ kho thành công.\nTổng chi phí: {hd.TongChiPhi:N0} đ",
                "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);

            gioHang.Clear(); txtNoiDung.Clear(); CapNhatTamTinh();
            LoadPhuTung(); LoadPhieu();
        }

        // =====================================================================
        // TAB 4 - THANH TOÁN & HÓA ĐƠN
        // =====================================================================
        private void BuildThanhToanTab()
        {
            var page = new TabPage("Thanh toán & Hóa đơn");

            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
            dgvPhieu = NewGrid();
            dgvPhieu.SelectionChanged += (s, e) => { if (!loading) Run(HienChiTietPhieu); };
            split.Panel1.Controls.Add(dgvPhieu);

            dgvHoaDon = NewGrid();
            split.Panel2.Controls.Add(dgvHoaDon);
            lblTong = new Label { Dock = DockStyle.Bottom, Height = 56, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(6) };
            split.Panel2.Controls.Add(lblTong);
            page.Controls.Add(split);

            var bar = Bar(DockStyle.Top);
            cboLoc = new ComboBox { Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLoc.Items.Add("Tất cả");
            cboLoc.Items.AddRange(TrangThaiPhieu.All);
            cboLoc.SelectedIndex = 0;
            cboLoc.SelectedIndexChanged += (s, e) => { if (!loading) Run(LoadPhieu); };
            bar.Controls.AddRange(new Control[]
            {
                Lbl("Trạng thái:"), cboLoc,
                Btn("Làm mới", (s, e) => Run(LoadPhieu)),
                Btn("💳 Thanh toán phiếu đã chọn", (s, e) => Run(ThanhToan)),
                Btn("🖨 Xuất hóa đơn (.txt)", (s, e) => Run(XuatHoaDon))
            });
            page.Controls.Add(bar);
            tabs.TabPages.Add(page);
        }

        private void LoadPhieu()
        {
            string tt = cboLoc.SelectedIndex <= 0 ? null : (string)cboLoc.SelectedItem;
            loading = true;
            try
            {
                dgvPhieu.DataSource = phieuBus.GetAll(tt);
                HideCols(dgvPhieu, "NoiDungSua", "DienThoai", "HieuXe");
                Money(dgvPhieu, "TongTien");
            }
            finally { loading = false; }
            HienChiTietPhieu();
        }

        private void HienChiTietPhieu()
        {
            if (!(dgvPhieu.CurrentRow?.DataBoundItem is PhieuSuaChuaDTO p))
            {
                hoaDonHienTai = null; dgvHoaDon.DataSource = null; lblTong.Text = ""; return;
            }
            hoaDonHienTai = phieuBus.TaoHoaDon(p.MaPhieu);
            dgvHoaDon.DataSource = hoaDonHienTai.ChiTiet;
            HideCols(dgvHoaDon, "MaPhieu", "MaPhuTung");
            Money(dgvHoaDon, "DonGia", "TienCong", "TienPhuTung", "ThanhTien");

            lblTong.Text =
                $"Phiếu #{p.MaPhieu} - {p.BienSo} - {p.TenKH} [{p.TrangThai}]\n" +
                $"Tiền phụ tùng: {hoaDonHienTai.TongTienPhuTung:N0} đ  +  Tiền công: {hoaDonHienTai.TongTienCong:N0} đ  =  " +
                $"TỔNG CHI PHÍ: {hoaDonHienTai.TongChiPhi:N0} đ";
        }

        private void ThanhToan()
        {
            if (!(dgvPhieu.CurrentRow?.DataBoundItem is PhieuSuaChuaDTO p)) throw new InvalidOperationException("Hãy chọn một phiếu.");
            var hd = phieuBus.TaoHoaDon(p.MaPhieu);
            if (MessageBox.Show($"Thanh toán phiếu #{p.MaPhieu} với tổng chi phí {hd.TongChiPhi:N0} đ?",
                    "Xác nhận thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            phieuBus.ThanhToan(p.MaPhieu);
            LoadPhieu();
            MessageBox.Show("Đã chuyển phiếu sang \"Đã thanh toán\". Bạn có thể bấm \"Xuất hóa đơn\".", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void XuatHoaDon()
        {
            if (hoaDonHienTai == null) throw new InvalidOperationException("Hãy chọn một phiếu.");
            using var dlg = new SaveFileDialog
            {
                Filter = "Text (*.txt)|*.txt",
                FileName = $"HoaDon_{hoaDonHienTai.Phieu.MaPhieu}_{hoaDonHienTai.Phieu.BienSo}.txt"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(dlg.FileName, PhieuSuaChuaBUS.XuatHoaDonText(hoaDonHienTai), System.Text.Encoding.UTF8);
            MessageBox.Show("Đã xuất hóa đơn.", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
