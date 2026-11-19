-- =====================================================
-- BÀI 6.5 - KaraokeDB (SQL Server)
-- =====================================================
IF DB_ID(N'KaraokeDB') IS NULL CREATE DATABASE KaraokeDB;
GO
USE KaraokeDB;
GO

CREATE TABLE PhongHat (
    MaPhong     INT IDENTITY(1,1) PRIMARY KEY,
    TenPhong    NVARCHAR(50) NOT NULL,
    LoaiPhong   NVARCHAR(30) NULL,
    GiaTheoGio  DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai   NVARCHAR(20) NOT NULL DEFAULT N'Trống',
    CONSTRAINT CK_Phong_TrangThai CHECK (TrangThai IN (N'Trống', N'Đang hát')),
    CONSTRAINT CK_Phong_Gia CHECK (GiaTheoGio >= 0)
);

CREATE TABLE DichVu (
    MaDV   INT IDENTITY(1,1) PRIMARY KEY,
    TenDV  NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL DEFAULT 0,
    TonKho INT NOT NULL DEFAULT 0,
    CONSTRAINT CK_DV_TonKho CHECK (TonKho >= 0),
    CONSTRAINT CK_DV_Gia CHECK (DonGia >= 0)
);

CREATE TABLE HoaDon (
    MaHD           INT IDENTITY(1,1) PRIMARY KEY,
    MaPhong        INT NOT NULL,
    GioVao         DATETIME NOT NULL DEFAULT GETDATE(),
    GioRa          DATETIME NULL,
    TongTienPhong  DECIMAL(18,2) NOT NULL DEFAULT 0,
    TongTienDichVu DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai      BIT NOT NULL DEFAULT 0,        -- 0: chưa thanh toán, 1: đã thanh toán
    CONSTRAINT FK_HoaDon_Phong FOREIGN KEY (MaPhong) REFERENCES PhongHat(MaPhong)
);
-- Mỗi phòng chỉ có tối đa 1 hóa đơn chưa thanh toán (chặn mở phòng 2 lần)
CREATE UNIQUE INDEX UX_HoaDon_PhongDangMo ON HoaDon(MaPhong) WHERE TrangThai = 0;

CREATE TABLE ChiTietDichVu (
    MaHD    INT NOT NULL,
    MaDV    INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGia  DECIMAL(18,2) NOT NULL,              -- đơn giá tại thời điểm gọi
    CONSTRAINT PK_ChiTietDV PRIMARY KEY (MaHD, MaDV),
    CONSTRAINT FK_CTDV_HoaDon FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD) ON DELETE CASCADE,
    CONSTRAINT FK_CTDV_DichVu FOREIGN KEY (MaDV) REFERENCES DichVu(MaDV),
    CONSTRAINT CK_CTDV_SoLuong CHECK (SoLuong > 0)
);
GO

-- ---------------- Dữ liệu mẫu ----------------
INSERT PhongHat (TenPhong, LoaiPhong, GiaTheoGio) VALUES
 (N'Phòng VIP 01',     N'VIP',    250000),
 (N'Phòng VIP 02',     N'VIP',    250000),
 (N'Phòng VIP 03',     N'VIP',    250000),
 (N'Phòng Thường 01',  N'Thường', 120000),
 (N'Phòng Thường 02',  N'Thường', 120000),
 (N'Phòng Thường 03',  N'Thường', 120000),
 (N'Phòng Thường 04',  N'Thường', 120000),
 (N'Phòng Thường 05',  N'Thường', 120000);

INSERT DichVu (TenDV, DonGia, TonKho) VALUES
 (N'Bia Heineken',   30000, 120),
 (N'Bia Tiger',      25000, 100),
 (N'Nước suối',      10000, 200),
 (N'Coca-Cola',      15000, 100),
 (N'Trái cây dĩa',  120000,  30),
 (N'Khô bò',         90000,  25),
 (N'Snack',          20000,  80);

-- Phòng VIP 02 đang hát từ 95 phút trước, đã gọi 6 bia Heineken + 1 trái cây dĩa
INSERT HoaDon (MaPhong, GioVao, TrangThai) VALUES (2, DATEADD(MINUTE, -95, GETDATE()), 0);
UPDATE PhongHat SET TrangThai = N'Đang hát' WHERE MaPhong = 2;
INSERT ChiTietDichVu (MaHD, MaDV, SoLuong, DonGia) VALUES (1, 1, 6, 30000), (1, 5, 1, 120000);
GO
