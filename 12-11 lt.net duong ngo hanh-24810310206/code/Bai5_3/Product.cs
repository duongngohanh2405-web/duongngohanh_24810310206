namespace Bai5_3
{
    // Lớp đối tượng sản phẩm
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }

        public Product() { }

        public Product(string id, string name, decimal price, int quantity, string category)
        {
            ProductId = id;
            ProductName = name;
            UnitPrice = price;
            Quantity = quantity;
            Category = category;
        }
    }
}
