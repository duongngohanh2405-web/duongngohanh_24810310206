using System;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.UI;

namespace CineLuxe
{
    static class Program
    {
        private static LoginForm login;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DataStore.Get();                       // nạp / tạo dữ liệu mẫu
            login = new LoginForm();
            Application.Run(login);
        }

        /// <summary>Hiện lại màn hình đăng nhập (sau khi Đăng xuất).</summary>
        public static void ShowLogin()
        {
            login.ResetForm();
            login.Show();
            login.Activate();
        }
    }
}
