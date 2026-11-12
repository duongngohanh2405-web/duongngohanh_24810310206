namespace Bai5_3
{
    partial class frmSanPham
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpThongTin = new System.Windows.Forms.GroupBox();
            lblProductId = new System.Windows.Forms.Label();
            txtProductId = new System.Windows.Forms.TextBox();
            lblProductName = new System.Windows.Forms.Label();
            txtProductName = new System.Windows.Forms.TextBox();
            lblUnitPrice = new System.Windows.Forms.Label();
            txtUnitPrice = new System.Windows.Forms.TextBox();
            lblQuantity = new System.Windows.Forms.Label();
            nudQuantity = new System.Windows.Forms.NumericUpDown();
            lblCategory = new System.Windows.Forms.Label();
            cboCategory = new System.Windows.Forms.ComboBox();
            grpChucNang = new System.Windows.Forms.GroupBox();
            btnAdd = new System.Windows.Forms.Button();
            btnEdit = new System.Windows.Forms.Button();
            btnDelete = new System.Windows.Forms.Button();
            btnClear = new System.Windows.Forms.Button();
            lblSearch = new System.Windows.Forms.Label();
            txtSearch = new System.Windows.Forms.TextBox();
            btnSearch = new System.Windows.Forms.Button();
            btnShowAll = new System.Windows.Forms.Button();
            dgvProducts = new System.Windows.Forms.DataGridView();
            bsProducts = new System.Windows.Forms.BindingSource(components);
            grpThongTin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).BeginInit();
            grpChucNang.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProducts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)bsProducts).BeginInit();
            SuspendLayout();
            //
            // grpThongTin
            //
            grpThongTin.Controls.Add(lblProductId);
            grpThongTin.Controls.Add(txtProductId);
            grpThongTin.Controls.Add(lblProductName);
            grpThongTin.Controls.Add(txtProductName);
            grpThongTin.Controls.Add(lblUnitPrice);
            grpThongTin.Controls.Add(txtUnitPrice);
            grpThongTin.Controls.Add(lblQuantity);
            grpThongTin.Controls.Add(nudQuantity);
            grpThongTin.Controls.Add(lblCategory);
            grpThongTin.Controls.Add(cboCategory);
            grpThongTin.Location = new System.Drawing.Point(12, 12);
            grpThongTin.Name = "grpThongTin";
            grpThongTin.Size = new System.Drawing.Size(420, 230);
            grpThongTin.TabIndex = 0;
            grpThongTin.TabStop = false;
            grpThongTin.Text = "Thông tin sản phẩm";
            //
            // lblProductId
            //
            lblProductId.AutoSize = true;
            lblProductId.Location = new System.Drawing.Point(15, 33);
            lblProductId.Name = "lblProductId";
            lblProductId.Text = "Mã SP";
            //
            // txtProductId
            //
            txtProductId.Location = new System.Drawing.Point(130, 30);
            txtProductId.Name = "txtProductId";
            txtProductId.Size = new System.Drawing.Size(270, 27);
            txtProductId.TabIndex = 0;
            //
            // lblProductName
            //
            lblProductName.AutoSize = true;
            lblProductName.Location = new System.Drawing.Point(15, 73);
            lblProductName.Name = "lblProductName";
            lblProductName.Text = "Tên SP";
            //
            // txtProductName
            //
            txtProductName.Location = new System.Drawing.Point(130, 70);
            txtProductName.Name = "txtProductName";
            txtProductName.Size = new System.Drawing.Size(270, 27);
            txtProductName.TabIndex = 1;
            //
            // lblUnitPrice
            //
            lblUnitPrice.AutoSize = true;
            lblUnitPrice.Location = new System.Drawing.Point(15, 113);
            lblUnitPrice.Name = "lblUnitPrice";
            lblUnitPrice.Text = "Đơn giá";
            //
            // txtUnitPrice
            //
            txtUnitPrice.Location = new System.Drawing.Point(130, 110);
            txtUnitPrice.Name = "txtUnitPrice";
            txtUnitPrice.Size = new System.Drawing.Size(270, 27);
            txtUnitPrice.TabIndex = 2;
            //
            // lblQuantity
            //
            lblQuantity.AutoSize = true;
            lblQuantity.Location = new System.Drawing.Point(15, 153);
            lblQuantity.Name = "lblQuantity";
            lblQuantity.Text = "Số lượng";
            //
            // nudQuantity
            //
            nudQuantity.Location = new System.Drawing.Point(130, 150);
            nudQuantity.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            nudQuantity.Name = "nudQuantity";
            nudQuantity.Size = new System.Drawing.Size(120, 27);
            nudQuantity.TabIndex = 3;
            //
            // lblCategory
            //
            lblCategory.AutoSize = true;
            lblCategory.Location = new System.Drawing.Point(15, 193);
            lblCategory.Name = "lblCategory";
            lblCategory.Text = "Danh mục";
            //
            // cboCategory
            //
            cboCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCategory.FormattingEnabled = true;
            cboCategory.Location = new System.Drawing.Point(130, 190);
            cboCategory.Name = "cboCategory";
            cboCategory.Size = new System.Drawing.Size(270, 28);
            cboCategory.TabIndex = 4;
            //
            // grpChucNang
            //
            grpChucNang.Controls.Add(btnAdd);
            grpChucNang.Controls.Add(btnEdit);
            grpChucNang.Controls.Add(btnDelete);
            grpChucNang.Controls.Add(btnClear);
            grpChucNang.Controls.Add(lblSearch);
            grpChucNang.Controls.Add(txtSearch);
            grpChucNang.Controls.Add(btnSearch);
            grpChucNang.Controls.Add(btnShowAll);
            grpChucNang.Location = new System.Drawing.Point(445, 12);
            grpChucNang.Name = "grpChucNang";
            grpChucNang.Size = new System.Drawing.Size(430, 230);
            grpChucNang.TabIndex = 1;
            grpChucNang.TabStop = false;
            grpChucNang.Text = "Chức năng";
            //
            // btnAdd
            //
            btnAdd.Location = new System.Drawing.Point(15, 35);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(90, 40);
            btnAdd.TabIndex = 5;
            btnAdd.Text = "Thêm";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            //
            // btnEdit
            //
            btnEdit.Location = new System.Drawing.Point(115, 35);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new System.Drawing.Size(90, 40);
            btnEdit.TabIndex = 6;
            btnEdit.Text = "Sửa";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            //
            // btnDelete
            //
            btnDelete.Location = new System.Drawing.Point(215, 35);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new System.Drawing.Size(90, 40);
            btnDelete.TabIndex = 7;
            btnDelete.Text = "Xóa";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            //
            // btnClear
            //
            btnClear.Location = new System.Drawing.Point(315, 35);
            btnClear.Name = "btnClear";
            btnClear.Size = new System.Drawing.Size(100, 40);
            btnClear.TabIndex = 8;
            btnClear.Text = "Làm mới";
            btnClear.UseVisualStyleBackColor = true;
            btnClear.Click += btnClear_Click;
            //
            // lblSearch
            //
            lblSearch.AutoSize = true;
            lblSearch.Location = new System.Drawing.Point(15, 110);
            lblSearch.Name = "lblSearch";
            lblSearch.Text = "Tìm theo Tên SP:";
            //
            // txtSearch
            //
            txtSearch.Location = new System.Drawing.Point(15, 138);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new System.Drawing.Size(230, 27);
            txtSearch.TabIndex = 9;
            //
            // btnSearch
            //
            btnSearch.Location = new System.Drawing.Point(255, 134);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new System.Drawing.Size(85, 34);
            btnSearch.TabIndex = 10;
            btnSearch.Text = "Tìm kiếm";
            btnSearch.UseVisualStyleBackColor = true;
            btnSearch.Click += btnSearch_Click;
            //
            // btnShowAll
            //
            btnShowAll.Location = new System.Drawing.Point(348, 134);
            btnShowAll.Name = "btnShowAll";
            btnShowAll.Size = new System.Drawing.Size(70, 34);
            btnShowAll.TabIndex = 11;
            btnShowAll.Text = "Tất cả";
            btnShowAll.UseVisualStyleBackColor = true;
            btnShowAll.Click += btnShowAll_Click;
            //
            // dgvProducts
            //
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProducts.Location = new System.Drawing.Point(12, 255);
            dgvProducts.MultiSelect = false;
            dgvProducts.Name = "dgvProducts";
            dgvProducts.ReadOnly = true;
            dgvProducts.RowHeadersWidth = 51;
            dgvProducts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.Size = new System.Drawing.Size(863, 290);
            dgvProducts.TabIndex = 12;
            dgvProducts.CellClick += dgvProducts_CellClick;
            //
            // frmSanPham
            //
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(889, 560);
            Controls.Add(grpThongTin);
            Controls.Add(grpChucNang);
            Controls.Add(dgvProducts);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "frmSanPham";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Quản lý danh sách sản phẩm";
            Load += frmSanPham_Load;
            grpThongTin.ResumeLayout(false);
            grpThongTin.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).EndInit();
            grpChucNang.ResumeLayout(false);
            grpChucNang.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvProducts).EndInit();
            ((System.ComponentModel.ISupportInitialize)bsProducts).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.GroupBox grpThongTin;
        private System.Windows.Forms.Label lblProductId;
        private System.Windows.Forms.TextBox txtProductId;
        private System.Windows.Forms.Label lblProductName;
        private System.Windows.Forms.TextBox txtProductName;
        private System.Windows.Forms.Label lblUnitPrice;
        private System.Windows.Forms.TextBox txtUnitPrice;
        private System.Windows.Forms.Label lblQuantity;
        private System.Windows.Forms.NumericUpDown nudQuantity;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.ComboBox cboCategory;
        private System.Windows.Forms.GroupBox grpChucNang;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnShowAll;
        private System.Windows.Forms.DataGridView dgvProducts;
        private System.Windows.Forms.BindingSource bsProducts;
    }
}
