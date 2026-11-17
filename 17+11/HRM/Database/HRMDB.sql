-- ============================================================
-- Bài 6.2 - HRMDB (chạy toàn bộ script này trong SSMS)
-- ============================================================
IF DB_ID(N'HRMDB') IS NULL
    CREATE DATABASE HRMDB;
GO
USE HRMDB;
GO

IF OBJECT_ID(N'PhongBan', N'U') IS NULL
CREATE TABLE PhongBan (
    MaPB  INT IDENTITY(1,1) PRIMARY KEY,
    TenPB NVARCHAR(100) NOT NULL,
    Mota  NVARCHAR(255)
);

IF OBJECT_ID(N'NhanVien', N'U') IS NULL
CREATE TABLE NhanVien (
    MaNV        INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    NgaySinh    DATETIME NULL,
    CCCD        VARCHAR(20) NULL UNIQUE,
    MaPB        INT NOT NULL FOREIGN KEY REFERENCES PhongBan(MaPB),
    LuongCoBan  DECIMAL(18,2) NOT NULL CHECK (LuongCoBan > 0),
    HeSoLuong   FLOAT NOT NULL DEFAULT 1.0 CHECK (HeSoLuong > 0)
);

-- DaKhoa: cột bổ sung để "khóa" dữ liệu chấm công sau khi chốt lương
IF OBJECT_ID(N'BangChamCong', N'U') IS NULL
CREATE TABLE BangChamCong (
    MaCong     INT IDENTITY(1,1) PRIMARY KEY,
    MaNV       INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam   VARCHAR(7) NOT NULL,                       -- ví dụ '11/2026'
    SoNgayCong FLOAT NOT NULL DEFAULT 0 CHECK (SoNgayCong >= 0 AND SoNgayCong <= 31),
    SoGioOT    FLOAT NOT NULL DEFAULT 0 CHECK (SoGioOT >= 0),
    DaKhoa     BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ChamCong_NV_Thang UNIQUE (MaNV, ThangNam)
);

-- Khóa chính là (MaNV, ThangNam) theo đề; MaLuong vẫn là cột IDENTITY (UNIQUE)
IF OBJECT_ID(N'BangLuong', N'U') IS NULL
CREATE TABLE BangLuong (
    MaLuong  INT IDENTITY(1,1) NOT NULL UNIQUE,
    MaNV     INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    ThangNam VARCHAR(7) NOT NULL,
    ThucLinh DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_BangLuong PRIMARY KEY (MaNV, ThangNam)
);
GO

-- Dữ liệu mẫu (chỉ chèn khi bảng còn trống)
IF NOT EXISTS (SELECT 1 FROM PhongBan)
INSERT INTO PhongBan (TenPB, Mota) VALUES
(N'Kế toán',     N'Phòng Kế toán - Tài chính'),
(N'Kinh doanh',  N'Phòng Kinh doanh'),
(N'Kỹ thuật',    N'Phòng Kỹ thuật - Phát triển phần mềm');

IF NOT EXISTS (SELECT 1 FROM NhanVien)
INSERT INTO NhanVien (HoTen, NgaySinh, CCCD, MaPB, LuongCoBan, HeSoLuong) VALUES
(N'Nguyễn Văn An',   '1995-03-12', '001095000111', 1, 12000000, 1.2),
(N'Trần Thị Bình',   '1998-07-25', '001098000222', 2, 10000000, 1.0),
(N'Lê Minh Châu',    '1992-11-05', '001092000333', 3, 15000000, 1.5),
(N'Phạm Quốc Dũng',  '2000-01-30', '001200000444', 3,  9000000, 1.0),
(N'Hoàng Thu Hà',    '1996-09-18', '001096000555', 2, 11000000, 1.1);

-- Chấm công mẫu cho tháng 10/2026 (chưa chốt) để thử chức năng chốt lương
IF NOT EXISTS (SELECT 1 FROM BangChamCong)
INSERT INTO BangChamCong (MaNV, ThangNam, SoNgayCong, SoGioOT)
SELECT MaNV, '10/2026', 22, 4 FROM NhanVien;
GO
