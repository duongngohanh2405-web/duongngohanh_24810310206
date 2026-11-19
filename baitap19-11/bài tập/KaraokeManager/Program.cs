using System;
using System.Windows.Forms;
using KaraokeManager.GUI;

namespace KaraokeManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());
        }
    }
}
