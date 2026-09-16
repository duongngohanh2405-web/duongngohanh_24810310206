using System;
using System.Windows.Forms;

namespace FORM_CALCULATOR
{
    public partial class Form1 : Form
    {
        private double soThuNhat = 0;
        private string phepToan = "";
        private bool batDauSoMoi = true;

        public Form1()
        {
            InitializeComponent();

            // Các nút Designer CHƯA gán Click -> gán ở đây
            button4.Click += NumberButton_Click;
            button7.Click += NumberButton_Click;
            button11.Click += (s, e) => ChonPhepToan("+");
            button12.Click += (s, e) => ChonPhepToan("-");
            button13.Click += (s, e) => ChonPhepToan("*");
            button15.Click += btnClear_Click;
            button16.Click += btnEquals_Click;
        }

        // Các hàm Designer ĐÃ tự gán sẵn -> phải tồn tại đúng tên này
        private void button1_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button2_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button3_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button5_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button6_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button8_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button9_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button10_Click(object sender, EventArgs e) => NumberButton_Click(sender, e);
        private void button14_Click(object sender, EventArgs e) => ChonPhepToan("/");

        private void NumberButton_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (batDauSoMoi)
            {
                textBox1.Text = btn.Text;
                batDauSoMoi = false;
            }
            else
            {
                textBox1.Text += btn.Text;
            }
        }

        private void ChonPhepToan(string toan)
        {
            soThuNhat = double.Parse(textBox1.Text);
            phepToan = toan;
            batDauSoMoi = true;
        }

        private void btnEquals_Click(object sender, EventArgs e)
        {
            double soThuHai = double.Parse(textBox1.Text);
            double ketQua = 0;

            switch (phepToan)
            {
                case "+": ketQua = soThuNhat + soThuHai; break;
                case "-": ketQua = soThuNhat - soThuHai; break;
                case "*": ketQua = soThuNhat * soThuHai; break;
                case "/":
                    if (soThuHai == 0)
                    {
                        MessageBox.Show("Không thể chia cho 0!", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    ketQua = soThuNhat / soThuHai;
                    break;
            }

            textBox1.Text = ketQua.ToString();
            batDauSoMoi = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            textBox1.Text = "0";
            soThuNhat = 0;
            phepToan = "";
            batDauSoMoi = true;
        }
    }
}