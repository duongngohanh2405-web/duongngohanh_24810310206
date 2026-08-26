using System;

class Program
{
    static void Main()
    {
        int luaChon;

        do
        {
            HienThiMenu();
            luaChon = int.Parse(Console.ReadLine()!);

            switch (luaChon)
            {
                case 1:
                    Console.Clear();
                    BaiTap1_Calculator();
                    TamDung();
                    break;

                case 2:
                    Console.Clear();
                    BaiTap2_PhuongTrinhBac2();
                    TamDung();
                    break;

                case 3:
                    Console.Clear();
                    BaiTap3_NguyenToVaFibonacci();
                    TamDung();
                    break;

                case 0:
                    Console.WriteLine("Da thoat chuong trinh. Tam biet!");
                    break;

                default:
                    Console.WriteLine("Lua chon khong hop le! Nhan phim bat ky de tiep tuc...");
                    Console.ReadKey();
                    Console.Clear();
                    break;
            }

        } while (luaChon != 0);
    }

    static void HienThiMenu()
    {
        Console.WriteLine("===== MENU CHUONG TRINH =====");
        Console.WriteLine("1. Chay Bai tap 1 (Calculator)");
        Console.WriteLine("2. Chay Bai tap 2 (Phuong trinh bac 2)");
        Console.WriteLine("3. Chay Bai tap 3 (So nguyen to & Fibonacci)");
        Console.WriteLine("0. Thoat chuong trinh");
        Console.Write("Nhap lua chon cua ban: ");
    }

    static void TamDung()
    {
        Console.WriteLine("\nNhan phim bat ky de quay ve menu...");
        Console.ReadKey();
        Console.Clear();
    }

    // ---------- BAI TAP 1: CALCULATOR ----------
    static void BaiTap1_Calculator()
    {
        Console.Write("Nhap so thu nhat (a): ");
        double a = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap so thu hai (b): ");
        double b = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap phep toan (+, -, *, /, %): ");
        char op = char.Parse(Console.ReadLine()!);

        string ketQua = op switch
        {
            '+' => (a + b).ToString("F2"),
            '-' => (a - b).ToString("F2"),
            '*' => (a * b).ToString("F2"),
            '/' when b == 0 => "Loi: Khong the chia cho 0!",
            '/' => (a / b).ToString("F2"),
            '%' when b == 0 => "Loi: Khong the chia cho 0!",
            '%' => (a % b).ToString("F2"),
            _ => "Loi: Phep toan khong hop le!"
        };

        Console.WriteLine(ketQua);
    }

    // ---------- BAI TAP 2: PHUONG TRINH BAC 2 ----------
    static void BaiTap2_PhuongTrinhBac2()
    {
        Console.Write("Nhap he so a: ");
        double a = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap he so b: ");
        double b = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap he so c: ");
        double c = double.Parse(Console.ReadLine()!);

        if (a == 0)
        {
            if (b == 0)
            {
                Console.WriteLine(c == 0 ? "Phuong trinh vo so nghiem!" : "Phuong trinh vo nghiem!");
            }
            else
            {
                double x = -c / b;
                Console.WriteLine($"Phuong trinh bac nhat co nghiem: x = {x:F2}");
            }
        }
        else
        {
            double delta = b * b - 4 * a * c;

            if (delta > 0)
            {
                double x1 = (-b + Math.Sqrt(delta)) / (2 * a);
                double x2 = (-b - Math.Sqrt(delta)) / (2 * a);
                Console.WriteLine($"x1 = {x1:F2}, x2 = {x2:F2}");
            }
            else if (delta == 0)
            {
                double x = -b / (2 * a);
                Console.WriteLine($"Nghiem kep x = {x:F2}");
            }
            else
            {
                Console.WriteLine("Vo nghiem!");
            }
        }
    }

    // ---------- BAI TAP 3: SO NGUYEN TO & FIBONACCI ----------
    static void BaiTap3_NguyenToVaFibonacci()
    {
        Console.Write("Nhap so nguyen duong N: ");
        int n = int.Parse(Console.ReadLine()!);

        Console.WriteLine(IsPerfectNumber(n) ? $"{n} la So hoan hao!" : $"{n} KHONG la So hoan hao.");
        Console.WriteLine(IsPrime(n) ? $"{n} la So nguyen to." : $"{n} KHONG la So nguyen to.");

        Console.Write($"Day Fibonacci {n} so: ");
        PrintFibonacci(n);
    }

    static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

    static bool IsPerfectNumber(int n)
    {
        if (n < 1) return false;
        int sum = 0;
        for (int i = 1; i < n; i++)
        {
            if (n % i == 0) sum += i;
        }
        return sum == n;
    }

    static void PrintFibonacci(int n)
    {
        int a = 0, b = 1;
        for (int i = 0; i < n; i++)
        {
            Console.Write(a);
            if (i < n - 1) Console.Write(", ");
            int next = a + b;
            a = b;
            b = next;
        }
        Console.WriteLine();
    }
}
