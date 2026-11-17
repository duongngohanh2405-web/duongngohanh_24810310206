namespace HRM.DTO
{
    public class LuongChiTietDTO
    {
        public int MaNV { get; set; }
        public string HoTen { get; set; }
        public decimal LuongCoBan { get; set; }
        public double HeSoLuong { get; set; }
        public double SoNgayCong { get; set; }
        public double SoGioOT { get; set; }
        public decimal ThucLinh { get; set; }
        public decimal? ThucLinhDaChot { get; set; }   // có giá trị khi tháng đã chốt
    }
}
