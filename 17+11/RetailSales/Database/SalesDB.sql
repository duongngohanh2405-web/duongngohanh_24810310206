-- ============================================================
-- Bài 6.1 - SalesDB (chạy toàn bộ script này trong SSMS)
-- ============================================================
IF DB_ID(N'SalesDB') IS NULL
    CREATE DATABASE SalesDB;
GO
USE SalesDB;
GO

IF OBJECT_ID(N'NhanVien', N'U') IS NULL
CREATE TABLE NhanVien (
    MaNV        INT IDENTITY(1,1) PRIMARY KEY,
    TenNV       NVARCHAR(100) NOT NULL,
    SoDienThoai VARCHAR(15) UNIQUE,
    ChucVu      NVARCHAR(50)
);

IF OBJECT_ID(N'KhachHang', N'U') IS NULL
CREATE TABLE KhachHang (
    MaKH        INT IDENTITY(1,1) PRIMARY KEY,
    TenKH       NVARCHAR(100) NOT NULL,
    DienThoai   VARCHAR(15) UNIQUE,
    DiemTichLuy INT NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'SanPham', N'U') IS NULL
CREATE TABLE SanPham (
    MaSP       VARCHAR(20) PRIMARY KEY,
    TenSP      NVARCHAR(150) NOT NULL,
    GiaBan     DECIMAL(18,2) NOT NULL CHECK (GiaBan > 0),
    SoLuongTon INT NOT NULL CHECK (SoLuongTon >= 0)
);

IF OBJECT_ID(N'HoaDon', N'U') IS NULL
CREATE TABLE HoaDon (
    MaHD     INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap  DATETIME NOT NULL DEFAULT GETDATE(),
    MaNV     INT NOT NULL FOREIGN KEY REFERENCES NhanVien(MaNV),
    MaKH     INT NULL FOREIGN KEY REFERENCES KhachHang(MaKH),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0
);

IF OBJECT_ID(N'ChiTietHoaDon', N'U') IS NULL
CREATE TABLE ChiTietHoaDon (
    MaHD    INT NOT NULL FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaSP    VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES SanPham(MaSP),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia  DECIMAL(18,2) NOT NULL,
    CONSTRAINT PK_CTHD PRIMARY KEY (MaHD, MaSP)
);
GO

-- Dữ liệu mẫu (chỉ chèn khi bảng còn trống)
IF NOT EXISTS (SELECT 1 FROM NhanVien)
INSERT INTO NhanVien (TenNV, SoDienThoai, ChucVu) VALUES
(N'Nguyễn Văn An', '0901000001', N'Thu ngân'),
(N'Trần Thị Bình', '0901000002', N'Quản lý');

IF NOT EXISTS (SELECT 1 FROM KhachHang)
INSERT INTO KhachHang (TenKH, DienThoai) VALUES
(N'Lê Minh Châu', '0912000001'),
(N'Phạm Quốc Dũng', '0912000002');

IF NOT EXISTS (SELECT 1 FROM SanPham)
INSERT INTO SanPham (MaSP, TenSP, GiaBan, SoLuongTon) VALUES
('8934563', N'Mì Hảo Hảo', 4500, 200),
('8935001', N'Sữa tươi Vinamilk 1L', 32000, 80),
('8936002', N'Nước suối Aquafina 500ml', 6000, 150),
('8937003', N'Dầu ăn Neptune 1L', 58000, 40),
('8938004', N'Gạo ST25 5kg', 165000, 30),
('8939005', N'Bánh Oreo', 18000, 100);
GO
