using System.Globalization;

namespace Bai5_2
{
    // Lớp mô tả một dịch vụ: tên + đơn giá
    public class DichVu
    {
        public string Ten { get; set; }
        public decimal DonGia { get; set; }

        public DichVu(string ten, decimal donGia)
        {
            Ten = ten;
            DonGia = donGia;
        }

        // ListBox hiển thị kết quả của ToString()
        public override string ToString()
        {
            return Ten + " - " + DonGia.ToString("N0", new CultureInfo("vi-VN")) + " đ";
        }
    }
}
