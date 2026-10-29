-- ============================================================================
-- Kết hợp ưu điểm: 
-- 1. Chuẩn hóa phân tách trạng thái & Tối ưu hóa Index 
-- 2. Đa ảnh (RoomImages), Lượt xem (RoomViews) & Chi phí chi tiết 
-- ============================================================================

IF DB_ID(N'NhaTroDb') IS NULL
BEGIN
    EXEC(N'CREATE DATABASE NhaTroDb');
END
GO

USE NhaTroDb;
GO

SET NOCOUNT ON;
GO

-- XÓA BẢNG CŨ NẾU TỒN TẠI (THEO THỨ TỰ PHỤ THUỘC)
IF OBJECT_ID(N'dbo.RoomViews', N'U') IS NOT NULL DROP TABLE dbo.RoomViews;
IF OBJECT_ID(N'dbo.RoomImages', N'U') IS NOT NULL DROP TABLE dbo.RoomImages;
IF OBJECT_ID(N'dbo.Contacts', N'U') IS NOT NULL DROP TABLE dbo.Contacts;
IF OBJECT_ID(N'dbo.Favorites', N'U') IS NOT NULL DROP TABLE dbo.Favorites;
IF OBJECT_ID(N'dbo.Rooms', N'U') IS NOT NULL DROP TABLE dbo.Rooms;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL DROP TABLE dbo.AspNetUsers;
GO

-- 1. BẢNG ASPNETUSERS (QUẢN LÝ TÀI KHOẢN NGƯỜI DÙNG)
CREATE TABLE dbo.AspNetUsers
(
    Id NVARCHAR(450) NOT NULL,
    UserName NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    FullName NVARCHAR(150) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    AvatarUrl NVARCHAR(500) NULL,
    DateOfBirth DATE NULL,
    Address NVARCHAR(300) NULL,
    Role NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_AspNetUsers_Status DEFAULT N'Active',
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_AspNetUsers_CreatedAt DEFAULT GETDATE(),

    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id),
    CONSTRAINT UQ_AspNetUsers_UserName UNIQUE (UserName),
    CONSTRAINT UQ_AspNetUsers_Email UNIQUE (Email),
    CONSTRAINT CK_AspNetUsers_Role CHECK (Role IN (N'Tenant', N'Landlord', N'Admin')),
    CONSTRAINT CK_AspNetUsers_Status CHECK (Status IN (N'Active', N'Locked'))
);
GO

-- 2. BẢNG CATEGORIES (QUẢN LÝ DANH MỤC KHU VỰC VÀ LOẠI PHÒNG)
CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(150) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT 1,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT GETDATE(),

    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT CK_Categories_Type CHECK (Type IN (N'Location', N'RoomType'))
);
GO

-- 3. BẢNG ROOMS (QUẢN LÝ BÀI ĐĂNG PHÒNG TRỌ CHI TIẾT)
CREATE TABLE dbo.Rooms
(
    RoomId INT IDENTITY(1,1) NOT NULL,
    Title NVARCHAR(250) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Area DECIMAL(10,2) NOT NULL,
    Address NVARCHAR(300) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImageUrl NVARCHAR(500) NULL, -- Ảnh đại diện chính

    -- Chi phí dịch vụ chi tiết & Tiện ích
    ElectricityPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Rooms_ElectricityPrice DEFAULT(0),
    WaterPrice DECIMAL(18,2) NOT NULL CONSTRAINT DF_Rooms_WaterPrice DEFAULT(0),
    ServiceFee DECIMAL(18,2) NOT NULL CONSTRAINT DF_Rooms_ServiceFee DEFAULT(0),
    Amenities NVARCHAR(500) NULL, -- Ví dụ: N'Điều hòa, Nóng lạnh, Wifi, Ban công'

    -- Liên kết danh mục song song (Khu vực & Loại phòng)
    LocationCategoryId INT NOT NULL,
    RoomTypeCategoryId INT NOT NULL,
    LandlordId NVARCHAR(450) NOT NULL,

    -- Tách biệt rành mạch 2 cột Trạng thái
    ModerationStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_Rooms_ModerationStatus DEFAULT N'Pending', -- Pending, Approved, Rejected
    RentalStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_Rooms_RentalStatus DEFAULT N'Available',    -- Available, Rented
    RejectionReason NVARCHAR(500) NULL,

    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Rooms_CreatedAt DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,

    CONSTRAINT PK_Rooms PRIMARY KEY (RoomId),
    CONSTRAINT CK_Rooms_Price CHECK (Price >= 0),
    CONSTRAINT CK_Rooms_Area CHECK (Area > 0),
    CONSTRAINT CK_Rooms_ModerationStatus CHECK (ModerationStatus IN (N'Pending', N'Approved', N'Rejected')),
    CONSTRAINT CK_Rooms_RentalStatus CHECK (RentalStatus IN (N'Available', N'Rented')),

    CONSTRAINT FK_Rooms_LocationCategory FOREIGN KEY (LocationCategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Rooms_RoomTypeCategory FOREIGN KEY (RoomTypeCategoryId) REFERENCES dbo.Categories(CategoryId),
    CONSTRAINT FK_Rooms_AspNetUsers FOREIGN KEY (LandlordId) REFERENCES dbo.AspNetUsers(Id)
);
GO

-- 4. BẢNG ROOMIMAGES (QUẢN LÝ BỘ SƯU TẬP NHIỀU ẢNH PHÒNG TRỌ)
CREATE TABLE dbo.RoomImages
(
    RoomImageId INT IDENTITY(1,1) NOT NULL,
    RoomId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT(0),

    CONSTRAINT PK_RoomImages PRIMARY KEY (RoomImageId),
    CONSTRAINT FK_RoomImages_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId) ON DELETE CASCADE
);
GO

-- 5. BẢNG ROOMVIEWS (LƯU VẾT THỐNG KÊ LƯỢT XEM PHÒNG)
CREATE TABLE dbo.RoomViews
(
    RoomViewId BIGINT IDENTITY(1,1) NOT NULL,
    RoomId INT NOT NULL,
    ViewedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_RoomViews PRIMARY KEY (RoomViewId),
    CONSTRAINT FK_RoomViews_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId) ON DELETE CASCADE
);
GO

-- 6. BẢNG FAVORITES (QUẢN LÝ PHÒNG YÊU THÍCH)
CREATE TABLE dbo.Favorites
(
    FavoriteId INT IDENTITY(1,1) NOT NULL,
    TenantId NVARCHAR(450) NOT NULL,
    RoomId INT NOT NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Favorites_CreatedAt DEFAULT GETDATE(),

    CONSTRAINT PK_Favorites PRIMARY KEY (FavoriteId),
    CONSTRAINT FK_Favorites_AspNetUsers FOREIGN KEY (TenantId) REFERENCES dbo.AspNetUsers(Id),
    CONSTRAINT FK_Favorites_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId) ON DELETE CASCADE,
    CONSTRAINT UQ_Favorites_Tenant_Room UNIQUE (TenantId, RoomId)
);
GO

-- 7. BẢNG CONTACTS (QUẢN LÝ THÔNG TIN LIÊN HỆ / LỊCH HẸN XEM PHÒNG)
CREATE TABLE dbo.Contacts
(
    ContactId INT IDENTITY(1,1) NOT NULL,
    RoomId INT NOT NULL,
    SenderName NVARCHAR(150) NOT NULL,
    SenderPhone NVARCHAR(20) NOT NULL,
    SenderEmail NVARCHAR(256) NULL,
    SenderUserId NVARCHAR(450) NULL,
    Message NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL CONSTRAINT DF_Contacts_CreatedAt DEFAULT GETDATE(),

    CONSTRAINT PK_Contacts PRIMARY KEY (ContactId),
    CONSTRAINT FK_Contacts_Rooms FOREIGN KEY (RoomId) REFERENCES dbo.Rooms(RoomId),
    CONSTRAINT FK_Contacts_AspNetUsers_SenderUserId FOREIGN KEY (SenderUserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE SET NULL
);
GO

-- ============================================================================
-- TỐI ƯU HÓA HIỆU NĂNG TRUY VẤN (INDEXES)
-- ============================================================================
CREATE INDEX IX_Rooms_Moderation_Rental ON dbo.Rooms(ModerationStatus, RentalStatus);
CREATE INDEX IX_Rooms_LocationCategoryId ON dbo.Rooms(LocationCategoryId);
CREATE INDEX IX_Rooms_RoomTypeCategoryId ON dbo.Rooms(RoomTypeCategoryId);
CREATE INDEX IX_Rooms_LandlordId ON dbo.Rooms(LandlordId);
CREATE INDEX IX_RoomViews_ViewedAt ON dbo.RoomViews(ViewedAt);
CREATE INDEX IX_Favorites_TenantId ON dbo.Favorites(TenantId);
CREATE INDEX IX_Contacts_RoomId ON dbo.Contacts(RoomId);
GO

-- ============================================================================
-- THÊM DỮ LIỆU MẪU CHUẨN ĐỂ ĐEMO HỆ THỐNG (SEED DATA)
-- ============================================================================

-- 1. NGUỜI DÙNG MẪU
INSERT INTO dbo.AspNetUsers (Id, UserName, Email, PasswordHash, FullName, PhoneNumber, Role, Status)
VALUES
    (N'USR001', N'admin@nhatro.com', N'admin@nhatro.com', NULL, N'Quản trị viên', N'0901111111', N'Admin', N'Active'),
    (N'USR002', N'nam@gmail.com', N'nam@gmail.com', NULL, N'Nguyễn Văn Nam (Chủ trọ)', N'0912345678', N'Landlord', N'Active'),
    (N'USR003', N'hoa@gmail.com', N'hoa@gmail.com', NULL, N'Trần Thị Hoa (Chủ trọ)', N'0987654321', N'Landlord', N'Active'),
    (N'USR004', N'anh@gmail.com', N'anh@gmail.com', NULL, N'Lê Minh Anh (Người thuê)', N'0933445566', N'Tenant', N'Active'),
    (N'USR005', N'ha@gmail.com', N'ha@gmail.com', NULL, N'Phạm Thu Hà (Người thuê)', N'0977889900', N'Tenant', N'Active');
GO

-- 2. DANH MỤC MẪU
INSERT INTO dbo.Categories (CategoryName, Type, Description)
VALUES
    (N'Quận Cầu Giấy', N'Location', N'Khu vực tập trung nhiều ĐH: Mở, Quốc Gia, Sư Phạm...'),
    (N'Quận Đống Đa', N'Location', N'Khu vực trung tâm Đống Đa, Hà Nội'),
    (N'Quận Thanh Xuân', N'Location', N'Khu vực Thanh Xuân, Hà Nội'),
    (N'Quận Hai Bà Trưng', N'Location', N'Khu vực Hai Bà Trưng, Bách Khoa, Kinh Tế'),
    (N'Quận Nam Từ Liêm', N'Location', N'Khu vực Nam Từ Liêm, Mỹ Đình'),
    (N'Phòng trọ bình dân', N'RoomType', N'Phòng trọ giá rẻ cho sinh viên'),
    (N'Phòng trọ khép kín', N'RoomType', N'Phòng có vệ sinh riêng'),
    (N'Căn hộ mini', N'RoomType', N'Căn hộ mini cao cấp đầy đủ nội thất');
GO

-- 3. PHÒNG TRỌ MẪU
INSERT INTO dbo.Rooms
(
    Title, Price, Area, Address, Description, ImageUrl,
    ElectricityPrice, WaterPrice, ServiceFee, Amenities,
    LocationCategoryId, RoomTypeCategoryId, LandlordId,
    ModerationStatus, RentalStatus, RejectionReason
)
VALUES
(
    N'Phòng trọ khép kín mới xây gần ĐH Mở Hà Nội',
    3200000, 28.00, N'Cầu Giấy, Hà Nội',
    N'Phòng mới xây khép kín, có điều hòa, bình nóng lạnh, giờ giấc tự do, không chung chủ.',
    N'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=85',
    3500, 100000, 150000, N'Điều hòa, Nóng lạnh, Wifi, Nấu ăn, Để xe',
    1, 7, N'USR002', N'Approved', N'Available', NULL
),
(
    N'Căn hộ mini cao cấp full nội thất Thanh Xuân',
    4800000, 35.00, N'Thanh Xuân, Hà Nội',
    N'Căn hộ mini có sofa, bếp ăn riêng, tủ lạnh, máy giặt chung sân thượng, view đẹp.',
    N'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=85',
    3800, 30000/1, 200000, N'Điều hòa, Tủ lạnh, Bếp, Ban công, Thang máy',
    3, 8, N'USR002', N'Approved', N'Available', NULL
),
(
    N'Phòng trọ giá rẻ sinh viên Đống Đa',
    2200000, 20.00, N'Đống Đa, Hà Nội',
    N'Phòng sạch sẽ, giá tiết kiệm, chung vệ sinh sạch, ưu tiên sinh viên ngoan ngoãn.',
    N'https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=1200&q=85',
    3500, 80000, 50000, N'Wifi, Chỗ để xe, Giờ giấc tự do',
    2, 6, N'USR003', N'Approved', N'Available', NULL
),
(
    N'Phòng trọ chờ Admin phê duyệt',
    3000000, 25.00, N'Hai Bà Trưng, Hà Nội',
    N'Bài đăng mới khởi tạo đang trong danh sách chờ Admin kiểm duyệt.',
    N'https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=85',
    4000, 100000, 100000, N'Điều hòa, Ban công',
    4, 7, N'USR003', N'Pending', N'Available', NULL
),
(
    N'Phòng trọ đã cho thuê',
    2800000, 24.00, N'Nam Từ Liêm, Hà Nội',
    N'Phòng đã có khách chốt hợp đồng thuê.',
    N'https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=85',
    3500, 100000, 100000, N'Nóng lạnh, Giờ giấc tự do',
    5, 7, N'USR002', N'Approved', N'Rented', NULL
);
GO

-- 4. BỘ SƯU TẬP ẢNH MẪU (ROOMIMAGES)
INSERT INTO dbo.RoomImages (RoomId, ImageUrl, DisplayOrder)
VALUES
    (1, N'https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=85', 1),
    (1, N'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=85', 2),
    (2, N'https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=1200&q=85', 1);
GO

-- 5. PHÒNG YÊU THÍCH MẪU
INSERT INTO dbo.Favorites (TenantId, RoomId)
VALUES
    (N'USR004', 1),
    (N'USR004', 2),
    (N'USR005', 1);
GO

-- 6. THÔNG TIN LIÊN HỆ MẪU
INSERT INTO dbo.Contacts (RoomId, SenderName, SenderPhone, SenderEmail, SenderUserId, Message)
VALUES
    (1, N'Lê Minh Anh', N'0933445566', N'anh@gmail.com', N'USR004', N'Tôi muốn hẹn lịch xem phòng vào chiều thứ Bảy này.'),
    (2, N'Phạm Thu Hà', N'0977889900', N'ha@gmail.com', N'USR005', N'Căn hộ này có thể chuyển vào ở ngay từ đầu tháng sau được không?');
GO

PRINT N'=== KHOẢI TẠO CƠ SỞ DỮ LIỆU NHATRODB TỔNG HỢP HOÀN TẢO THÀNH CÔNG! ===';
