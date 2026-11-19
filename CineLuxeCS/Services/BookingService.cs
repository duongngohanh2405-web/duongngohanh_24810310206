using System;
using System.Collections.Generic;
using System.Linq;
using CineLuxe.Data;
using CineLuxe.Models;

namespace CineLuxe.Services
{
    public enum ScanResult { HOP_LE, DA_SU_DUNG, DA_HUY, KHONG_TON_TAI, SAI_NGAY }

    /// <summary>Nghiệp vụ: Quản lý đặt vé, Quản lý đơn hàng, Xác thực vé.</summary>
    public static class BookingService
    {
        private static readonly Dictionary<string, int> Vouchers = new Dictionary<string, int>
        {
            { "CINELUXE20", 20000 }, { "WELCOME50", 50000 }
        };
        private static readonly object Gate = new object();
        public const int PointValue = 100;          // 1 điểm = 100đ
        public const int CancelBeforeHours = 2;     // được hủy trước giờ chiếu 2 tiếng

        public static int VoucherValue(string code)
        {
            int v;
            if (code == null || !Vouchers.TryGetValue(code.Trim().ToUpperInvariant(), out v)) return 0;
            return v;
        }

        /// <summary>Các ghế đã bán của một suất chiếu (đọc từ mảng ghế Seat[][] của Showtime).</summary>
        public static HashSet<string> SoldSeats(string showtimeId)
        {
            Showtime st = DataStore.Get().GetShowtime(showtimeId);
            return st == null ? new HashSet<string>() : new HashSet<string>(st.BookedSeats());
        }

        public static long ComboTotal(Dictionary<string, int> combos)
        {
            long t = 0;
            DataStore d = DataStore.Get();
            foreach (KeyValuePair<string, int> e in combos) t += (long)d.GetCombo(e.Key).Price * e.Value;
            return t;
        }

        /// <summary>Tạo đơn đặt vé. Ném InvalidOperationException nếu ghế đã bị người khác đặt.</summary>
        public static Booking Book(User u, Showtime st, List<string> seats, Dictionary<string, int> combos,
                                   string voucher, bool usePoints, string payment)
        {
            lock (Gate)
            {
                DataStore d = DataStore.Get();
                if (seats.Count == 0) throw new InvalidOperationException("Bạn chưa chọn ghế.");
                HashSet<string> sold = SoldSeats(st.Id);
                foreach (string s in seats)
                    if (sold.Contains(s)) throw new InvalidOperationException("Ghế " + s + " vừa có người đặt, vui lòng chọn ghế khác.");

                Booking o = new Booking();
                foreach (string s in seats) { o.Seats.Add(s); o.TicketTotal += SeatMap.PriceOf(s); }
                foreach (KeyValuePair<string, int> kv in combos) if (kv.Value > 0) o.Combos[kv.Key] = kv.Value;
                o.ComboTotal = ComboTotal(o.Combos);
                o.Voucher = voucher == null ? "" : voucher.Trim().ToUpperInvariant();
                long sub = o.TicketTotal + o.ComboTotal;
                o.Discount = Math.Min(VoucherValue(o.Voucher), sub);
                if (usePoints)
                {
                    o.PointsUsed = (int)Math.Min(u.Points, (sub - o.Discount) / PointValue);
                    o.Discount += (long)o.PointsUsed * PointValue;
                }
                o.Total = sub - o.Discount;
                o.PointsEarned = (int)(o.Total / 10000);
                int seq = d.NextSeq();
                o.Id = "CNL-" + (50000 + seq);
                o.CheckInCode = "CLX-" + (900000 + seq);
                o.UserId = u.Id; o.ShowtimeId = st.Id; o.Payment = payment;
                o.CreatedAt = DateTime.Now;
                o.Status = BookingStatus.DA_THANH_TOAN;
                foreach (string s in seats) st.Book(s);            // đánh dấu ghế đã đặt trong mảng 2 chiều
                u.Points += o.PointsEarned - o.PointsUsed;
                d.Orders.Add(o);
                d.Save();
                return o;
            }
        }

        public static List<Booking> OrdersOf(User u)
        {
            return DataStore.Get().Orders.Where(o => o.UserId == u.Id).OrderByDescending(o => o.CreatedAt).ToList();
        }

        public static bool CanCancel(Booking o)
        {
            Showtime st = DataStore.Get().GetShowtime(o.ShowtimeId);
            return o.Status == BookingStatus.DA_THANH_TOAN && DateTime.Now.AddHours(CancelBeforeHours) < st.Start;
        }

        /// <summary>Hủy vé: hoàn lại điểm đã dùng và thu hồi điểm đã tích.</summary>
        public static void Cancel(Booking o)
        {
            if (!CanCancel(o)) throw new InvalidOperationException(
                    "Chỉ được hủy vé chưa sử dụng, trước giờ chiếu tối thiểu " + CancelBeforeHours + " tiếng.");
            DataStore d = DataStore.Get();
            o.Status = BookingStatus.DA_HUY;
            Showtime st = d.GetShowtime(o.ShowtimeId);
            if (st != null) foreach (string seat in o.Seats) st.Release(seat);   // trả ghế về trống
            User u = d.GetUser(o.UserId);
            if (u != null) u.Points = Math.Max(0, u.Points + o.PointsUsed - o.PointsEarned);
            d.Save();
        }

        // ---------------- Xác thực vé (nhân viên) ----------------
        public static Booking FindByCode(string code)
        {
            if (code == null) return null;
            string c = code.Trim().ToUpperInvariant();
            foreach (Booking o in DataStore.Get().Orders)
                if (string.Equals(o.CheckInCode, c, StringComparison.OrdinalIgnoreCase) || string.Equals(o.Id, c, StringComparison.OrdinalIgnoreCase)) return o;
            return null;
        }

        public static ScanResult Check(Booking o)
        {
            if (o == null) return ScanResult.KHONG_TON_TAI;
            if (o.Status == BookingStatus.DA_HUY) return ScanResult.DA_HUY;
            if (o.Status == BookingStatus.DA_SOAT) return ScanResult.DA_SU_DUNG;
            Showtime st = DataStore.Get().GetShowtime(o.ShowtimeId);
            if (st.Start.Date != DateTime.Today) return ScanResult.SAI_NGAY;
            return ScanResult.HOP_LE;
        }

        public static void ConfirmEntry(Booking o, User staff)
        {
            o.Status = BookingStatus.DA_SOAT;
            o.ScannedAt = DateTime.Now;
            o.ScannedBy = staff.Code.Length == 0 ? staff.FullName : staff.Code;
            DataStore.Get().Save();
        }

        public static long MinutesUntil(Showtime st)
        {
            return (long)(st.Start - DateTime.Now).TotalMinutes;
        }
    }
}
