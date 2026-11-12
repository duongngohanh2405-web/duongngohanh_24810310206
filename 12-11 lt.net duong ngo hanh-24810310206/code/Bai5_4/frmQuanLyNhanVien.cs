using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Bai5_4
{
    public partial class frmQuanLyNhanVien : Form
    {
        private const string TenCongTy = "Công ty ABC";

        // Chỉ số biểu tượng trong ImageList
        private const int ICON_CONGTY = 0;
        private const int ICON_PHONG = 1;
        private const int ICON_NHOM = 2;
        private const int ICON_NHANVIEN = 3;

        private readonly List<Employee> dsNhanVien = new List<Employee>();

        public frmQuanLyNhanVien()
        {
            InitializeComponent();
        }

        private void frmQuanLyNhanVien_Load(object sender, EventArgs e)
        {
            TaoImageList();
            NapDuLieuNhanVien();
            XayDungCay();

            // Các chế độ xem của ListView (tên trùng với giá trị enum View)
            cboView.Items.AddRange(new object[]
            {
                "Details", "SmallIcon", "LargeIcon", "Tile", "List"
            });
            lsvEmployees.TileSize = new Size(240, 46);
            cboView.SelectedIndex = 0;   // Details

            tvDepartments.SelectedNode = tvDepartments.Nodes[0];
        }

        // ---------- ImageList: vẽ biểu tượng bằng code, không cần file ảnh ----------
        private void TaoImageList()
        {
            imgSmall.ColorDepth = ColorDepth.Depth32Bit;
            imgSmall.ImageSize = new Size(16, 16);
            imgLarge.ColorDepth = ColorDepth.Depth32Bit;
            imgLarge.ImageSize = new Size(32, 32);

            for (int loai = 0; loai < 4; loai++)
            {
                imgSmall.Images.Add(TaoIcon(16, loai));
                imgLarge.Images.Add(TaoIcon(32, loai));
            }

            tvDepartments.ImageList = imgSmall;
            lsvEmployees.SmallImageList = imgSmall;
            lsvEmployees.LargeImageList = imgLarge;
        }

        private Bitmap TaoIcon(int size, int loai)
        {
            Bitmap bmp = new Bitmap(size, size);
            float u = size / 16f;   // đơn vị co giãn theo kích thước

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                switch (loai)
                {
                    case ICON_CONGTY:   // tòa nhà
                        g.FillRectangle(Brushes.SteelBlue, 2 * u, 1 * u, 12 * u, 14 * u);
                        for (int r = 0; r < 3; r++)
                            for (int c = 0; c < 2; c++)
                                g.FillRectangle(Brushes.White,
                                    (4 + c * 4.5f) * u, (3 + r * 3.2f) * u, 2.5f * u, 2 * u);
                        g.FillRectangle(Brushes.Navy, 6.5f * u, 12 * u, 3 * u, 3 * u);
                        break;

                    case ICON_PHONG:    // thư mục
                        g.FillRectangle(Brushes.DarkOrange, 1 * u, 3 * u, 6 * u, 3 * u);
                        g.FillRectangle(Brushes.Orange, 1 * u, 5 * u, 14 * u, 9 * u);
                        break;

                    case ICON_NHOM:     // hai người
                        g.FillEllipse(Brushes.SeaGreen, 2 * u, 3 * u, 5 * u, 5 * u);
                        g.FillEllipse(Brushes.SeaGreen, 1 * u, 8 * u, 7 * u, 7 * u);
                        g.FillEllipse(Brushes.MediumSeaGreen, 9 * u, 3 * u, 5 * u, 5 * u);
                        g.FillEllipse(Brushes.MediumSeaGreen, 8 * u, 8 * u, 7 * u, 7 * u);
                        break;

                    case ICON_NHANVIEN: // một người
                        g.FillEllipse(Brushes.SlateGray, 4.5f * u, 1 * u, 7 * u, 7 * u);
                        g.FillEllipse(Brushes.SlateGray, 2 * u, 9 * u, 12 * u, 10 * u);
                        break;
                }
            }
            return bmp;
        }

        // ---------- Dữ liệu mẫu ----------
        private void NapDuLieuNhanVien()
        {
            string kt = "Phòng Kỹ thuật", kd = "Phòng Kinh doanh", ns = "Phòng Nhân sự";

            dsNhanVien.AddRange(new[]
            {
                new Employee("NV001", "Nguyễn Văn An",   "Trưởng nhóm",          new DateTime(2019, 3, 15),  kt, "Nhóm Phát triển"),
                new Employee("NV002", "Trần Thị Bình",   "Lập trình viên",       new DateTime(2020, 7, 1),   kt, "Nhóm Phát triển"),
                new Employee("NV003", "Lê Hoàng Cường",  "Lập trình viên",       new DateTime(2021, 10, 20), kt, "Nhóm Phát triển"),
                new Employee("NV004", "Phạm Minh Đức",   "Trưởng nhóm",          new DateTime(2018, 5, 12),  kt, "Nhóm Kiểm thử"),
                new Employee("NV005", "Võ Thị Em",       "Kiểm thử viên",        new DateTime(2022, 2, 3),   kt, "Nhóm Kiểm thử"),
                new Employee("NV006", "Đặng Quốc Phong", "Trưởng nhóm",          new DateTime(2017, 8, 8),   kd, "Nhóm Bán hàng"),
                new Employee("NV007", "Bùi Thị Giang",   "Nhân viên bán hàng",   new DateTime(2020, 9, 14),  kd, "Nhóm Bán hàng"),
                new Employee("NV008", "Hoàng Văn Hải",   "Nhân viên bán hàng",   new DateTime(2022, 11, 25), kd, "Nhóm Bán hàng"),
                new Employee("NV009", "Ngô Thị Lan",     "Trưởng nhóm",          new DateTime(2019, 1, 30),  kd, "Nhóm Marketing"),
                new Employee("NV010", "Đỗ Văn Khánh",    "Chuyên viên Marketing", new DateTime(2021, 6, 17), kd, "Nhóm Marketing"),
                new Employee("NV011", "Lý Thị Mai",      "Trưởng nhóm",          new DateTime(2018, 4, 9),   ns, "Nhóm Tuyển dụng"),
                new Employee("NV012", "Trương Văn Nam",  "Chuyên viên tuyển dụng", new DateTime(2023, 8, 22), ns, "Nhóm Tuyển dụng"),
                new Employee("NV013", "Phan Thị Oanh",   "Trưởng nhóm",          new DateTime(2019, 11, 11), ns, "Nhóm Đào tạo"),
                new Employee("NV014", "Vũ Văn Phúc",     "Chuyên viên đào tạo",  new DateTime(2022, 3, 5),   ns, "Nhóm Đào tạo")
            });
        }

        // 1. Xây dựng cây: Công ty -> Phòng ban -> Nhóm
        private void XayDungCay()
        {
            var cauTruc = new Dictionary<string, string[]>
            {
                ["Phòng Kỹ thuật"]  = new[] { "Nhóm Phát triển", "Nhóm Kiểm thử" },
                ["Phòng Kinh doanh"] = new[] { "Nhóm Bán hàng", "Nhóm Marketing" },
                ["Phòng Nhân sự"]   = new[] { "Nhóm Tuyển dụng", "Nhóm Đào tạo" }
            };

            tvDepartments.Nodes.Clear();
            TreeNode goc = new TreeNode(TenCongTy, ICON_CONGTY, ICON_CONGTY);

            foreach (var phong in cauTruc)
            {
                TreeNode nodePhong = new TreeNode(phong.Key, ICON_PHONG, ICON_PHONG);
                foreach (string nhom in phong.Value)
                    nodePhong.Nodes.Add(new TreeNode(nhom, ICON_NHOM, ICON_NHOM));
                goc.Nodes.Add(nodePhong);
            }

            tvDepartments.Nodes.Add(goc);
            goc.ExpandAll();
        }

        // Đường dẫn đầy đủ của nhóm mà nhân viên thuộc về, cùng dạng với TreeNode.FullPath
        private string DuongDan(Employee nv)
        {
            string s = tvDepartments.PathSeparator;
            return TenCongTy + s + nv.Phong + s + nv.Nhom;
        }

        // 2. Chọn node -> lọc nhân viên thuộc node đó (và các node con)
        private void tvDepartments_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string duongDanNode = e.Node.FullPath;
            string tienTo = duongDanNode + tvDepartments.PathSeparator;

            var ketQua = dsNhanVien
                .Where(nv =>
                {
                    string p = DuongDan(nv);
                    return p == duongDanNode || p.StartsWith(tienTo);
                })
                .ToList();

            lsvEmployees.BeginUpdate();
            lsvEmployees.Items.Clear();
            foreach (Employee nv in ketQua)
            {
                ListViewItem item = new ListViewItem(nv.MaNV, ICON_NHANVIEN);
                item.SubItems.Add(nv.HoTen);
                item.SubItems.Add(nv.ChucVu);
                item.SubItems.Add(nv.NgayVaoLam.ToString("dd/MM/yyyy"));
                lsvEmployees.Items.Add(item);
            }
            lsvEmployees.EndUpdate();

            lblInfo.Text = e.Node.Text + ": " + ketQua.Count + " nhân viên";
        }

        // Đổi chế độ xem của ListView
        private void cboView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboView.SelectedItem == null) return;
            lsvEmployees.View = (View)Enum.Parse(typeof(View), cboView.SelectedItem.ToString());
        }
    }
}
