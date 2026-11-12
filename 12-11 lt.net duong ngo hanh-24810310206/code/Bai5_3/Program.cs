using System;
using System.Windows.Forms;

namespace Bai5_3
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new frmSanPham());
        }
    }
}
