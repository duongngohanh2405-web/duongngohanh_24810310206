using System;

namespace AutoSpeed;

internal class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== HỆ THỐNG QUẢN LÝ PHƯƠNG TIỆN AUTOSPEED ===");

        Console.WriteLine("\nTC01 - Năm sản xuất không hợp lệ:");
        try
        {
            var test = new OTo("OT01", "Toyota", 1850, 1000000000m, 5, 2.0);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }

        var quanLy = new QuanLyPhuongTien();
        var oto = new OTo("OT01", "Toyota", 2024, 1000000000m, 5, 2.0);
        var xeMay = new XeMay("XM01", "Honda", 2023, 50000000m, 150);
        var oto9 = new OTo("OT02", "Ford", 2022, 800000000m, 7, 2.5);

        quanLy.AddPhuongTien(oto);
        quanLy.AddPhuongTien(xeMay);
        quanLy.AddPhuongTien(oto9);

        Console.WriteLine("\nTC02 - Ô tô 5 chỗ, giá gốc 1 tỷ:");
        Console.WriteLine($"Giá lăn bánh: {oto.TinhGiaLanBanh():N0} đồng");

        Console.WriteLine("\nTC03 - Xe máy 150cc, giá gốc 50 triệu:");
        Console.WriteLine($"Giá lăn bánh: {xeMay.TinhGiaLanBanh():N0} đồng");

        Console.WriteLine("\nTC04 - Đa hình:");
        quanLy.DisplayAll();

        Console.WriteLine("\nTC05 - Phương tiện có giá lăn bánh lớn nhất:");
        var max = quanLy.FindMaxGiaLanBanh();
        Console.WriteLine(max?.GetInfo());

        Console.WriteLine("\nTìm kiếm hãng chứa 'toy':");
        foreach (var pt in quanLy.SearchByName("toy"))
            Console.WriteLine(pt.GetInfo());

        Console.WriteLine("\nNhấn phím bất kỳ để kết thúc...");
        Console.ReadKey();
    }
}
