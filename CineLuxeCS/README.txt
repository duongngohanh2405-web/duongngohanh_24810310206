CineLuxury - Hệ thống đặt vé xem phim (C# .NET Windows Forms)
==============================================================
Chuyển từ bản Java Swing sang C# .NET 8 Windows Forms. Giữ nguyên kiến trúc:
  Models/    Entities.cs, Seats.cs       (User, Movie, Room, Combo, Booking, Seat -> StandardSeat/VipSeat/SweetboxSeat, SeatMap, Showtime)
  Data/      DataStore.cs, DbStorage.cs  (lưu file .txt hoặc MySQL, dữ liệu mẫu)
  Services/  AuthService, BookingService, ScheduleService, StatsService
  UI/        LoginForm, CustomerForm (Home/Booking/MyTickets/Account), StaffForm, AdminForm (+ Movies/Schedule/Report), ...

YÊU CẦU
  - Windows + .NET 8 SDK (https://dotnet.microsoft.com/download) + Visual Studio 2022 (workload ".NET desktop development")
  - Mở file CineLuxe.csproj bằng Visual Studio -> bấm F5 (lần đầu sẽ tự tải gói MySqlConnector từ NuGet)
  - Hoặc dùng dòng lệnh:  dotnet run

LƯU DỮ LIỆU
  - Mặc định (db.properties: enabled=false): lưu file văn bản trong thư mục data/ cạnh file .exe. Xóa thư mục data/ để tạo lại dữ liệu mẫu.
  - Dùng MySQL: sửa db.properties -> enabled=true và điền mật khẩu root. Chương trình tự tạo database "cineluxe" và các bảng
    (cấu trúc bảng giống bản Java nên dùng chung được CSDL cũ).
  - Ảnh poster (tùy chọn): đặt file posters/<mã phim>.jpg, ví dụ posters/MOV-8821.jpg.

TÀI KHOẢN DÙNG THỬ (mật khẩu 123456)
  Khách hàng : 0908345678     Nhân viên : 0900000002     Quản lý : 0900000001
  Voucher thử: CINELUXE20, WELCOME50
