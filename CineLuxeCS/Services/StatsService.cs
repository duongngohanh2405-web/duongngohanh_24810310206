using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CineLuxe.Data;
using CineLuxe.Models;

namespace CineLuxe.Services
{
    /// <summary>Use case: Thống kê doanh thu vé (theo ngày/tháng, theo phim, trạng thái vé, hình thức thanh toán).</summary>
    public static class StatsService
    {
        public class Row
        {
            public readonly string Label; public readonly int Tickets; public readonly long Revenue;
            public Row(string label, int tickets, long revenue) { Label = label; Tickets = tickets; Revenue = revenue; }
        }

        private static bool Paid(Booking o) { return o.Status != BookingStatus.DA_HUY; }

        private static bool In(Booking o, DateTime from, DateTime to)
        {
            DateTime d = o.CreatedAt.Date;
            return d >= from.Date && d <= to.Date;
        }

        public static List<Row> ByDay(DateTime from, DateTime to)
        {
            SortedDictionary<DateTime, long[]> m = new SortedDictionary<DateTime, long[]>();
            for (DateTime d = from.Date; d <= to.Date; d = d.AddDays(1)) m[d] = new long[2];
            foreach (Booking o in DataStore.Get().Orders)
                if (Paid(o) && In(o, from, to)) { long[] a = m[o.CreatedAt.Date]; a[0] += o.Seats.Count; a[1] += o.Total; }
            return m.Select(kv => new Row(kv.Key.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), (int)kv.Value[0], kv.Value[1])).ToList();
        }

        public static List<Row> ByMonth(DateTime from, DateTime to)
        {
            SortedDictionary<DateTime, long[]> m = new SortedDictionary<DateTime, long[]>();
            DateTime end = new DateTime(to.Year, to.Month, 1);
            for (DateTime ym = new DateTime(from.Year, from.Month, 1); ym <= end; ym = ym.AddMonths(1)) m[ym] = new long[2];
            foreach (Booking o in DataStore.Get().Orders)
                if (Paid(o) && In(o, from, to))
                {
                    long[] a = m[new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1)];
                    a[0] += o.Seats.Count; a[1] += o.Total;
                }
            return m.Select(kv => new Row(kv.Key.ToString("MM/yyyy", CultureInfo.InvariantCulture), (int)kv.Value[0], kv.Value[1])).ToList();
        }

        public static List<Row> ByMovie(DateTime from, DateTime to)
        {
            DataStore d = DataStore.Get();
            Dictionary<string, long[]> m = new Dictionary<string, long[]>();
            foreach (Booking o in d.Orders)
            {
                if (!Paid(o) || !In(o, from, to)) continue;
                string title = d.GetMovie(d.GetShowtime(o.ShowtimeId).MovieId).Title;
                long[] a;
                if (!m.TryGetValue(title, out a)) { a = new long[2]; m[title] = a; }
                a[0] += o.Seats.Count; a[1] += o.Total;
            }
            return m.Select(kv => new Row(kv.Key, (int)kv.Value[0], kv.Value[1])).OrderByDescending(r => r.Revenue).ToList();
        }

        /// <summary>Trạng thái vé: đã bán (kèm đã soát) / đã hủy.</summary>
        public static List<Row> ByStatus(DateTime from, DateTime to)
        {
            int sold = 0, used = 0, cancelled = 0; long rs = 0, rc = 0;
            foreach (Booking o in DataStore.Get().Orders)
            {
                if (!In(o, from, to)) continue;
                if (o.Status == BookingStatus.DA_HUY) { cancelled += o.Seats.Count; rc += o.Total; }
                else
                {
                    sold += o.Seats.Count; rs += o.Total;
                    if (o.Status == BookingStatus.DA_SOAT) used += o.Seats.Count;
                }
            }
            return new List<Row> { new Row("Vé đã bán", sold, rs), new Row("   trong đó đã soát vé", used, 0), new Row("Vé đã hủy", cancelled, rc) };
        }

        public static List<Row> ByPayment(DateTime from, DateTime to)
        {
            Dictionary<string, long[]> m = new Dictionary<string, long[]>();
            foreach (Booking o in DataStore.Get().Orders)
            {
                if (!Paid(o) || !In(o, from, to)) continue;
                long[] a;
                if (!m.TryGetValue(o.Payment, out a)) { a = new long[2]; m[o.Payment] = a; }
                a[0]++; a[1] += o.Total;
            }
            // phổ biến nhất lên đầu (theo số đơn)
            return m.Select(kv => new Row(kv.Key, (int)kv.Value[0], kv.Value[1])).OrderByDescending(r => r.Tickets).ToList();
        }

        public static long Revenue(DateTime day)
        {
            return DataStore.Get().Orders.Where(o => Paid(o) && o.CreatedAt.Date == day.Date).Sum(o => o.Total);
        }

        public static int TicketsSold(DateTime day)
        {
            return DataStore.Get().Orders.Where(o => Paid(o) && o.CreatedAt.Date == day.Date).Sum(o => o.Seats.Count);
        }

        public static long FnbRevenue(DateTime day)
        {
            return DataStore.Get().Orders.Where(o => Paid(o) && o.CreatedAt.Date == day.Date).Sum(o => o.ComboTotal);
        }

        /// <summary>Tỷ lệ lấp đầy các suất chiếu trong ngày (0..1).</summary>
        public static double FillRate(DateTime day)
        {
            int seats = 0, cap = 0;
            foreach (Showtime st in DataStore.Get().Showtimes)
            {
                if (st.Start.Date != day.Date) continue;
                cap += SeatMap.Total();
                seats += BookingService.SoldSeats(st.Id).Count;
            }
            return cap == 0 ? 0 : (double)seats / cap;
        }
    }
}
