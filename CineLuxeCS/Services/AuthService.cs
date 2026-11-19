using System;
using System.Text.RegularExpressions;
using CineLuxe.Data;
using CineLuxe.Models;

namespace CineLuxe.Services
{
    /// <summary>Use case: Đăng nhập (include Xác thực tài khoản + Xác định vai trò) và Đăng ký tài khoản.</summary>
    public static class AuthService
    {
        public static bool ValidPhone(string s) { return Regex.IsMatch(s, @"^0\d{9}$"); }
        public static bool ValidEmail(string s) { return Regex.IsMatch(s, @"^[\w.+-]+@[\w-]+(\.[\w-]+)+$"); }

        /// <summary>Trả về User nếu đúng tài khoản/mật khẩu và đúng vai trò; ném ArgumentException nếu sai.</summary>
        public static User Login(string account, string password, Role role)
        {
            account = account.Trim();
            if (account.Length == 0 || password.Length == 0)
                throw new ArgumentException("Vui lòng nhập tài khoản và mật khẩu.");
            foreach (User u in DataStore.Get().Users)
            {
                bool match = u.Phone == account || string.Equals(u.Email, account, StringComparison.OrdinalIgnoreCase);
                if (!match) continue;
                if (u.Password != password)                       // xác thực tài khoản
                    throw new ArgumentException("Mật khẩu không chính xác.");
                if (u.Role != role)                               // xác định vai trò
                    throw new ArgumentException("Tài khoản này không thuộc vai trò \"" + role.Label() + "\".");
                return u;
            }
            throw new ArgumentException("Tài khoản không tồn tại.");
        }

        public static User Register(string name, string phone, string email, string dob, string pass, string pass2)
        {
            name = name.Trim(); phone = phone.Trim(); email = email.Trim();
            if (name.Length == 0 || phone.Length == 0 || email.Length == 0 || pass.Length == 0)
                throw new ArgumentException("Vui lòng điền đầy đủ thông tin.");
            if (!ValidPhone(phone)) throw new ArgumentException("Số điện thoại phải gồm 10 số, bắt đầu bằng 0.");
            if (!ValidEmail(email)) throw new ArgumentException("Email không hợp lệ.");
            if (pass.Length < 6) throw new ArgumentException("Mật khẩu tối thiểu 6 ký tự.");
            if (pass != pass2) throw new ArgumentException("Mật khẩu nhập lại không khớp.");
            DataStore d = DataStore.Get();
            foreach (User x in d.Users)
            {
                if (x.Phone == phone) throw new ArgumentException("Số điện thoại đã được đăng ký.");
                if (string.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Email đã được đăng ký.");
            }
            User u = new User("U" + d.NextSeq(), name, phone, email, pass, Role.KHACH_HANG);
            u.Dob = dob.Trim();
            u.Points = 50;                                        // tặng +50 Pts khi đăng ký như trên giao diện
            d.Users.Add(u);
            d.Save();
            return u;
        }
    }
}
