using System;

class Program
{
    static void Main()
    {
        Console.Write("Nhap so thu nhat (a): ");
        double a = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap so thu hai (b): ");
        double b = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap phep toan (+, -, *, /, %): ");
        char op = char.Parse(Console.ReadLine()!);

        string result = Calculate(a, b, op);
        Console.WriteLine(result);
    }

    static string Calculate(double a, double b, char op)
    {
        return op switch
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
    }
}
