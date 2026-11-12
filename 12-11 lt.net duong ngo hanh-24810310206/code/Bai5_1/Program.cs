using System;
using System.Windows.Forms;

namespace Bai5_1
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmDangKy());
        }
    }
}
