using System;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace FORM_ĐẶT_HÀNG
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadMenu();

            button1.Click += button1_Click;
            button2.Click += button2_Click;
        }

        // Nạp danh sách món ăn có sẵn vào listBox1
        private void LoadMenu()
        {
            listBox1.Items.Add(new MonAn("Hamburger", 50000));
            listBox1.Items.Add(new MonAn("Pizza", 120000));
            listBox1.Items.Add(new MonAn("Gà Rán", 35000));
            listBox1.Items.Add(new MonAn("Pepsi", 15000));

            CapNhatTongTien();
        }

        // Nút ">" - chuyển món từ listBox1 sang listBox2
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn món ăn cần thêm!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MonAn mon = (MonAn)listBox1.SelectedItem;
            listBox2.Items.Add(mon);
            CapNhatTongTien();
        }

        // Nút "<" - xóa món khỏi listBox2
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn món ăn cần xóa!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listBox2.Items.Remove(listBox2.SelectedItem);
            CapNhatTongTien();
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Tính và cập nhật tổng tiền lên label1
        private void CapNhatTongTien()
        {
            double tongTien = 0;

            foreach (MonAn mon in listBox2.Items)
            {
                tongTien += mon.Gia;
            }

            label1.Text = $"Tổng tiền: {tongTien:N0} đ";
        }
    }

    public class MonAn
    {
        public string Ten { get; set; }
        public double Gia { get; set; }

        public MonAn(string ten, double gia)
        {
            Ten = ten;
            Gia = gia;
        }

        public override string ToString()
        {
            return $"{Ten} - {Gia:N0}đ";
        }
    }
}