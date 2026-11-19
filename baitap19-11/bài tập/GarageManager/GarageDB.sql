-- =====================================================
-- BÀI 6.4 - GarageDB (SQL Server)
-- =====================================================
IF DB_ID(N'GarageDB') IS NULL CREATE DATABASE GarageDB;
GO
USE GarageDB;
GO

CREATE TABLE KhachHang (
    MaKH      INT IDENTITY(1,1) PRIMARY KEY,
    TenKH     NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(15) NULL,
    DiaChi    NVARCHAR(200) NULL
);
-- UNIQUE cho điện thoại nhưng cho phép nhiều dòng NULL (UNIQUE thường chỉ cho 1 NULL)
CREATE UNIQUE INDEX UX_KhachHang_DienThoai ON KhachHang(DienThoai) WHERE DienThoai IS NOT NULL;

CREATE TABLE Xe (
    BienSo     VARCHAR(20) PRIMARY KEY,
    MaKH       INT NOT NULL,
    HieuXe     NVARCHAR(50) NULL,
    NamSanXuat INT NULL,
    CONSTRAINT FK_Xe_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)
);

CREATE TABLE PhuTung (
    MaPhuTung  INT IDENTITY(1,1) PRIMARY KEY,
    TenPhuTung NVARCHAR(150) NOT NULL,
    DonGia     DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoLuongTon INT NOT NULL DEFAULT 0,
    CONSTRAINT CK_PhuTung_Ton CHECK (SoLuongTon >= 0),
    CONSTRAINT CK_PhuTung_Gia CHECK (DonGia >= 0)
);

CREATE TABLE PhieuSuaChua (
    MaPhieu    INT IDENTITY(1,1) PRIMARY KEY,
    BienSo     VARCHAR(20) NOT NULL,
    NgayNhan   DATETIME NOT NULL DEFAULT GETDATE(),
    NoiDungSua NVARCHAR(MAX) NULL,
    TongTien   DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai  NVARCHAR(30) NOT NULL DEFAULT N'Đang sửa',
    CONSTRAINT FK_Phieu_Xe FOREIGN KEY (BienSo) REFERENCES Xe(BienSo),
    CONSTRAINT CK_Phieu_TrangThai CHECK (TrangThai IN (N'Đang sửa', N'Đã hoàn thành', N'Đã thanh toán'))
);

CREATE TABLE ChiTietSuaChua (
    MaPhieu    INT NOT NULL,
    MaPhuTung  INT NOT NULL,
    SoLuong    INT NOT NULL,
    TienCong   DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT PK_ChiTiet PRIMARY KEY (MaPhieu, MaPhuTung),
    CONSTRAINT FK_CT_Phieu FOREIGN KEY (MaPhieu) REFERENCES PhieuSuaChua(MaPhieu) ON DELETE CASCADE,
    CONSTRAINT FK_CT_PhuTung FOREIGN KEY (MaPhuTung) REFERENCES PhuTung(MaPhuTung),
    CONSTRAINT CK_CT_SoLuong CHECK (SoLuong > 0),
    CONSTRAINT CK_CT_TienCong CHECK (TienCong >= 0)
);
GO

-- ---------------- Dữ liệu mẫu ----------------
INSERT KhachHang (TenKH, DienThoai, DiaChi) VALUES
 (N'Nguyễn Văn Hùng', '0901234567', N'12 Lê Lợi, Hà Nội'),
 (N'Trần Thị Mai',    '0912345678', N'45 Trần Phú, Đà Nẵng');

INSERT Xe (BienSo, MaKH, HieuXe, NamSanXuat) VALUES
 ('30A-123.45', 1, N'Toyota',  2019),
 ('30G-678.90', 1, N'Honda',   2021),
 ('43A-555.66', 2, N'VinFast', 2023);

-- Tồn kho dưới đây đã trừ sẵn phiếu mẫu số 1 (2 dầu nhớt, 1 lọc gió)
INSERT PhuTung (TenPhuTung, DonGia, SoLuongTon) VALUES
 (N'Dầu nhớt 5W-30 (lít)',  150000, 48),
 (N'Lọc gió động cơ',       200000, 19),
 (N'Má phanh trước',        650000, 10),
 (N'Bugi',                   90000, 40),
 (N'Ắc quy 12V',           1800000,  3);

INSERT PhieuSuaChua (BienSo, NoiDungSua, TongTien, TrangThai)
VALUES ('30A-123.45', N'Bảo dưỡng định kỳ 20.000 km', 650000, N'Đã hoàn thành');
INSERT ChiTietSuaChua (MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES (1, 1, 2, 100000), (1, 2, 1, 50000);
-- 2*150.000 + 1*200.000 + 100.000 + 50.000 = 650.000
GO
