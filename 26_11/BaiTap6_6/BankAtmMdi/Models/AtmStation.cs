namespace BankAtmMdi.Models
{
    /// <summary>Thông tin một trạm ATM.</summary>
    public class AtmStation
    {
        public string MaTram { get; set; }
        public string DiaChi { get; set; }
        public decimal TonQuy { get; set; }
        public string TrangThai { get; set; }
        public string ImagePath { get; set; }

        // Dùng để hiển thị trên DataGridView
        public string AnhHienTruong =>
            string.IsNullOrEmpty(ImagePath) ? "(chưa có)" : Path.GetFileName(ImagePath);
    }
}
