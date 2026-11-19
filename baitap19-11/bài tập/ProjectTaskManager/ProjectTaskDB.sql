-- =====================================================
-- BÀI 6.3 - ProjectTaskDB (SQL Server)
-- =====================================================
IF DB_ID(N'ProjectTaskDB') IS NULL CREATE DATABASE ProjectTaskDB;
GO
USE ProjectTaskDB;
GO

CREATE TABLE DuAn (
    MaDuAn      INT IDENTITY(1,1) PRIMARY KEY,
    TenDuAn     NVARCHAR(150) NOT NULL,
    NgayBatDau  DATETIME NULL,
    NgayKetThuc DATETIME NULL,
    TrangThai   NVARCHAR(50) NULL,
    CONSTRAINT CK_DuAn_TrangThai CHECK (TrangThai IN (N'Đang chạy', N'Hoàn thành', N'Tạm dừng'))
);

CREATE TABLE ThanhVien (
    MaThanhVien INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NULL UNIQUE,
    ChuyenMon   NVARCHAR(100) NULL
);

CREATE TABLE CongViec (
    MaTask    INT IDENTITY(1,1) PRIMARY KEY,
    MaDuAn    INT NOT NULL,
    TenTask   NVARCHAR(200) NOT NULL,
    MoTa      NVARCHAR(MAX) NULL,
    HanNop    DATETIME NULL,
    DoUuTien  NVARCHAR(20) NULL,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'To Do',
    CONSTRAINT FK_CongViec_DuAn FOREIGN KEY (MaDuAn) REFERENCES DuAn(MaDuAn) ON DELETE CASCADE,
    CONSTRAINT CK_CongViec_UuTien CHECK (DoUuTien IN (N'Cao', N'Trung bình', N'Thấp')),
    CONSTRAINT CK_CongViec_TrangThai CHECK (TrangThai IN (N'To Do', N'In Progress', N'Review', N'Done'))
);

CREATE TABLE PhanCong (
    MaTask      INT NOT NULL,
    MaThanhVien INT NOT NULL,
    NgayGiao    DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_PhanCong PRIMARY KEY (MaTask, MaThanhVien),
    CONSTRAINT FK_PhanCong_Task FOREIGN KEY (MaTask) REFERENCES CongViec(MaTask) ON DELETE CASCADE,
    CONSTRAINT FK_PhanCong_TV   FOREIGN KEY (MaThanhVien) REFERENCES ThanhVien(MaThanhVien)
);
GO

-- ---------------- Dữ liệu mẫu ----------------
INSERT DuAn (TenDuAn, NgayBatDau, NgayKetThuc, TrangThai) VALUES
 (N'Website bán hàng', '2026-09-01', '2026-12-31', N'Đang chạy'),
 (N'App quản lý kho',  '2026-06-01', '2026-10-30', N'Tạm dừng');

INSERT ThanhVien (HoTen, Email, ChuyenMon) VALUES
 (N'Nguyễn Văn An', 'an@example.com',   N'Backend'),
 (N'Trần Thị Bình', 'binh@example.com', N'Frontend'),
 (N'Lê Minh Châu',  'chau@example.com', N'Tester');

INSERT CongViec (MaDuAn, TenTask, MoTa, HanNop, DoUuTien, TrangThai) VALUES
 (1, N'Thiết kế CSDL',      N'Vẽ ERD, tạo bảng',     '2026-09-15', N'Cao',        N'Done'),
 (1, N'Làm trang chủ',      N'HTML/CSS responsive',  '2026-11-10', N'Trung bình', N'In Progress'),
 (1, N'API đăng nhập',      N'JWT',                  '2026-11-01', N'Cao',        N'In Progress'),
 (1, N'Kiểm thử thanh toán',N'Test cổng thanh toán', '2026-12-20', N'Thấp',       N'To Do'),
 (2, N'Phân tích yêu cầu',  N'Họp khách hàng',       '2026-07-01', N'Cao',        N'Review');

INSERT PhanCong (MaTask, MaThanhVien) VALUES (1,1),(2,2),(3,1),(3,2),(4,3),(5,3);
GO
