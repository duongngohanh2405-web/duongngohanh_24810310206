using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Màn hình "Đăng nhập &amp; Đăng ký tài khoản" (có bộ chọn vai trò 3 tab).</summary>
    public class LoginForm : Form
    {
        private Role role = Role.KHACH_HANG;
        private readonly RoundButton[] roleBtns = new RoundButton[3];
        private RoundButton tabLogin, tabRegister;
        private Stack loginForm, registerForm;

        private readonly Input txtAccount = Theme.Field(), txtPass = Theme.Password();
        private readonly CheckBox chkRemember = new CheckBox();
        private readonly Input rName = Theme.Field(), rPhone = Theme.Field(), rEmail = Theme.Field(), rDob = Theme.Field();
        private readonly Input rPass = Theme.Password(), rPass2 = Theme.Password();

        public LoginForm()
        {
            Text = "CineLuxury - Đăng nhập";
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1200, 760);
            Font = Theme.Fnt(FontStyle.Regular, 14);
            BackColor = Color.White;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill; root.ColumnCount = 2; root.RowCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(BuildBrandPanel(), 0, 0);
            root.Controls.Add(BuildFormPanel(), 1, 0);
            Controls.Add(root);
        }

        /// <summary>Đưa form về trạng thái ban đầu (gọi sau khi Đăng xuất).</summary>
        public void ResetForm()
        {
            txtPass.Text = "";
            SelectRole(Role.KHACH_HANG);
            ShowTab(true);
        }

        // ------------------------- CỘT TRÁI: thương hiệu -------------------------
        private class BrandPanel : Panel
        {
            public BrandPanel() { SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true); }
            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rc = new Rectangle(0, 0, Math.Max(1, Width), Math.Max(1, Height));
                using (LinearGradientBrush b = new LinearGradientBrush(rc, Color.FromArgb(60, 8, 24), Color.FromArgb(18, 18, 26), 55f))
                    g.FillRectangle(b, rc);
                using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 224, 24, 72))) g.FillEllipse(b, -120, -120, 420, 420);
            }
        }

        private Control BuildBrandPanel()
        {
            BrandPanel p = new BrandPanel();
            p.Dock = DockStyle.Fill; p.Padding = new Padding(44, 36, 44, 30);

            Stack top = new Stack();
            top.Add(Theme.Lbl("CineLuxury", FontStyle.Bold, 30, Color.White));
            top.Add(Theme.Lbl("Trải nghiệm điện ảnh đỉnh cao chuẩn Quốc tế", FontStyle.Regular, 13, Theme.OnDark(190)), 2);
            top.Add(Theme.Badge("Hơn 45+ cụm rạp toàn quốc", Color.White, Theme.OnDark(40)), 8, false);

            top.Add(Theme.Badge("TUẦN LỄ ĐẶC QUYỀN CINELUXE", Color.White, Theme.RUBY), 34, false);
            top.Add(Theme.Lbl("IMAX Laser & Dolby Atmos", FontStyle.Bold, 14, Theme.OnDark(200)), 10);
            top.Add(Theme.Lbl("Đặc Quyền Hội Viên\nCineClub VIP", FontStyle.Bold, 34, Color.White), 2);
            top.Add(Theme.Lbl("Khám phá thế giới điện ảnh không giới hạn cùng chuẩn dịch vụ 5 sao,\nđón đầu các bom tấn Hollywood và châu Á.",
                    FontStyle.Regular, 13, Theme.OnDark(190), 520), 8);

            System.Collections.Generic.List<Control> cards = new System.Collections.Generic.List<Control>();
            cards.Add(Benefit("Tích lũy 10% CineRewards", "Tích lũy tới 10% giá trị hóa đơn, nâng cấp thẻ VIP Gold & Diamond nhanh chóng."));
            cards.Add(Benefit("Đổi vé xem phim miễn phí", "Tự do đổi vé 2D/3D/IMAX Laser vào cuối tuần không phụ thu ngày lễ."));
            cards.Add(Benefit("Quà tặng sinh nhật", "Nhận ngay 02 vé xem phim miễn phí cùng combo bắp nước trong tháng sinh nhật."));
            cards.Add(Benefit("Ưu đãi 20% Combo Bắp Nước", "Thưởng thức bắp rang bơ Gourmet và đồ uống mát lạnh với giá ưu đãi."));
            top.Add(Lay.Grid(2, 12, 12, cards), 18);

            Stack bottom = new Stack();
            bottom.Dock = DockStyle.Bottom;
            RoundPanel quote = new RoundPanel(16, Color.FromArgb(30, 255, 255, 255), Color.FromArgb(50, 255, 255, 255));
            quote.Padding = new Padding(16, 14, 16, 14);
            Stack q = new Stack();
            q.Add(Theme.Lbl("Lê Hoàng Nam • VIP Diamond", FontStyle.Bold, 13, Color.White));
            q.Add(Theme.Lbl("\"Đặt ghế nhanh chỉ 30s, vé QR vào thẳng phòng chiếu không cần chờ in, chất lượng âm thanh IMAX cực đỉnh!\"",
                    FontStyle.Italic, 13, Theme.OnDark(210), 480), 4);
            q.Add(Theme.Lbl("128 suất chiếu đã xem • Thành viên 4 năm", FontStyle.Regular, 11, Theme.OnDark(150)), 4);
            quote.Controls.Add(q);
            bottom.Add(quote);
            bottom.Add(Theme.Lbl("Bảo mật SSL 256-bit  •  Chuẩn dữ liệu ISO 27001  •  Tổng đài CSKH: 1900 1234",
                    FontStyle.Regular, 11, Theme.OnDark(150), 520), 14);

            p.Controls.Add(top);
            p.Controls.Add(bottom);
            return p;
        }

        private Control Benefit(string title, string desc)
        {
            RoundPanel c = new RoundPanel(14, Color.FromArgb(28, 255, 255, 255), Color.Empty);
            c.Padding = new Padding(14, 12, 14, 12);
            Stack s = new Stack();
            s.Add(Theme.Lbl(title, FontStyle.Bold, 13, Color.White));
            s.Add(Theme.Lbl(desc, FontStyle.Regular, 11, Theme.OnDark(190), 200), 4);
            c.Controls.Add(s);
            return c;
        }

        // ------------------------- CỘT PHẢI: form -------------------------
        private Control BuildFormPanel()
        {
            Panel outer = new Panel();
            outer.Dock = DockStyle.Fill; outer.BackColor = Color.White; outer.AutoScroll = true;

            Stack p = new Stack();
            p.Dock = DockStyle.None; p.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            p.MinimumSize = new Size(440, 0); p.MaximumSize = new Size(440, 0);
            p.Location = new Point(40, 40);

            p.Add(Theme.Lbl("CỔNG DỊCH VỤ KHÁCH HÀNG 4.0", FontStyle.Bold, 11, Theme.RUBY));
            p.Add(Theme.Lbl("Chào mừng bạn trở lại!", FontStyle.Bold, 28, Theme.TEXT), 6);
            p.Add(Theme.Lbl("Đăng nhập tài khoản cá nhân để chọn ghế đẹp, tích điểm CineRewards và săn ưu đãi thành viên.",
                    FontStyle.Regular, 13, Theme.MUTED, 440), 2);

            // Bộ chọn vai trò
            p.Add(Theme.Lbl("VAI TRÒ TRUY CẬP HỆ THỐNG:", FontStyle.Bold, 11, Theme.MUTED), 16);
            Role[] all = (Role[])Enum.GetValues(typeof(Role));
            Control[] rb = new Control[3];
            for (int i = 0; i < 3; i++)
            {
                Role r = all[i];
                RoundButton b = new RoundButton(r.Label(), Color.White, Theme.MUTED, Theme.BORDER);
                b.Font = Theme.Fnt(FontStyle.Bold, 12);
                b.MinimumSize = new Size(0, 40);
                b.Click += (s, e) => SelectRole(r);
                roleBtns[i] = b; rb[i] = b;
            }
            p.Add(Lay.Grid(3, 8, rb), 6);

            // Tab Đăng nhập / Đăng ký
            tabLogin = Theme.Primary("Đăng nhập");
            tabRegister = Theme.Outline("Đăng ký mới  +50 Pts");
            tabLogin.MinimumSize = new Size(0, 40); tabRegister.MinimumSize = new Size(0, 40);
            tabLogin.Click += (s, e) => ShowTab(true);
            tabRegister.Click += (s, e) => ShowTab(false);
            p.Add(Lay.Grid(2, 8, new Control[] { tabLogin, tabRegister }), 14);

            loginForm = BuildLoginForm();
            registerForm = BuildRegisterForm();
            Panel formArea = new Panel();
            formArea.AutoSize = true; formArea.AutoSizeMode = AutoSizeMode.GrowAndShrink; formArea.BackColor = Color.Transparent;
            loginForm.Dock = DockStyle.Top; registerForm.Dock = DockStyle.Top;
            registerForm.Visible = false;
            formArea.Controls.Add(registerForm);
            formArea.Controls.Add(loginForm);
            p.Add(formArea, 14);

            outer.Controls.Add(p);
            outer.Resize += (s, e) => p.Left = Math.Max(10, (outer.ClientSize.Width - 440) / 2);
            SelectRole(Role.KHACH_HANG);
            return outer;
        }

        private void SelectRole(Role r)
        {
            role = r;
            Role[] all = (Role[])Enum.GetValues(typeof(Role));
            for (int i = 0; i < 3; i++)
            {
                bool on = all[i] == role;
                roleBtns[i].SetColors(on ? Theme.RUBY_SOFT : Color.White, on ? Theme.RUBY : Theme.MUTED, on ? Theme.RUBY : Theme.BORDER);
            }
            bool cus = role == Role.KHACH_HANG;
            tabRegister.Enabled = cus;
            if (!cus) ShowTab(true);
        }

        private void ShowTab(bool login)
        {
            loginForm.Visible = login;
            registerForm.Visible = !login;
            tabLogin.SetColors(login ? Theme.RUBY : Color.White, login ? Color.White : Theme.TEXT, login ? Color.Empty : Theme.BORDER);
            tabRegister.SetColors(!login ? Theme.RUBY : Color.White, !login ? Color.White : Theme.TEXT, !login ? Color.Empty : Theme.BORDER);
        }

        private Stack BuildLoginForm()
        {
            Stack f = new Stack();
            AddField(f, "Tài khoản", txtAccount, "Số điện thoại hoặc Email CineClub");
            AddField(f, "Mật khẩu", txtPass, "Nhập mật khẩu của bạn");

            chkRemember.Text = "Ghi nhớ đăng nhập trên thiết bị này (30 ngày)";
            chkRemember.AutoSize = true; chkRemember.ForeColor = Theme.MUTED; chkRemember.Font = Theme.Fnt(FontStyle.Regular, 12);
            chkRemember.BackColor = Color.Transparent;
            Button forgot = Theme.NavButton("Quên mật khẩu?");
            forgot.ForeColor = Theme.RUBY;
            forgot.Click += (s, e) => Theme.Info(this, "Vui lòng liên hệ tổng đài CSKH 1900 1234 để được cấp lại mật khẩu.");
            f.Add(Lay.Border(chkRemember, null, forgot), 2);

            RoundButton go = Theme.Primary("Đăng nhập vào CineLuxury");
            go.MinimumSize = new Size(0, 44);
            go.Click += (s, e) => DoLogin();
            txtPass.Submit += (s, e) => DoLogin();
            txtAccount.Submit += (s, e) => DoLogin();
            f.Add(go, 10);

            f.Add(Theme.Lbl("HOẶC TIẾP TỤC VỚI", FontStyle.Bold, 11, Theme.MUTED), 14);
            System.Collections.Generic.List<Control> social = new System.Collections.Generic.List<Control>();
            foreach (string name in new[] { "Google", "Apple ID", "Zalo", "Facebook" })
            {
                string n = name;
                RoundButton b = Theme.Outline(n);
                b.MinimumSize = new Size(0, 38);
                b.Click += (s, e) => Theme.Info(this, "Đăng nhập bằng " + n + " sẽ được hỗ trợ trong phiên bản sau.");
                social.Add(b);
            }
            f.Add(Lay.Grid(4, 8, 0, social), 8);

            f.Add(Theme.Lbl("Tài khoản dùng thử (mật khẩu 123456):", FontStyle.Bold, 11, Theme.MUTED), 16);
            f.Add(Theme.Lbl("Khách hàng: 0908345678  |  Nhân viên: 0900000002  |  Quản lý: 0900000001",
                    FontStyle.Regular, 11, Theme.MUTED, 440), 2);
            return f;
        }

        private Stack BuildRegisterForm()
        {
            Stack f = new Stack();
            AddField(f, "Họ và tên", rName, "Nguyễn Văn A");
            AddField(f, "Số điện thoại", rPhone, "0912345678");
            AddField(f, "Email", rEmail, "ten@email.com");
            AddField(f, "Ngày sinh", rDob, "dd/MM/yyyy (không bắt buộc)");
            AddField(f, "Mật khẩu", rPass, "Tối thiểu 6 ký tự");
            AddField(f, "Nhập lại mật khẩu", rPass2, "");
            RoundButton go = Theme.Primary("Đăng ký nhận quà");
            go.MinimumSize = new Size(0, 44);
            go.Click += (s, e) =>
            {
                try
                {
                    User u = AuthService.Register(rName.Text, rPhone.Text, rEmail.Text, rDob.Text, rPass.Text, rPass2.Text);
                    Theme.Info(this, "Đăng ký thành công! Bạn được tặng 50 điểm CineRewards.\nHãy đăng nhập để tiếp tục.");
                    txtAccount.Text = u.Phone; txtPass.Text = "";
                    ShowTab(true);
                }
                catch (ArgumentException ex) { Theme.Error(this, ex.Message); }
            };
            f.Add(go, 6);
            return f;
        }

        private void AddField(Stack parent, string caption, Input field, string hint)
        {
            parent.Add(Theme.Lbl(caption, FontStyle.Bold, 12, Theme.TEXT), 0);
            parent.Add(field, 4);
            if (hint.Length > 0) parent.Add(Theme.Lbl(hint, FontStyle.Regular, 11, Theme.MUTED), 2);
            parent.Add(new Panel { Height = 8, BackColor = Color.Transparent, Margin = Padding.Empty });
        }

        private void DoLogin()
        {
            try
            {
                User u = AuthService.Login(txtAccount.Text, txtPass.Text, role);
                Form next;
                switch (u.Role)
                {
                    case Role.KHACH_HANG: next = new CustomerForm(u); break;
                    case Role.NHAN_VIEN: next = new StaffForm(u); break;
                    default: next = new AdminForm(u); break;
                }
                Hide();
                next.Show();
            }
            catch (ArgumentException ex)
            {
                Theme.Error(this, ex.Message);
            }
        }
    }
}
