using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RetailSales.BUS;
using RetailSales.DTO;

namespace RetailSales.GUI
{
    public class frmBanHang : Form
    {
        private readonly SanPhamBUS spBus = new SanPhamBUS();
        private readonly KhachHangBUS khBus = new KhachHangBUS();
        private readonly NhanVienBUS nvBus = new NhanVienBUS();
        private readonly HoaDonBUS hdBus = new HoaDonBUS();
        private readonly BindingList<ChiTietHoaDonDTO> gioHang = new BindingList<ChiTietHoaDonDTO>();

        private readonly ComboBox cboNV, cboKH, cboSP;
        private readonly TextBox txtMa;
        private readonly NumericUpDown numSL;
        private readonly DataGridView dgv;
        private readonly Label lblTong, lblDiem;
        private bool dangTai;

        public frmBanHang()
        {
            Text = "Bán hàng / Lập hóa đơn";
            Font = new Font("Segoe UI", 10F);
            ClientSize = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;

            // Giỏ hàng (thêm đầu tiên để Dock.Fill chiếm phần còn lại)
            dgv = Ui.NewGrid();
            dgv.AutoGenerateColumns = false;
            AddCol("MaSP", "Mã SP", null);
            AddCol("TenSP", "Tên sản phẩm", null);
            AddCol("SoLuong", "Số lượng", "N0");
            AddCol("DonGia", "Đơn giá", "N0");
            AddCol("ThanhTien", "Thành tiền", "N0");
            dgv.DataSource = gioHang;
            Controls.Add(dgv);

            // Thanh dưới: tổng tiền + thanh toán
            var pnlDuoi = new Panel { Dock = DockStyle.Bottom, Height = 75 };
            Controls.Add(pnlDuoi);
            lblTong = new Label
            {
                Left = 10, Top = 8, AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Text = "Tổng tiền: 0 đ"
            };
            pnlDuoi.Controls.Add(lblTong);
            lblDiem = new Label { Left = 12, Top = 45, AutoSize = true, Text = "Điểm tích lũy sẽ cộng: 0" };
            pnlDuoi.Controls.Add(lblDiem);
            var btnThanhToan = Ui.AddButton(pnlDuoi, "THANH TOÁN", 790, 12, 190, (s, e) => ThanhToan());
            btnThanhToan.Height = 50;
            btnThanhToan.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnThanhToan.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            // Thanh trên: chọn nhân viên, khách hàng, sản phẩm
            var pnlTren = new Panel { Dock = DockStyle.Top, Height = 95 };
            Controls.Add(pnlTren);

            Ui.AddLabel(pnlTren, "Nhân viên", 10, 10);
            cboNV = new ComboBox { Left = 90, Top = 10, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlTren.Controls.Add(cboNV);

            Ui.AddLabel(pnlTren, "Khách hàng", 320, 10);
            cboKH = new ComboBox { Left = 410, Top = 10, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlTren.Controls.Add(cboKH);

            Ui.AddLabel(pnlTren, "Sản phẩm", 10, 52);
            cboSP = new ComboBox { Left = 90, Top = 52, Width = 380, DropDownStyle = ComboBoxStyle.DropDownList };
            pnlTren.Controls.Add(cboSP);

            Ui.AddLabel(pnlTren, "Mã/quét", 490, 52);
            txtMa = Ui.AddText(pnlTren, 560, 52, 130);
            txtMa.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { ThemVaoGio(); e.SuppressKeyPress = true; }
            };

            Ui.AddLabel(pnlTren, "SL", 705, 52);
            numSL = new NumericUpDown { Left = 735, Top = 52, Width = 60, Minimum = 1, Maximum = 9999, Value = 1 };
            pnlTren.Controls.Add(numSL);

            Ui.AddButton(pnlTren, "Thêm", 810, 49, 80, (s, e) => ThemVaoGio());
            Ui.AddButton(pnlTren, "Xóa dòng", 895, 49, 90, (s, e) => XoaDong());

            Load += frmBanHang_Load;
        }

        private void AddCol(string prop, string header, string format)
        {
            var c = new DataGridViewTextBoxColumn { DataPropertyName = prop, HeaderText = header, Name = prop };
            if (format != null)
            {
                c.DefaultCellStyle.Format = format;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            dgv.Columns.Add(c);
        }

        private void frmBanHang_Load(object sender, EventArgs e)
        {
            try
            {
                // Nhân viên
                cboNV.DisplayMember = "TenNV";
                cboNV.ValueMember = "MaNV";
                cboNV.DataSource = nvBus.GetAll();

                // Khách hàng: dòng đầu là "Khách lẻ" (MaKH = 0 => lưu NULL)
                DataTable dtKH = khBus.GetAll();
                DataRow le = dtKH.NewRow();
                le["MaKH"] = 0;
                le["TenKH"] = "Khách lẻ";
                le["DienThoai"] = "";
                le["DiemTichLuy"] = 0;
                dtKH.Rows.InsertAt(le, 0);
                cboKH.DisplayMember = "TenKH";
                cboKH.ValueMember = "MaKH";
                cboKH.DataSource = dtKH;
                cboKH.SelectedIndex = 0;
                cboKH.SelectedIndexChanged += (s, ev) => CapNhatTong();

                TaiSanPham();
                cboSP.SelectedIndexChanged += (s, ev) =>
                {
                    if (dangTai) return;
                    var sp = cboSP.SelectedItem as SanPhamDTO;
                    if (sp != null) txtMa.Text = sp.MaSP;
                };

                CapNhatTong();
                txtMa.Focus();
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void TaiSanPham()
        {
            dangTai = true;
            cboSP.DisplayMember = "HienThi";
            cboSP.ValueMember = "MaSP";
            cboSP.DataSource = spBus.GetList();
            cboSP.SelectedIndex = -1;
            dangTai = false;
        }

        private int LayMaKH()
        {
            var drv = cboKH.SelectedItem as DataRowView;
            return drv == null ? 0 : Convert.ToInt32(drv["MaKH"]);
        }

        private int LayMaNV()
        {
            var drv = cboNV.SelectedItem as DataRowView;
            return drv == null ? 0 : Convert.ToInt32(drv["MaNV"]);
        }

        private void ThemVaoGio()
        {
            try
            {
                string ma = txtMa.Text.Trim();
                if (ma.Length == 0) { Ui.ShowInfo("Hãy chọn sản phẩm hoặc nhập/quét mã."); return; }

                SanPhamDTO sp = spBus.GetById(ma);
                if (sp == null) { Ui.ShowInfo("Không tìm thấy sản phẩm có mã: " + ma); return; }

                int sl = (int)numSL.Value;
                ChiTietHoaDonDTO dong = gioHang.FirstOrDefault(c => c.MaSP == sp.MaSP);
                int daCo = dong == null ? 0 : dong.SoLuong;

                if (daCo + sl > sp.SoLuongTon)
                {
                    MessageBox.Show("Tồn kho của \"" + sp.TenSP + "\" chỉ còn " + sp.SoLuongTon + ".",
                        "Không đủ hàng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (dong == null)
                {
                    gioHang.Add(new ChiTietHoaDonDTO
                    {
                        MaSP = sp.MaSP, TenSP = sp.TenSP, SoLuong = sl, DonGia = sp.GiaBan
                    });
                }
                else
                {
                    dong.SoLuong += sl;
                    gioHang.ResetBindings();
                }

                txtMa.Clear();
                numSL.Value = 1;
                txtMa.Focus();
                CapNhatTong();
            }
            catch (Exception ex) { Ui.ShowError(ex); }
        }

        private void XoaDong()
        {
            if (dgv.CurrentRow == null) return;
            int i = dgv.CurrentRow.Index;
            if (i >= 0 && i < gioHang.Count)
            {
                gioHang.RemoveAt(i);
                CapNhatTong();
            }
        }

        private void CapNhatTong()
        {
            decimal tong = gioHang.Sum(c => c.ThanhTien);
            lblTong.Text = "Tổng tiền: " + tong.ToString("N0") + " đ";
            int diem = LayMaKH() == 0 ? 0 : HoaDonBUS.TinhDiem(tong);
            lblDiem.Text = "Điểm tích lũy sẽ cộng: " + diem;
        }

        private void ThanhToan()
        {
            if (gioHang.Count == 0) { Ui.ShowInfo("Giỏ hàng đang trống."); return; }

            int maKH = LayMaKH();
            var hd = new HoaDonDTO
            {
                MaNV = LayMaNV(),
                MaKH = maKH == 0 ? (int?)null : maKH,
                ChiTiet = gioHang.ToList()
            };

            try
            {
                int maHD = hdBus.ThanhToan(hd);
                Ui.ShowInfo("Thanh toán thành công.\nMã hóa đơn: " + maHD +
                            "\nTổng tiền: " + hd.TongTien.ToString("N0") + " đ");
                gioHang.Clear();
                CapNhatTong();
                TaiSanPham();       // cập nhật lại tồn kho trong danh sách
            }
            catch (TonKhoException ex)
            {
                // Đã Rollback: không có hóa đơn nào được lưu
                MessageBox.Show(ex.Message + "\nGiao dịch đã được hoàn tác.", "Không thể thanh toán",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                TaiSanPham();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giao dịch thất bại và đã được hoàn tác.\n\n" + ErrorHelper.ToMessage(ex),
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
