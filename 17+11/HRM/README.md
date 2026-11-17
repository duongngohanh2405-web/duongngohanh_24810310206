# Bài 6.2 - HRM & Payroll System (C# WinForms, 3-Tier)

## Cách chạy
1. Mở SSMS, chạy file `Database/HRMDB.sql` (tạo CSDL HRMDB + dữ liệu mẫu).
2. Cài **.NET 8 SDK** (hoặc Visual Studio 2022 có workload ".NET desktop development").
3. Mở `HRM.csproj` bằng Visual Studio rồi bấm F5, hoặc chạy `dotnet run` trong thư mục project
   (lần đầu cần Internet để tải gói NuGet Microsoft.Data.SqlClient).

## Kết nối SQL Server
Chương trình **tự dò** các server thường gặp: `.`, `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`, `localhost\SQLEXPRESS`.
Nếu tự dò không được, mở `connection.txt`, bỏ dấu `#` ở dòng cuối và sửa tên Server cho đúng với SSMS (chỉ 1 dấu `\`).
Hoặc đặt biến môi trường `HRM_CONN`.

## Cấu trúc
- DTO/ : lớp dữ liệu
- DAL/ : truy cập CSDL (SqlParameter, SqlDataAdapter + SqlCommandBuilder, SqlTransaction)
- BUS/ : nghiệp vụ, kiểm tra dữ liệu, công thức lương
- GUI/ : giao diện (tạo control bằng code, không cần Designer)

## Ghi chú thiết kế
- `BangChamCong` có thêm cột `DaKhoa` (bit) để khóa chấm công sau khi chốt lương, và ràng buộc UNIQUE (MaNV, ThangNam).
- `BangLuong`: khóa chính (MaNV, ThangNam) theo đề; `MaLuong` vẫn là cột IDENTITY (UNIQUE).
- Khi nạp một tháng chưa khóa, chương trình bổ sung dòng chấm công (0 công, 0 OT) cho nhân viên chưa có trong tháng đó.
- Thực lĩnh được làm tròn đến đồng.
- Số thập phân (ví dụ 0,5 ngày công) nhập theo định dạng của máy: vi-VN dùng dấu phẩy, en-US dùng dấu chấm.

## Gợi ý kiểm thử
1. Nhân viên: thêm / sửa / xóa, "Chuyển phòng ban", tìm theo tên, CCCD, tên phòng ban.
2. Chấm công: chọn 11/2026, nạp, sửa vài ô, Lưu, đóng mở lại để kiểm tra dữ liệu còn.
3. Tính lương: chọn 10/2026 (có dữ liệu mẫu), "Tính lương", rồi "Chốt lương tháng".
4. Mở lại Chấm công 10/2026: lưới chỉ xem, nút Lưu bị khóa. Thử chốt lần hai sẽ không cho.
   Ví dụ kiểm tra tay: NV1 lương 12.000.000, hệ số 1,2, 22 công, 4 giờ OT
   => 12.000.000 × 1,2 / 22 × 22 + 4 × 50.000 = 14.600.000.
