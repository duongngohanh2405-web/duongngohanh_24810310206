using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FORM_ĐĂNG_KÝ
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            LoadCourses();
        }

        // Nạp danh sách khóa học vào ComboBox
        private void LoadCourses()
        {
            var courses = new List<Course>
            {
                new Course { MaKH = "IT01", TenKH = "Lập trình C#" },
                new Course { MaKH = "IT02", TenKH = "Cơ sở dữ liệu" },
                new Course { MaKH = "IT03", TenKH = "Lập trình Web" },
                new Course { MaKH = "IT04", TenKH = "Mạng máy tính" }
            };

            comboBox1.DataSource = courses;
            comboBox1.DisplayMember = "TenKH";
            comboBox1.ValueMember = "MaKH";
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {
            MessageBox.Show("Vui lòng chỉ nhập số cho đúng định dạng!", "Cảnh báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!checkBox1.Checked)
            {
                MessageBox.Show("Bạn cần đồng ý điều khoản trước khi đăng ký!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string gioiTinh = radioButton1.Checked ? "Nam" : (radioButton2.Checked ? "Nữ" : "Chưa chọn");

            string thongTin =
                $"Họ tên: {textBox1.Text}\n" +
                $"Giới tính: {gioiTinh}\n" +
                $"Ngày sinh: {dateTimePicker1.Value.ToShortDateString()}\n" +
                $"Số điện thoại: {maskedTextBox1.Text}\n" +
                $"Khóa học: {((Course)comboBox1.SelectedItem).TenKH}\n" +
                $"Mã khóa học: {comboBox1.SelectedValue}";

            MessageBox.Show(thongTin, "Thông tin đăng ký",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }

    public class Course
    {
        public string MaKH { get; set; }
        public string TenKH { get; set; }
    }
}