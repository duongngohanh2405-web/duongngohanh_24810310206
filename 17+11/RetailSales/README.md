# Bài 6.1 - Retail Sales System (C# WinForms, 3-Tier)

## Cách chạy
1. Mở SSMS, chạy file `Database/SalesDB.sql` (tạo CSDL SalesDB + dữ liệu mẫu).
2. Cài **.NET 8 SDK** (hoặc Visual Studio 2022 có workload ".NET desktop development").
3. Mở `RetailSales.csproj` bằng Visual Studio rồi bấm F5,
   hoặc trong thư mục project chạy: `dotnet run`
   (lần đầu cần Internet để tải gói NuGet Microsoft.Data.SqlClient).

## Đổi chuỗi kết nối
Sửa trong `DAL/DBHelper.cs` (hằng `DefaultConnStr`) hoặc đặt biến môi trường `RETAIL_CONN`.
- SQL Server Express: `Server=.\SQLEXPRESS;Database=SalesDB;Integrated Security=True;TrustServerCertificate=True;`
- LocalDB: `Server=(localdb)\MSSQLLocalDB;Database=SalesDB;Integrated Security=True;TrustServerCertificate=True;`

## Cấu trúc
- DTO/  : lớp dữ liệu
- DAL/  : truy cập CSDL (SqlParameter, SqlTransaction)
- BUS/  : nghiệp vụ, kiểm tra dữ liệu
- GUI/  : giao diện (tạo control bằng code, không cần Designer)
