using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Bai5_3
{
    public partial class frmSanPham : Form
    {
        // Dữ liệu nguồn nằm trong bộ nhớ
        private readonly List<Product> dsSanPham = new List<Product>();

        public frmSanPham()
        {
            InitializeComponent();
        }

        private void frmSanPham_Load(object sender, EventArgs e)
        {
            // Danh mục
            cboCategory.Items.AddRange(new object[]
            {
                "Điện thoại", "Laptop", "Máy tính bảng", "Phụ kiện"
            });
            cboCategory.SelectedIndex = 0;

            // Dữ liệu mẫu
            dsSanPham.Add(new Product("SP01", "iPhone 15", 22000000, 10, "Điện thoại"));
            dsSanPham.Add(new Product("SP02", "Laptop Dell Inspiron", 16500000, 5, "Laptop"));
            dsSanPham.Add(new Product("SP03", "iPad Air", 15000000, 8, "Máy tính bảng"));
            dsSanPham.Add(new Product("SP04", "Tai nghe Bluetooth", 750000, 30, "Phụ kiện"));

            // Gắn BindingSource vào DataGridView
            dgvProducts.DataSource = bsProducts;
            LoadGrid();

            // Đặt tiêu đề cột tiếng Việt (cột chỉ có sau khi đã gắn dữ liệu)
            dgvProducts.Columns["ProductId"].HeaderText = "Mã SP";
            dgvProducts.Columns["ProductName"].HeaderText = "Tên SP";
            dgvProducts.Columns["UnitPrice"].HeaderText = "Đơn giá";
            dgvProducts.Columns["Quantity"].HeaderText = "Số lượng";
            dgvProducts.Columns["Category"].HeaderText = "Danh mục";
            dgvProducts.Columns["UnitPrice"].DefaultCellStyle.Format = "N0";
        }

        // Làm mới DataGridView (có áp dụng từ khóa tìm kiếm hiện tại)
        private void LoadGrid()
        {
            string tuKhoa = txtSearch.Text.Trim();

            List<Product> ds = string.IsNullOrEmpty(tuKhoa)
                ? dsSanPham
                : dsSanPham.Where(p => p.ProductName.IndexOf(
                      tuKhoa, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();

            bsProducts.DataSource = ds;
            bsProducts.ResetBindings(false);
        }

        private void ClearInputs()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            nudQuantity.Value = 0;
            cboCategory.SelectedIndex = 0;
            txtProductId.Focus();
        }

        // Đọc và kiểm tra dữ liệu từ các ô nhập; trả về null nếu không hợp lệ.
        // dangSua: sản phẩm đang sửa (bỏ qua khi kiểm tra trùng mã), null nếu là thêm mới.
        private Product LayTuForm(Product dangSua)
        {
            string ma = txtProductId.Text.Trim();
            string ten = txtProductName.Text.Trim();

            if (ma == "")
            {
                MessageBox.Show("Mã sản phẩm không được để trống!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductId.Focus();
                return null;
            }
            if (dsSanPham.Any(p => p != dangSua &&
                string.Equals(p.ProductId, ma, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã sản phẩm \"" + ma + "\" đã tồn tại!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductId.Focus();
                return null;
            }
            if (ten == "")
            {
                MessageBox.Show("Tên sản phẩm không được để trống!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProductName.Focus();
                return null;
            }
            if (!decimal.TryParse(txtUnitPrice.Text.Trim(),
                    NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                    CultureInfo.CurrentCulture, out decimal gia) || gia < 0)
            {
                MessageBox.Show("Đơn giá phải là số không âm!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUnitPrice.Focus();
                return null;
            }

            return new Product(ma, ten, gia, (int)nudQuantity.Value,
                               cboCategory.SelectedItem.ToString());
        }

        // 2. Nút Thêm
        private void btnAdd_Click(object sender, EventArgs e)
        {
            Product sp = LayTuForm(null);
            if (sp == null) return;

            dsSanPham.Add(sp);
            LoadGrid();
            ClearInputs();
        }

        // Nút Sửa: cập nhật sản phẩm đang chọn trên lưới
        private void btnEdit_Click(object sender, EventArgs e)
        {
            Product dangChon = bsProducts.Current as Product;
            if (dangChon == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần sửa trên danh sách!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Product moi = LayTuForm(dangChon);
            if (moi == null) return;

            dangChon.ProductId = moi.ProductId;
            dangChon.ProductName = moi.ProductName;
            dangChon.UnitPrice = moi.UnitPrice;
            dangChon.Quantity = moi.Quantity;
            dangChon.Category = moi.Category;

            LoadGrid();
            MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 4. Nút Xóa (có xác nhận)
        private void btnDelete_Click(object sender, EventArgs e)
        {
            Product dangChon = bsProducts.Current as Product;
            if (dangChon == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa trên danh sách!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult kq = MessageBox.Show(
                "Bạn có chắc muốn xóa sản phẩm \"" + dangChon.ProductName + "\" không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (kq == DialogResult.Yes)
            {
                dsSanPham.Remove(dangChon);
                LoadGrid();
                ClearInputs();
            }
        }

        // Nút Làm mới ô nhập
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        // Nút Tìm kiếm theo tên SP
        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadGrid();
            if (bsProducts.Count == 0)
            {
                MessageBox.Show("Không tìm thấy sản phẩm nào phù hợp!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Nút hiển thị tất cả
        private void btnShowAll_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            LoadGrid();
        }

        // 3. Click vào một dòng -> đẩy dữ liệu lên các ô nhập
        private void dgvProducts_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;   // bấm vào tiêu đề cột

            Product sp = dgvProducts.Rows[e.RowIndex].DataBoundItem as Product;
            if (sp == null) return;

            txtProductId.Text = sp.ProductId;
            txtProductName.Text = sp.ProductName;
            txtUnitPrice.Text = sp.UnitPrice.ToString("0.##");
            nudQuantity.Value = Math.Min(sp.Quantity, nudQuantity.Maximum);
            cboCategory.SelectedItem = sp.Category;
        }
    }
}
