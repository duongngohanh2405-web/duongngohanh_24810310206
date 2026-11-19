using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Màn hình "Quản lý tài khoản": thẻ hội viên, hồ sơ cá nhân, đổi mật khẩu.</summary>
    public class AccountPanel : Panel
    {
        private readonly CustomerForm frame;
        private readonly User user;
        private readonly Stack body = new Stack();
        // các ô nhập được tạo mới mỗi lần vẽ lại màn hình
        private Input fName, fDob, fPhone, fEmail, pOld, pNew, pNew2;

        public AccountPanel(CustomerForm frame, User user)
        {
            this.frame = frame; this.user = user;
            Dock = DockStyle.Fill; BackColor = Theme.BG;
            Controls.Add(Lay.Scroll(Lay.Pad(body, 32, 20, 32, 32)));
        }

        public void RefreshView()
        {
            body.SuspendLayout();
            body.Reset();
            body.Add(Theme.Lbl("Trang chủ  /  Tài khoản cá nhân  /  Cài đặt & Hồ sơ", FontStyle.Regular, 12, Theme.MUTED));
            body.Add(Lay.Row(10, Theme.Lbl("Quản Lý Tài Khoản", FontStyle.Bold, 26, Theme.TEXT),
                    Theme.Badge("VIP " + user.Tier().ToUpperInvariant() + " MEMBER", Theme.RUBY, Theme.RUBY_SOFT)), 4, false);
            body.Add(VipPass(), 14);
            body.Add(ProfileCard(), 16);
            body.Add(PasswordCard(), 16);
            body.ResumeLayout(true);
        }

        private class Bar : Control
        {
            public double Ratio;
            public Bar() { SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true); BackColor = Color.Transparent; Size = new Size(360, 8); }
            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, Width, 8), 8)) g.FillPath(b, p);
                int w = (int)(Width * Ratio);
                if (w > 2)
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(255, 215, 120)))
                    using (GraphicsPath p = Theme.Rounded(new RectangleF(0, 0, w, 8), 8)) g.FillPath(b, p);
            }
        }

        private void AddAt(Control parent, Control c, int x, int y)
        {
            c.Location = new Point(x, y);
            parent.Controls.Add(c);
        }

        private Control VipPass()
        {
            GradCard pass = new GradCard(Color.FromArgb(40, 10, 20), Color.FromArgb(160, 20, 60));
            pass.Arc = 26; pass.Circle = true;
            pass.MinimumSize = new Size(0, 200); pass.MaximumSize = new Size(0, 200);

            string num = "CL - " + Math.Abs(Theme.StableHash(user.Id) % 10000).ToString("0000") + " - "
                    + Math.Abs(Theme.StableHash(user.Phone) % 10000).ToString("0000") + " - 002";
            Color dim = Theme.OnDark(160), soft = Theme.OnDark(190), gold = Color.FromArgb(255, 215, 120);
            AddAt(pass, Theme.Lbl("THẺ HỘI VIÊN ĐIỆN TỬ • CineClub VIP Pass", FontStyle.Bold, 11, soft), 28, 20);
            AddAt(pass, Theme.Lbl("PREMIUM CINEMA CLUB", FontStyle.Bold, 12, gold), 28, 50);
            AddAt(pass, Theme.Lbl("CHỦ THẺ HỘI VIÊN", FontStyle.Regular, 10, dim), 28, 88);
            AddAt(pass, Theme.Lbl(user.FullName.ToUpperInvariant(), FontStyle.Bold, 22, Color.White), 28, 102);
            AddAt(pass, Theme.Lbl(num, FontStyle.Bold, 14, Theme.OnDark(210)), 28, 142);
            AddAt(pass, Theme.Lbl("CẤP BẬC", FontStyle.Regular, 10, dim), 420, 88);
            AddAt(pass, Theme.Lbl(user.Tier(), FontStyle.Bold, 20, gold), 420, 102);
            AddAt(pass, Theme.Lbl("TÍCH ĐIỂM", FontStyle.Regular, 10, dim), 600, 88);
            AddAt(pass, Theme.Lbl(user.Points.ToString("N0", Theme.Inv).Replace(',', '.') + " Pts", FontStyle.Bold, 20, Color.White), 600, 102);
            AddAt(pass, Theme.Lbl("ĐIỂM QUY ĐỔI", FontStyle.Regular, 10, dim), 420, 140);
            AddAt(pass, Theme.Lbl(Theme.Money((long)user.Points * BookingService.PointValue), FontStyle.Bold, 16, Color.White), 420, 154);

            // thanh tiến độ lên hạng
            int next = user.NextTierPoints();
            Bar bar = new Bar();
            bar.Ratio = next == 0 ? 1 : Math.Min(1.0, (double)user.Points / next);
            AddAt(pass, bar, 28, 176);
            string note = user.Points >= next ? "Bạn đã đạt hạng cao nhất" : "Tích lũy thêm " + (next - user.Points) + " điểm để lên hạng kế tiếp";
            AddAt(pass, Theme.Lbl(note, FontStyle.Regular, 11, soft), 400, 175);
            return pass;
        }

        private Control Labeled(string cap, Control f)
        {
            Stack p = new Stack();
            p.Dock = DockStyle.Fill;
            p.Add(Theme.Lbl(cap, FontStyle.Bold, 12, Theme.MUTED));
            p.Add(f, 4);
            return p;
        }

        private Control ProfileCard()
        {
            fName = Theme.Field(); fDob = Theme.Field(); fPhone = Theme.Field(); fEmail = Theme.Field();
            fName.Text = user.FullName; fDob.Text = user.Dob; fPhone.Text = user.Phone; fEmail.Text = user.Email;
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Theme.Lbl("Thông tin định danh & liên hệ", FontStyle.Bold, 17, Theme.TEXT));
            s.Add(Lay.Grid(2, 16, 12, new Control[] {
                Labeled("Họ và tên khách hàng", fName), Labeled("Ngày sinh (dd/MM/yyyy)", fDob),
                Labeled("Số điện thoại di động", fPhone), Labeled("Địa chỉ Email", fEmail) }), 12);
            RoundButton cancel = Theme.Outline("Hủy bỏ"), save = Theme.Primary("Lưu thay đổi hồ sơ");
            cancel.Click += (e, a) => RefreshView();
            save.Click += (e, a) => SaveProfile();
            s.Add(Lay.Border(null, null, Lay.Row(8, cancel, save)), 12);
            c.Controls.Add(s);
            return c;
        }

        private Control PasswordCard()
        {
            pOld = Theme.Password(); pNew = Theme.Password(); pNew2 = Theme.Password();
            RoundPanel c = Theme.Card();
            Stack s = new Stack();
            s.Add(Theme.Lbl("Bảo mật: Đổi mật khẩu", FontStyle.Bold, 17, Theme.TEXT));
            s.Add(Lay.Grid(3, 16, 0, new Control[] {
                Labeled("Mật khẩu hiện tại", pOld), Labeled("Mật khẩu mới", pNew), Labeled("Nhập lại mật khẩu mới", pNew2) }), 12);
            RoundButton ok = Theme.Primary("Đổi mật khẩu");
            ok.Click += (e, a) => ChangePassword();
            s.Add(Lay.Border(null, null, ok), 12);
            c.Controls.Add(s);
            return c;
        }

        private void SaveProfile()
        {
            string name = fName.Text.Trim(), phone = fPhone.Text.Trim(), email = fEmail.Text.Trim();
            if (name.Length == 0) { Theme.Error(this, "Họ tên không được để trống."); return; }
            if (!AuthService.ValidPhone(phone)) { Theme.Error(this, "Số điện thoại phải gồm 10 số, bắt đầu bằng 0."); return; }
            if (!AuthService.ValidEmail(email)) { Theme.Error(this, "Email không hợp lệ."); return; }
            foreach (User u in DataStore.Get().Users)
                if (u != user && (u.Phone == phone || string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
                {
                    Theme.Error(this, "Số điện thoại hoặc email đã được tài khoản khác sử dụng.");
                    return;
                }
            user.FullName = name; user.Phone = phone; user.Email = email; user.Dob = fDob.Text.Trim();
            DataStore.Get().Save();
            frame.RefreshUser();
            Theme.Info(this, "Đã lưu thay đổi hồ sơ.");
            RefreshView();
        }

        private void ChangePassword()
        {
            string o = pOld.Text, n = pNew.Text, n2 = pNew2.Text;
            if (user.Password != o) { Theme.Error(this, "Mật khẩu hiện tại không đúng."); return; }
            if (n.Length < 6) { Theme.Error(this, "Mật khẩu mới tối thiểu 6 ký tự."); return; }
            if (n != n2) { Theme.Error(this, "Mật khẩu nhập lại không khớp."); return; }
            user.Password = n;
            DataStore.Get().Save();
            Theme.Info(this, "Đổi mật khẩu thành công.");
            RefreshView();
        }
    }
}
