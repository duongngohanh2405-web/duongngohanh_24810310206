using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Bai5_2
{
    public partial class frmDichVu : Form
    {
        private readonly CultureInfo vi = new CultureInfo("vi-VN");

        // Dữ liệu: loại dịch vụ -> danh sách dịch vụ
        private readonly Dictionary<string, List<DichVu>> dsDichVu =
            new Dictionary<string, List<DichVu>>
            {
                ["Khám bệnh"] = new List<DichVu>
                {
                    new DichVu("Khám nội tổng quát", 150000),
                    new DichVu("Khám nhi", 120000),
                    new DichVu("Khám tai mũi họng", 130000),
                    new DichVu("Khám da liễu", 140000),
                    new DichVu("Khám mắt", 100000)
                },
                ["Xét nghiệm"] = new List<DichVu>
                {
                    new DichVu("Xét nghiệm máu tổng quát", 200000),
                    new DichVu("Xét nghiệm đường huyết", 80000),
                    new DichVu("Xét nghiệm nước tiểu", 90000),
                    new DichVu("Xét nghiệm mỡ máu", 180000)
                },
                ["Chụp X-Quang"] = new List<DichVu>
                {
                    new DichVu("X-Quang phổi", 250000),
                    new DichVu("X-Quang cột sống", 300000),
                    new DichVu("X-Quang xương khớp", 280000)
                },
                ["Vắc-xin"] = new List<DichVu>
                {
                    new DichVu("Vắc-xin cúm", 350000),
                    new DichVu("Vắc-xin viêm gan B", 280000),
                    new DichVu("Vắc-xin uốn ván", 150000),
                    new DichVu("Vắc-xin HPV", 1800000)
                }
            };

        public frmDichVu()
        {
            InitializeComponent();
        }

        private void frmDichVu_Load(object sender, EventArgs e)
        {
            cboCategory.Items.AddRange(dsDichVu.Keys.ToArray());
            cboCategory.SelectedIndex = 0;   // kích hoạt SelectedIndexChanged
            TinhTien();
        }

        // 1. Đổi loại dịch vụ -> nạp danh sách dịch vụ tương ứng
        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstAvailableServices.Items.Clear();
            if (cboCategory.SelectedItem == null) return;

            string loai = cboCategory.SelectedItem.ToString();
            foreach (DichVu dv in dsDichVu[loai])
                lstAvailableServices.Items.Add(dv);
        }

        // Chuyển một dịch vụ sang danh sách đã chọn
        private void ChonDichVu(DichVu dv)
        {
            if (dv == null) return;

            if (lstSelectedServices.Items.Contains(dv))
            {
                MessageBox.Show("Dịch vụ \"" + dv.Ten + "\" đã được chọn rồi!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            lstSelectedServices.Items.Add(dv);
            TinhTien();
        }

        // 2. Nút >
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (lstAvailableServices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dịch vụ ở danh sách bên trái!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Duyệt qua bản sao vì danh sách chọn có thể thay đổi
            foreach (DichVu dv in lstAvailableServices.SelectedItems.Cast<DichVu>().ToList())
                ChonDichVu(dv);
        }

        // 2. Double Click vào item ở lstAvailableServices
        private void lstAvailableServices_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lstAvailableServices.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
                ChonDichVu((DichVu)lstAvailableServices.Items[index]);
        }

        // Nút < : bỏ các dịch vụ đang chọn
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lstSelectedServices.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn dịch vụ cần bỏ ở danh sách bên phải!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            foreach (object item in lstSelectedServices.SelectedItems.Cast<object>().ToList())
                lstSelectedServices.Items.Remove(item);
            TinhTien();
        }

        // Nút << : xóa tất cả
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            lstSelectedServices.Items.Clear();
            TinhTien();
        }

        // 3. Tính lại tổng tiền, chiết khấu, thành tiền
        private void TinhTien()
        {
            decimal tong = 0;
            foreach (DichVu dv in lstSelectedServices.Items)
                tong += dv.DonGia;

            decimal tyLe = nudChietKhau.Value;                // %
            decimal thanhTien = tong * (100 - tyLe) / 100;

            txtTongTien.Text = tong.ToString("N0", vi) + " đ";
            txtThanhTien.Text = thanhTien.ToString("N0", vi) + " đ";
        }

        // Đổi tỷ lệ chiết khấu -> tính lại
        private void nudChietKhau_ValueChanged(object sender, EventArgs e)
        {
            TinhTien();
        }
    }
}
