using System;

class Program
{
    static void Main()
    {
        Console.Write("Nhap he so a: ");
        double a = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap he so b: ");
        double b = double.Parse(Console.ReadLine()!);

        Console.Write("Nhap he so c: ");
        double c = double.Parse(Console.ReadLine()!);

        GiaiPhuongTrinh(a, b, c);
    }

    static void GiaiPhuongTrinh(double a, double b, double c)
    {
        if (a == 0)
        {
            // Phuong trinh bac nhat: bx + c = 0
            if (b == 0)
            {
                if (c == 0)
                    Console.WriteLine("Phuong trinh vo so nghiem!");
                else
                    Console.WriteLine("Phuong trinh vo nghiem!");
            }
            else
            {
                double x = -c / b;
                Console.WriteLine($"Phuong trinh bac nhat co nghiem: x = {x:F2}");
            }
        }
        else
        {
            // Phuong trinh bac 2: ax^2 + bx + c = 0
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
}
