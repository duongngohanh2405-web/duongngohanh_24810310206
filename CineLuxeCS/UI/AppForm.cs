using System;
using System.Drawing;
using System.Windows.Forms;

namespace CineLuxe.UI
{
    /// <summary>Form gốc của các màn hình chính: đóng cửa sổ = thoát chương trình, Đăng xuất = quay lại màn hình đăng nhập.</summary>
    public class AppForm : Form
    {
        private bool loggingOut;

        public AppForm(string title, int width, int height)
        {
            Text = title;
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(width, height);
            BackColor = Theme.BG;
            Font = Theme.Fnt(FontStyle.Regular, 14);
            DoubleBuffered = true;
            FormClosed += (s, e) => { if (!loggingOut) Application.Exit(); };
        }

        public void Logout()
        {
            loggingOut = true;
            Program.ShowLogin();
            Close();
        }
    }
}
