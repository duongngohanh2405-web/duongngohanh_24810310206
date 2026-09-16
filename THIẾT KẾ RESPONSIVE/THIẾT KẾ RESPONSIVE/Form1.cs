#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;

namespace THIẾT_KẾ_RESPONSIVE
{
    public partial class Form1 : Form
    {
        // Cột 1: các control nhập liệu
        private GroupBox gbInput;
        private Label lblMa, lblTen, lblSL, lblGia;
        private TextBox txtMa, txtTen, txtSL, txtGia;

        // Cột 2: danh sách + nút bấm
        private Panel pnlList;
        private ListView lvDanhSach;
        private Button btnThem, btnSua, btnXoa;

        private TableLayoutPanel tlpMain;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Bài tập 4.5 - Responsive Form với TableLayoutPanel";
            this.MinimumSize = new Size(650, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 📌 BƯỚC 1: TableLayoutPanel chính, Dock = Fill, chia 2 cột 30% - 70%
            tlpMain = new TableLayoutPanel();
            tlpMain.Dock = DockStyle.Fill;
            tlpMain.ColumnCount = 2;
            tlpMain.RowCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMain.Padding = new Padding(10);
            this.Controls.Add(tlpMain);

            // 📌 BƯỚC 2: Cột 1 (30%) - GroupBox chứa control nhập liệu
            gbInput = new GroupBox();
            gbInput.Text = "Thông tin nhập liệu";
            gbInput.Dock = DockStyle.Fill;

            lblMa = new Label { Text = "Mã:", Location = new Point(15, 30), AutoSize = true };
            txtMa = new TextBox { Location = new Point(15, 50), Width = 200 };
            txtMa.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblTen = new Label { Text = "Tên:", Location = new Point(15, 85), AutoSize = true };
            txtTen = new TextBox { Location = new Point(15, 105), Width = 200 };
            txtTen.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblSL = new Label { Text = "Số lượng:", Location = new Point(15, 140), AutoSize = true };
            txtSL = new TextBox { Location = new Point(15, 160), Width = 200 };
            txtSL.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            lblGia = new Label { Text = "Giá:", Location = new Point(15, 195), AutoSize = true };
            txtGia = new TextBox { Location = new Point(15, 215), Width = 200 };
            txtGia.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            gbInput.Controls.AddRange(new Control[] { lblMa, txtMa, lblTen, txtTen, lblSL, txtSL, lblGia, txtGia });
            tlpMain.Controls.Add(gbInput, 0, 0);

            // 📌 BƯỚC 3: Cột 2 (70%) - Panel chứa danh sách + nút bấm
            pnlList = new Panel();
            pnlList.Dock = DockStyle.Fill;

            lvDanhSach = new ListView();
            lvDanhSach.View = View.Details;
            lvDanhSach.Location = new Point(0, 0);
            lvDanhSach.Size = new Size(pnlList.Width, pnlList.Height - 45);
            lvDanhSach.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lvDanhSach.Columns.Add("Mã", 100);
            lvDanhSach.Columns.Add("Tên", 150);
            lvDanhSach.Columns.Add("Số lượng", 100);
            lvDanhSach.Columns.Add("Giá", 100);
            lvDanhSach.FullRowSelect = true;
            lvDanhSach.GridLines = true;

            // Nút bấm neo Bottom, Right => luôn bám góc dưới-phải khi Maximize
            btnThem = new Button { Text = "Thêm", Size = new Size(80, 30) };
            btnSua = new Button { Text = "Sửa", Size = new Size(80, 30) };
            btnXoa = new Button { Text = "Xóa", Size = new Size(80, 30) };

            btnThem.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnSua.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnXoa.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            pnlList.Controls.Add(lvDanhSach);
            pnlList.Controls.Add(btnThem);
            pnlList.Controls.Add(btnSua);
            pnlList.Controls.Add(btnXoa);

            tlpMain.Controls.Add(pnlList, 1, 0);

            // 📌 BƯỚC 4: Đặt vị trí nút theo kích thước panel lúc khởi tạo và mỗi khi Resize
            pnlList.Resize += (s, e) => DatViTriNut();
            this.Load += (s, e) => DatViTriNut();
        }

        private void DatViTriNut()
        {
            int w = pnlList.ClientSize.Width;
            int h = pnlList.ClientSize.Height;

            lvDanhSach.Size = new Size(w, h - 45);

            btnXoa.Location = new Point(w - btnXoa.Width - 10, h - 35);
            btnSua.Location = new Point(btnXoa.Left - btnSua.Width - 10, h - 35);
            btnThem.Location = new Point(btnSua.Left - btnThem.Width - 10, h - 35);
        }
    }
}