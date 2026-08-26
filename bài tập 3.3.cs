using System;

class Program
{
    static void Main()
    {
        Console.Write("Nhap so nguyen duong N: ");
        int n = int.Parse(Console.ReadLine()!);

        // Kiem tra so hoan hao
        if (IsPerfectNumber(n))
            Console.WriteLine($"{n} la So hoan hao!");
        else
            Console.WriteLine($"{n} KHONG la So hoan hao.");

        // Kiem tra so nguyen to
        if (IsPrime(n))
            Console.WriteLine($"{n} la So nguyen to.");
        else
            Console.WriteLine($"{n} KHONG la So nguyen to.");

        // In day Fibonacci N so dau tien
        Console.Write($"Day Fibonacci {n} so: ");
        PrintFibonacci(n);
    }

    static bool IsPrime(int n)
    {
        if (n < 2) return false;

        for (int i = 2; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0)
                return false;
        }
        return true;
    }

    static bool IsPerfectNumber(int n)
    {
        if (n < 1) return false;

        int sum = 0;
        for (int i = 1; i < n; i++)
        {
            if (n % i == 0)
                sum += i;
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
