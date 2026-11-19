using System;
using System.Collections.Generic;

namespace CineLuxe.Models
{
    // Tên enum giữ đúng như bản Java (KHACH_HANG...) để đọc được dữ liệu MySQL / file cũ.
    public enum Role { KHACH_HANG, NHAN_VIEN, QUAN_LY }
    public enum MovieStatus { DANG_CHIEU, SAP_CHIEU, NGUNG }
    public enum BookingStatus { DA_THANH_TOAN, DA_SOAT, DA_HUY }

    /// <summary>Nhãn tiếng Việt hiển thị cho các enum.</summary>
    public static class Labels
    {
        public static string Label(this Role r)
        {
            switch (r)
            {
                case Role.KHACH_HANG: return "Khách hàng";
                case Role.NHAN_VIEN: return "Nhân viên POS";
                default: return "Quản lý";
            }
        }

        public static string Label(this MovieStatus s)
        {
            switch (s)
            {
                case MovieStatus.DANG_CHIEU: return "Đang chiếu";
                case MovieStatus.SAP_CHIEU: return "Sắp chiếu";
                default: return "Ngừng chiếu";
            }
        }

        public static string Label(this BookingStatus s)
        {
            switch (s)
            {
                case BookingStatus.DA_THANH_TOAN: return "Đã thanh toán";
                case BookingStatus.DA_SOAT: return "Đã soát vé";
                default: return "Đã hủy";
            }
        }
    }

    public class User
    {
        public string Id, FullName, Phone, Email, Password, Dob = "", Code = "";
        public Role Role;
        public int Points;

        public User(string id, string fullName, string phone, string email, string password, Role role)
        {
            Id = id; FullName = fullName; Phone = phone; Email = email; Password = password; Role = role;
        }

        /// <summary>Hạng thành viên theo điểm CineRewards.</summary>
        public string Tier()
        {
            if (Points >= 3450) return "Platinum";
            if (Points >= 1000) return "Diamond";
            if (Points >= 500) return "Gold";
            return "Member";
        }

        /// <summary>Mốc điểm của hạng kế tiếp.</summary>
        public int NextTierPoints()
        {
            if (Points >= 3450) return Points;
            if (Points >= 1000) return 3450;
            if (Points >= 500) return 1000;
            return 500;
        }
    }

    public class Movie
    {
        public string Id, Title, Genre, Director, Age, Formats, Description;
        public int Duration;            // phút
        public double Imdb;
        public MovieStatus Status;
        public bool Vietnamese;
        public DateTime ReleaseDate;

        public Movie(string id, string title, string genre, string director, int duration, string age,
                     string formats, double imdb, MovieStatus status, DateTime releaseDate, string description)
        {
            Id = id; Title = title; Genre = genre; Director = director; Duration = duration; Age = age;
            Formats = formats; Imdb = imdb; Status = status; ReleaseDate = releaseDate.Date; Description = description;
        }

        public override string ToString() { return Title; }
    }

    public class Room
    {
        public string Id, Name, Format, Info;
        public Room(string id, string name, string format, string info)
        {
            Id = id; Name = name; Format = format; Info = info;
        }
        public override string ToString() { return Name + " • " + Format; }
    }

    public class Combo
    {
        public string Id, Name, Desc;
        public int Price;
        public Combo(string id, string name, string desc, int price)
        {
            Id = id; Name = name; Desc = desc; Price = price;
        }
    }

    public class Booking
    {
        public string Id, CheckInCode, UserId, ShowtimeId, Payment, Voucher = "", ScannedBy = "";
        public List<string> Seats = new List<string>();
        public Dictionary<string, int> Combos = new Dictionary<string, int>();   // comboId -> số lượng
        public long TicketTotal, ComboTotal, Discount, Total;
        public int PointsUsed, PointsEarned;
        public BookingStatus Status = BookingStatus.DA_THANH_TOAN;
        public DateTime CreatedAt;
        public DateTime? ScannedAt;
    }
}
