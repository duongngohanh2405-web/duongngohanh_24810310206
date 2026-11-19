using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KaraokeManager.BUS;
using KaraokeManager.DTO;

namespace KaraokeManager.GUI
{
    public class FormMain : Form
    {
        private static readonly Color MauTrong = Color.FromArgb(46, 160, 90);     // xanh
        private static readonly Color MauDangHat = Color.FromArgb(205, 55, 55);   // đỏ

        private readonly PhongBUS phongBus = new PhongBUS();
        private readonly DichVuBUS dvBus = new DichVuBUS();
        private readonly ChiTietDichVuBUS ctBus = new ChiTietDichVuBUS();
        private readonly HoaDonBUS hdBus = new HoaDonBUS();

        // Sơ đồ phòng
        private FlowLayoutPanel pnlPhong;
        private readonly Dictionary<int, Button> btnPhong = new Dictionary<int, Button>();
        private List<PhongDTO> phongs = new List<PhongDTO>();
        private int selectedMaPhong;
        private TimeSpan serverOffset = TimeSpan.Zero;    // chênh lệch giờ máy so với SQL Server

        // Panel bên phải
        private Label lblInfo, lblTong;
        private Button btnMo, btnThanhToan, btnThem, btnLuu, btnXoa;
        private ComboBox cboDV;
        private NumericUpDown nudSL;
        private DataGridView dgvDV;
        private DataTable dtDV;                            // DataTable ngắt kết nối
        private readonly Timer timer = new Timer { Interval = 1000 };

        public FormMain()
        {
            Text = "Quản lý Karaoke";
            Size = new Size(1280, 760);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            // --- Khu sơ đồ phòng (Fill) - thêm đầu tiên
            pnlPhong = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10), BackColor = Color.WhiteSmoke };
            Controls.Add(pnlPhong);

            // --- Panel bên phải
            var right = new Panel { Dock = DockStyle.Right, Width = 540, Padding = new Padding(6) };

            dgvDV = new DataGridView
            {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            right.Controls.Add(dgvDV);

            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.TopDown, Padding = new Padding(4) };
            lblTong = new Label { AutoSize = true, Font = new Font(Font, FontStyle.Bold), ForeColor = Color.DarkRed, Margin = new Padding(4, 6, 4, 6) };
            btnThanhToan = Btn("💳 Thanh toán", (s, e) => Run(ThanhToan));
            btnThanhToan.Font = new Font(Font.FontFamily, 11f, FontStyle.Bold);
            bottom.Controls.AddRange(new Control[] { lblTong, btnThanhToan });
            right.Controls.Add(bottom);

            var rowSave = Bar();
            btnLuu = Btn("💾 Lưu thay đổi", (s, e) => Run(LuuThayDoi));
            btnXoa = Btn("Xóa dòng", (s, e) => Run(XoaDong));
            rowSave.Controls.AddRange(new Control[]
            {
                btnLuu, btnXoa,
                new Label { Text = "(sửa SL / xóa dòng chỉ ghi vào CSDL khi bấm Lưu)", AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(6, 9, 0, 0) }
            });
            right.Controls.Add(rowSave);

            var rowAdd = Bar();
            cboDV = new ComboBox { Width = 290, DropDownStyle = ComboBoxStyle.DropDownList };
            nudSL = new NumericUpDown { Width = 60, Minimum = 1, Maximum = 1000, Value = 1 };
            btnThem = Btn("+ Thêm", (s, e) => Run(ThemDichVu));
            rowAdd.Controls.AddRange(new Control[] { cboDV, nudSL, btnThem });
            right.Controls.Add(rowAdd);

            var rowBtn = Bar();
            btnMo = Btn("▶ Mở phòng", (s, e) => Run(MoPhong));
            btnMo.Font = new Font(Font.FontFamily, 11f, FontStyle.Bold);
            rowBtn.Controls.AddRange(new Control[] { btnMo, Btn("⟳ Làm mới", (s, e) => Run(LoadAll)) });
            right.Controls.Add(rowBtn);

            lblInfo = new Label { Dock = DockStyle.Top, Height = 92, Font = new Font(Font.FontFamily, 10.5f, FontStyle.Bold), Padding = new Padding(4) };
            right.Controls.Add(lblInfo);
            Controls.Add(right);

            // --- Chú thích màu (Top) - thêm cuối để nằm trên cùng
            var legend = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            legend.Controls.Add(new Label { Text = "  Trống  ", AutoSize = true, BackColor = MauTrong, ForeColor = Color.White, Margin = new Padding(4) });
            legend.Controls.Add(new Label { Text = "  Đang hát  ", AutoSize = true, BackColor = MauDangHat, ForeColor = Color.White, Margin = new Padding(4) });
            legend.Controls.Add(new Label { Text = "Chọn phòng rồi dùng các nút bên phải.", AutoSize = true, Margin = new Padding(12, 5, 0, 0) });
            Controls.Add(legend);

            timer.Tick += (s, e) => TickDongHo();
            Load += (s, e) => { Run(LoadAll); timer.Start(); };
            FormClosed += (s, e) => timer.Dispose();
        }

        // =====================================================================
        // Helpers
        // =====================================================================
        private void Run(Action a)
        {
            try { a(); }
            catch (KhoKhongDuException ex)
            {
                MessageBox.Show(ex.Message + "\nThao tác đã bị hủy (Rollback), dữ liệu không thay đổi.",
                    "Không đủ tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (ArgumentException ex) { MessageBox.Show(ex.Message, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (InvalidOperationException ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static FlowLayoutPanel Bar() => new FlowLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(2)
        };

        private static Button Btn(string t, EventHandler h)
        {
            var b = new Button { Text = t, AutoSize = true, Margin = new Padding(4), Padding = new Padding(6, 2, 6, 2) };
            b.Click += h;
            return b;
        }

        private DateTime Now() => DateTime.Now + serverOffset;

        private PhongDTO CurrentPhong() => phongs.FirstOrDefault(p => p.MaPhong == selectedMaPhong);

        private static string Fmt(TimeSpan t) => BangGia.DinhDangThoiGian(t);

        private decimal TongDichVuHienTai()
        {
            if (dtDV == null) return 0;
            object o = dtDV.Compute("SUM(ThanhTien)", "");
            return o == null || o == DBNull.Value ? 0 : Convert.ToDecimal(o);
        }

        // =====================================================================
        // SƠ ĐỒ PHÒNG (Visual Room Grid)
        // =====================================================================
        private void LoadAll()
        {
            serverOffset = phongBus.ThoiGianServer() - DateTime.Now;
            cboDV.DataSource = dvBus.GetAll();
            LoadPhong();
        }

        private void LoadPhong()
        {
            phongs = phongBus.GetAll();

            pnlPhong.SuspendLayout();
            foreach (var c in pnlPhong.Controls.Cast<Control>().ToList()) c.Dispose();
            pnlPhong.Controls.Clear();
            btnPhong.Clear();

            foreach (var p in phongs)
            {
                var b = new Button
                {
                    Width = 170, Height = 100, Margin = new Padding(8), Tag = p,
                    FlatStyle = FlatStyle.Flat, ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter
                };
                b.FlatAppearance.BorderColor = Color.RoyalBlue;
                b.Click += (s, e) => SelectPhong(((PhongDTO)((Button)s).Tag).MaPhong);
                btnPhong[p.MaPhong] = b;
                pnlPhong.Controls.Add(b);
            }
            pnlPhong.ResumeLayout();

            if (!phongs.Any(p => p.MaPhong == selectedMaPhong)) selectedMaPhong = 0;
            RefreshPhongButtons();
            SelectPhong(selectedMaPhong, reloadAlways: true);
        }

        private void RefreshPhongButtons()
        {
            foreach (var p in phongs)
            {
                var b = btnPhong[p.MaPhong];
                bool dangHat = p.TrangThai == TrangThaiPhong.DangHat;
                b.BackColor = dangHat ? MauDangHat : MauTrong;
                b.FlatAppearance.BorderSize = p.MaPhong == selectedMaPhong ? 5 : 0;

                string s = $"{p.TenPhong}\n{p.LoaiPhong} · {p.GiaTheoGio:N0}đ/giờ\n";
                s += dangHat && p.GioVao.HasValue ? "⏱ " + Fmt(Now() - p.GioVao.Value) : "Trống";
                if (b.Text != s) b.Text = s;
            }
        }

        private void SelectPhong(int maPhong, bool reloadAlways = false)
        {
            if (dtDV != null && dtDV.GetChanges() != null && !reloadAlways)
            {
                if (MessageBox.Show("Có thay đổi dịch vụ chưa lưu. Bỏ các thay đổi này?", "Chưa lưu",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            }

            selectedMaPhong = maPhong;
            RefreshPhongButtons();

            var p = CurrentPhong();
            dtDV = null;
            dgvDV.DataSource = null;
            if (p != null && p.MaHDHienTai.HasValue)
            {
                Run(() =>
                {
                    dtDV = ctBus.LayDanhSach(p.MaHDHienTai.Value);    // SqlDataAdapter.Fill -> DataTable
                    dgvDV.DataSource = dtDV;
                    CauHinhLuoiDichVu();
                });
            }
            CapNhatNut();
            CapNhatThongTin();
        }

        private void CauHinhLuoiDichVu()
        {
            foreach (DataGridViewColumn c in dgvDV.Columns) c.ReadOnly = true;
            dgvDV.Columns["SoLuong"].ReadOnly = false;                       // chỉ cho sửa số lượng
            dgvDV.Columns["MaHD"].Visible = false;
            dgvDV.Columns["MaDV"].Visible = false;
            dgvDV.Columns["TenDV"].HeaderText = "Dịch vụ";
            dgvDV.Columns["SoLuong"].HeaderText = "SL";
            dgvDV.Columns["DonGia"].HeaderText = "Đơn giá";
            dgvDV.Columns["ThanhTien"].HeaderText = "Thành tiền";
            dgvDV.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            dgvDV.Columns["ThanhTien"].DefaultCellStyle.Format = "N0";
            dgvDV.Columns["SoLuong"].DefaultCellStyle.BackColor = Color.LemonChiffon;
        }

        private void CapNhatNut()
        {
            var p = CurrentPhong();
            bool chon = p != null;
            bool dangHat = chon && p.TrangThai == TrangThaiPhong.DangHat;
            btnMo.Enabled = chon && !dangHat;
            btnThanhToan.Enabled = dangHat;
            btnThem.Enabled = btnLuu.Enabled = btnXoa.Enabled = cboDV.Enabled = nudSL.Enabled = dangHat;
        }

        private void CapNhatThongTin()
        {
            var p = CurrentPhong();
            if (p == null) { lblInfo.Text = "Hãy chọn một phòng ở sơ đồ bên trái."; lblTong.Text = ""; return; }

            if (p.TrangThai != TrangThaiPhong.DangHat || !p.GioVao.HasValue)
            {
                lblInfo.Text = $"{p.TenPhong} ({p.LoaiPhong}) - {p.GiaTheoGio:N0} đ/giờ\nPhòng đang TRỐNG. Bấm \"Mở phòng\" để bắt đầu tính giờ.";
                lblTong.Text = "";
                return;
            }

            var t = Now() - p.GioVao.Value;
            decimal tienPhong = BangGia.TinhTienPhong(p.GioVao.Value, Now(), p.GiaTheoGio);
            decimal tienDV = TongDichVuHienTai();
            lblInfo.Text = $"{p.TenPhong} ({p.LoaiPhong}) - {p.GiaTheoGio:N0} đ/giờ\n" +
                           $"Giờ vào: {p.GioVao:HH:mm:ss dd/MM}     Đã hát: {Fmt(t)}";
            lblTong.Text = $"Tạm tính: phòng {tienPhong:N0} + dịch vụ {tienDV:N0} = {tienPhong + tienDV:N0} đ";
        }

        private void TickDongHo()
        {
            if (phongs.Count == 0) return;
            RefreshPhongButtons();
            CapNhatThongTin();
        }

        // =====================================================================
        // THAO TÁC
        // =====================================================================
        private void MoPhong()
        {
            var p = CurrentPhong() ?? throw new InvalidOperationException("Hãy chọn một phòng.");
            if (p.TrangThai != TrangThaiPhong.Trong) throw new InvalidOperationException("Chỉ mở được phòng đang Trống.");
            phongBus.MoPhong(p.MaPhong);          // transaction: HoaDon mới + phòng -> "Đang hát"
            LoadPhong();
        }

        private void ThemDichVu()
        {
            var p = CurrentPhong();
            if (p?.MaHDHienTai == null) throw new InvalidOperationException("Hãy chọn phòng đang hát.");
            if (!(cboDV.SelectedItem is DichVuDTO dv)) throw new InvalidOperationException("Hãy chọn dịch vụ.");
            if (dtDV.GetChanges() != null)
                throw new InvalidOperationException("Đang có thay đổi chưa lưu trong danh sách. Hãy bấm \"Lưu thay đổi\" trước.");

            ctBus.ThemDichVu(p.MaHDHienTai.Value, dv.MaDV, (int)nudSL.Value);
            NapLaiDichVu(p);
        }

        private void NapLaiDichVu(PhongDTO p)
        {
            dtDV = ctBus.LayDanhSach(p.MaHDHienTai.Value);
            dgvDV.DataSource = dtDV;
            CauHinhLuoiDichVu();
            CapNhatThongTin();
        }

        private void XoaDong()
        {
            if (!(dgvDV.CurrentRow?.DataBoundItem is DataRowView drv)) throw new InvalidOperationException("Hãy chọn dòng cần xóa.");
            drv.Row.Delete();                      // chỉ đánh dấu Deleted trên DataTable, chưa ghi CSDL
            CapNhatThongTin();
        }

        private void LuuThayDoi()
        {
            var p = CurrentPhong();
            if (p?.MaHDHienTai == null || dtDV == null) return;

            dgvDV.EndEdit();
            BindingContext[dtDV].EndCurrentEdit();
            if (dtDV.GetChanges() == null) { MessageBox.Show("Không có thay đổi nào để lưu."); return; }

            try { ctBus.LuuThayDoi(dtDV); }        // SqlDataAdapter.Update
            finally { NapLaiDichVu(p); }           // luôn nạp lại để khớp CSDL (kể cả khi bị Rollback)
        }

        private void ThanhToan()
        {
            var p = CurrentPhong();
            if (p?.MaHDHienTai == null) throw new InvalidOperationException("Hãy chọn phòng đang hát.");

            dgvDV.EndEdit();
            BindingContext[dtDV].EndCurrentEdit();
            if (dtDV.GetChanges() != null)
                throw new InvalidOperationException("Đang có thay đổi dịch vụ chưa lưu. Hãy bấm \"Lưu thay đổi\" trước khi thanh toán.");

            var pre = hdBus.XemTruoc(p.MaHDHienTai.Value);
            string msg = $"{pre.TenPhong}\n" +
                         $"Giờ vào: {pre.GioVao:HH:mm:ss}   Giờ ra: {pre.GioRa:HH:mm:ss}\n" +
                         $"Thời gian hát: {Fmt(pre.ThoiGianHat)} ({pre.ThoiGianHat.TotalHours:0.00} giờ × {pre.GiaTheoGio:N0} đ)\n\n" +
                         $"Tiền phòng : {pre.TongTienPhong:N0} đ\n" +
                         $"Tiền dịch vụ: {pre.TongTienDichVu:N0} đ\n" +
                         $"TỔNG CỘNG  : {pre.TongCong:N0} đ\n\nXác nhận thanh toán?";
            if (MessageBox.Show(msg, "Thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            var hd = hdBus.ThanhToan(p.MaHDHienTai.Value);    // SqlTransaction: hóa đơn + trừ kho + phòng Trống
            Run(() => { cboDV.DataSource = dvBus.GetAll(); });
            LoadPhong();
            HienHoaDon(HoaDonBUS.XuatHoaDonText(hd), hd);
        }

        private void HienHoaDon(string text, HoaDonDTO hd)
        {
            using var f = new Form
            {
                Text = $"Hóa đơn #{hd.MaHD} - {hd.TenPhong}", Size = new Size(560, 520), StartPosition = FormStartPosition.CenterParent
            };
            var tb = new TextBox
            {
                Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Font = new Font("Consolas", 10f),
                Text = text.Replace("\r\n", "\n").Replace("\n", "\r\n"), ScrollBars = ScrollBars.Vertical
            };
            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
            var save = Btn("Lưu .txt", (s, e) =>
            {
                using var dlg = new SaveFileDialog { Filter = "Text (*.txt)|*.txt", FileName = $"HoaDon_{hd.MaHD}.txt" };
                if (dlg.ShowDialog() == DialogResult.OK) File.WriteAllText(dlg.FileName, text, System.Text.Encoding.UTF8);
            });
            var close = Btn("Đóng", (s, e) => f.Close());
            bar.Controls.AddRange(new Control[] { save, close });
            f.Controls.Add(tb);
            f.Controls.Add(bar);
            f.ShowDialog(this);
        }
    }
}
