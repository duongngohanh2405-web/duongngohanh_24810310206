using System;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("        THÔNG TIN MÔI TRƯỜNG THỰC THI");
        Console.WriteLine("==========================================");

        Console.WriteLine($"Phiên bản CLR/.NET : {Environment.Version}");
        Console.WriteLine($"Tên máy tính       : {Environment.MachineName}");
        Console.WriteLine($"Tên người dùng     : {Environment.UserName}");
        Console.WriteLine($"Hệ điều hành       : {Environment.OSVersion}");
        Console.WriteLine($"Hệ điều hành 64-bit: {Environment.Is64BitOperatingSystem}");
        Console.WriteLine($"Tiến trình 64-bit  : {Environment.Is64BitProcess}");

        Assembly assembly = Assembly.GetExecutingAssembly();

        Console.WriteLine($"Tên Assembly       : {assembly.GetName().Name}");
        Console.WriteLine($"Phiên bản Assembly : {assembly.GetName().Version}");

        long gcMemory = GC.GetTotalMemory(false);
        Console.WriteLine($"Bộ nhớ GC quản lý  : {gcMemory / (1024.0 * 1024.0):F2} MB");

        Console.WriteLine("==========================================");
        Console.WriteLine("Nhấn phím bất kỳ để kết thúc...");
        Console.ReadKey();
    }
}
vnamespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
}

using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Xin chao MaSV_HoVaTen!");
    }
}
