-- Market Management System Database Schema
-- T-SQL Script for SQL Server

-- Create database
CREATE DATABASE MarketManagementSystem;
GO

-- Use the database
USE MarketManagementSystem;
GO

-- Create schemas
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'market')
    EXEC('CREATE SCHEMA market')
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'auth')
    EXEC('CREATE SCHEMA auth')
GO

-- =============================================
-- AUTH SCHEMA TABLES
-- =============================================

-- Create Role table
CREATE TABLE auth.Role (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

-- Create User table
CREATE TABLE auth.[User] (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    PhoneNumber NVARCHAR(20),
    RoleId BIGINT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleId) REFERENCES auth.Role(Id)
);

-- Create RefreshToken table
CREATE TABLE auth.RefreshToken (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NOT NULL,
    Token NVARCHAR(500) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt DATETIME2,
    IsRevoked BIT NOT NULL DEFAULT 0,
    IsUsed BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (UserId) REFERENCES auth.[User](Id)
);

-- =============================================
-- MARKET SCHEMA TABLES
-- =============================================

-- Create Location table
CREATE TABLE market.Location (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NTEXT NOT NULL,
    City NVARCHAR(50) NOT NULL,
    PostalCode NVARCHAR(20),
    Country NVARCHAR(50) NOT NULL DEFAULT 'Georgia',
    Phone NVARCHAR(20),
    OpeningHours NVARCHAR(200),
    Description NTEXT,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Location_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Location_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create Vendor table
CREATE TABLE market.Vendor (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ContactPersonName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Address NTEXT NOT NULL,
    Description NTEXT,
    CommissionRate DECIMAL(5,2) NOT NULL DEFAULT 10.00,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Vendor_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Vendor_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create Category table
CREATE TABLE market.Category (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NTEXT,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Category_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Category_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create VendorLocation table
CREATE TABLE market.VendorLocation (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    VendorId BIGINT NOT NULL,
    LocationId BIGINT NOT NULL,
    StallNumber NVARCHAR(20),
    RentAmount DECIMAL(8,2),
    IsActive BIT NOT NULL DEFAULT 1,
    StartDate DATETIME2 NOT NULL,
    EndDate DATETIME2,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_VendorLocation_Vendor FOREIGN KEY (VendorId) REFERENCES market.Vendor(Id),
    CONSTRAINT FK_VendorLocation_Location FOREIGN KEY (LocationId) REFERENCES market.Location(Id),
    CONSTRAINT FK_VendorLocation_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_VendorLocation_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT UQ_VendorLocation_VendorId_LocationId UNIQUE (VendorId, LocationId)
);

-- Create Product table
CREATE TABLE market.Product (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NTEXT,
    Price DECIMAL(10,2) NOT NULL,
    InStock INT NOT NULL DEFAULT 0,
    Unit NVARCHAR(20) NOT NULL DEFAULT 'piece',
    LocationId BIGINT NOT NULL,
    CategoryId BIGINT NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Product_Location FOREIGN KEY (LocationId) REFERENCES market.Location(Id),
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryId) REFERENCES market.Category(Id),
    CONSTRAINT FK_Product_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Product_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create Discount table
CREATE TABLE market.Discount (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    DiscountCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NTEXT,
    Percentage DECIMAL(5,2) NOT NULL,
    StartDate DATETIME2,
    EndDate DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1,
    LocationId BIGINT,
    VendorId BIGINT,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Discount_Location FOREIGN KEY (LocationId) REFERENCES market.Location(Id),
    CONSTRAINT FK_Discount_Vendor FOREIGN KEY (VendorId) REFERENCES market.Vendor(Id),
    CONSTRAINT FK_Discount_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Discount_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create Order table
CREATE TABLE market.[Order] (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Total DECIMAL(10,2) NOT NULL,
    SubTotal DECIMAL(10,2) NOT NULL,
    TotalCommission DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    LocationId BIGINT NOT NULL,
    DiscountId BIGINT,
    DiscountAmount DECIMAL(10,2) DEFAULT 0,
    UserId BIGINT NOT NULL,
    CustomerName NVARCHAR(100),
    CustomerPhone NVARCHAR(20),
    Notes NTEXT,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Order_Location FOREIGN KEY (LocationId) REFERENCES market.Location(Id),
    CONSTRAINT FK_Order_Discount FOREIGN KEY (DiscountId) REFERENCES market.Discount(Id),
    CONSTRAINT FK_Order_User FOREIGN KEY (UserId) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Order_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Order_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create Procurement table
CREATE TABLE market.Procurement (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    VendorId BIGINT NOT NULL,
    LocationId BIGINT NOT NULL,
    ReferenceNo NVARCHAR(100) UNIQUE,
    ProcurementDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    Notes NTEXT,
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Procurement_Vendor FOREIGN KEY (VendorId) REFERENCES market.Vendor(Id),
    CONSTRAINT FK_Procurement_Location FOREIGN KEY (LocationId) REFERENCES market.Location(Id),
    CONSTRAINT FK_Procurement_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Procurement_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- Create ProcurementDetail table
CREATE TABLE market.ProcurementDetail (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    ProcurementId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    PurchasePrice DECIMAL(10,2) NOT NULL,
    Quantity INT NOT NULL,
    LineTotal DECIMAL(10,2) NOT NULL,
    
    CONSTRAINT FK_ProcurementDetail_Procurement FOREIGN KEY (ProcurementId) REFERENCES market.Procurement(Id),
    CONSTRAINT FK_ProcurementDetail_Product FOREIGN KEY (ProductId) REFERENCES market.Product(Id)
);

-- Create OrderDetail table
CREATE TABLE market.OrderDetail (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    Quantity BIGINT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    LineTotal DECIMAL(10,2) NOT NULL,
    CostPrice DECIMAL(10,2),
    Profit DECIMAL(10,2),
    
    CONSTRAINT FK_OrderDetail_Order FOREIGN KEY (OrderId) REFERENCES market.[Order](Id),
    CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductId) REFERENCES market.Product(Id)
);

-- Create Payment table
CREATE TABLE market.Payment (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    OrderId BIGINT NOT NULL,
    PaymentDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Completed',
    CreatedBy BIGINT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy BIGINT NOT NULL,
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Payment_Order FOREIGN KEY (OrderId) REFERENCES market.[Order](Id),
    CONSTRAINT FK_Payment_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES auth.[User](Id),
    CONSTRAINT FK_Payment_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES auth.[User](Id)
);

-- =============================================
-- CREATE INDEXES
-- =============================================

-- Auth schema indexes
CREATE INDEX IX_User_Email ON auth.[User](Email);
CREATE INDEX IX_User_Username ON auth.[User](Username);
CREATE INDEX IX_User_RoleId ON auth.[User](RoleId);
CREATE INDEX IX_User_IsActive ON auth.[User](IsActive);

CREATE INDEX IX_RefreshToken_UserId ON auth.RefreshToken(UserId);
CREATE INDEX IX_RefreshToken_Token ON auth.RefreshToken(Token);

-- Market schema indexes
CREATE INDEX IX_Location_City ON market.Location(City);
CREATE INDEX IX_Location_IsActive ON market.Location(IsActive);
CREATE INDEX IX_Location_Name ON market.Location(Name);

CREATE INDEX IX_Vendor_Email ON market.Vendor(Email);
CREATE INDEX IX_Vendor_IsActive ON market.Vendor(IsActive);
CREATE INDEX IX_Vendor_Name ON market.Vendor(Name);

CREATE INDEX IX_Category_Name ON market.Category(Name);

CREATE INDEX IX_VendorLocation_VendorId ON market.VendorLocation(VendorId);
CREATE INDEX IX_VendorLocation_LocationId ON market.VendorLocation(LocationId);

CREATE INDEX IX_Product_CategoryId ON market.Product(CategoryId);
CREATE INDEX IX_Product_LocationId ON market.Product(LocationId);
CREATE INDEX IX_Product_Price ON market.Product(Price);
CREATE INDEX IX_Product_IsAvailable ON market.Product(IsAvailable);
CREATE INDEX IX_Product_CreatedAt ON market.Product(CreatedAt);

CREATE INDEX IX_Discount_DiscountCode ON market.Discount(DiscountCode);
CREATE INDEX IX_Discount_IsActive ON market.Discount(IsActive);
CREATE INDEX IX_Discount_LocationId ON market.Discount(LocationId);
CREATE INDEX IX_Discount_VendorId ON market.Discount(VendorId);
CREATE INDEX IX_Discount_StartDate_EndDate ON market.Discount(StartDate, EndDate);

CREATE INDEX IX_Order_OrderDate ON market.[Order](OrderDate);
CREATE INDEX IX_Order_LocationId ON market.[Order](LocationId);
CREATE INDEX IX_Order_UserId ON market.[Order](UserId);
CREATE INDEX IX_Order_DiscountId ON market.[Order](DiscountId);
CREATE INDEX IX_Order_Status ON market.[Order](Status);
CREATE INDEX IX_Order_OrderNumber ON market.[Order](OrderNumber);

CREATE INDEX IX_Procurement_VendorId ON market.Procurement(VendorId);
CREATE INDEX IX_Procurement_LocationId ON market.Procurement(LocationId);
CREATE INDEX IX_Procurement_ProcurementDate ON market.Procurement(ProcurementDate);

CREATE INDEX IX_ProcurementDetail_ProcurementId ON market.ProcurementDetail(ProcurementId);
CREATE INDEX IX_ProcurementDetail_ProductId ON market.ProcurementDetail(ProductId);

CREATE INDEX IX_OrderDetail_OrderId ON market.OrderDetail(OrderId);
CREATE INDEX IX_OrderDetail_ProductId ON market.OrderDetail(ProductId);

CREATE INDEX IX_Payment_OrderId ON market.Payment(OrderId);
CREATE INDEX IX_Payment_PaymentDate ON market.Payment(PaymentDate);
CREATE INDEX IX_Payment_Status ON market.Payment(Status);

-- =============================================
-- INSERT SAMPLE DATA FOR ROLES
-- =============================================

INSERT INTO auth.Role (Name) VALUES 
('Admin'),
('VendorManager'),
('LocationManager');

PRINT 'Market Management System database schema created successfully!';