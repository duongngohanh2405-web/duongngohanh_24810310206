using System;
using System.Globalization;

namespace CineLuxe.UI
{
    /// <summary>Định dạng ngày giờ dùng chung.</summary>
    public static class Fmt
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        public const string CINEMA = "CineLuxury Landmark 81";

        public static string Date(DateTime d) { return d.ToString("dd/MM/yyyy", Inv); }
        public static string Time(DateTime d) { return d.ToString("HH:mm", Inv); }
        public static string DateTimeStr(DateTime d) { return d.ToString("HH:mm dd/MM/yyyy", Inv); }
        public static string Range(DateTime a, DateTime b) { return Time(a) + " - " + Time(b); }
        public static string Short(DateTime d) { return d.ToString("dd/MM", Inv); }
        public static string Clock() { return DateTime.Now.ToString("HH:mm:ss", Inv); }

        /// <summary>Đọc ngày dd/MM/yyyy; ném ArgumentException với thông báo tiếng Việt nếu sai.</summary>
        public static DateTime ParseDate(string s)
        {
            DateTime d;
            if (!DateTime.TryParseExact(s.Trim(), "dd/MM/yyyy", Inv, DateTimeStyles.None, out d))
                throw new ArgumentException("Ngày không hợp lệ: \"" + s + "\" (định dạng dd/MM/yyyy).");
            return d.Date;
        }

        public static TimeSpan ParseTime(string s)
        {
            DateTime d;
            if (!DateTime.TryParseExact(s.Trim(), new[] { "HH:mm", "H:mm" }, Inv, DateTimeStyles.None, out d))
                throw new ArgumentException("Giờ không hợp lệ: \"" + s + "\" (định dạng HH:mm).");
            return d.TimeOfDay;
        }

        public static string Day(DateTime d)
        {
            DateTime t = DateTime.Today;
            if (d.Date == t) return "Hôm nay, " + Short(d);
            if (d.Date == t.AddDays(1)) return "Ngày mai, " + Short(d);
            return Date(d);
        }
    }
}
