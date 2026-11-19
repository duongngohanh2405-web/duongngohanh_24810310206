using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CineLuxe.Data;
using CineLuxe.Models;

namespace CineLuxe.Services
{
    /// <summary>Use case: Quản lý phim, lịch chiếu.</summary>
    public static class ScheduleService
    {
        public const int CleaningMinutes = 15;

        public static List<Showtime> ForDay(DateTime day)
        {
            return DataStore.Get().Showtimes.Where(s => s.Start.Date == day.Date).OrderBy(s => s.Start).ToList();
        }

        public static List<Showtime> UpcomingOf(string movieId)
        {
            DateTime now = DateTime.Now;
            return DataStore.Get().Showtimes.Where(s => s.MovieId == movieId && s.End > now).OrderBy(s => s.Start).ToList();
        }

        /// <summary>Xếp lịch chiếu mới, kiểm tra xung đột phòng (có 15 phút dọn phòng).</summary>
        public static Showtime Add(string movieId, string roomId, DateTime start)
        {
            DataStore d = DataStore.Get();
            Movie m = d.GetMovie(movieId);
            DateTime end = start.AddMinutes(m.Duration);
            foreach (Showtime s in d.Showtimes)
            {
                if (s.RoomId != roomId) continue;
                bool overlap = start < s.End.AddMinutes(CleaningMinutes) && s.Start < end.AddMinutes(CleaningMinutes);
                if (overlap) throw new InvalidOperationException("Xung đột lịch với suất " + s.Start.ToString("HH:mm", CultureInfo.InvariantCulture)
                        + " - " + s.End.ToString("HH:mm", CultureInfo.InvariantCulture) + " (" + d.GetMovie(s.MovieId).Title + "). Cần giãn cách "
                        + CleaningMinutes + " phút dọn phòng.");
            }
            Showtime st = new Showtime("ST" + d.NextSeq(), movieId, roomId, start, end);
            d.Showtimes.Add(st);
            d.Save();
            return st;
        }

        public static void RemoveShowtime(Showtime st)
        {
            DataStore d = DataStore.Get();
            if (BookingService.SoldSeats(st.Id).Count > 0)
                throw new InvalidOperationException("Suất chiếu đã có vé bán, không thể xóa.");
            d.Showtimes.Remove(st);
            d.Save();
        }

        public static void RemoveMovie(Movie m)
        {
            DataStore d = DataStore.Get();
            foreach (Showtime s in d.Showtimes)
                if (s.MovieId == m.Id && s.End > DateTime.Now && BookingService.SoldSeats(s.Id).Count > 0)
                    throw new InvalidOperationException("Phim đang có suất chiếu đã bán vé, không thể xóa.");
            d.Showtimes.RemoveAll(s => s.MovieId == m.Id && s.End > DateTime.Now);
            d.Movies.Remove(m);
            d.Save();
        }
    }
}
