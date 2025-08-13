-- Insurance Management System Database Creation Script
-- Run this script in SQL Server Management Studio or any SQL client

-- 1. Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InsuranceSystem')
BEGIN
    CREATE DATABASE InsuranceSystem;
END
GO

-- 2. Use Database
USE InsuranceSystem;
GO

-- 3. Create Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(255) NOT NULL,
        Role NVARCHAR(50) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        Password_Hash NVARCHAR(MAX) NOT NULL,
        Created_At DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 4. Create Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        User_Id INT NULL,
        Type NVARCHAR(50) NOT NULL,
        Id_No NVARCHAR(50) NOT NULL UNIQUE,
        Address NVARCHAR(1000) NULL,
        Phone NVARCHAR(20) NULL,
        FOREIGN KEY (User_Id) REFERENCES Users(Id) ON DELETE SET NULL
    );
END
GO

-- 5. Create Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_IdNo')
BEGIN
    CREATE UNIQUE INDEX IX_Customers_IdNo ON Customers(Id_No);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_UserId')
BEGIN
    CREATE UNIQUE INDEX IX_Customers_UserId ON Customers(User_Id) WHERE User_Id IS NOT NULL;
END
GO

-- 6. Insert Admin User (Password: Admin123!)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@insurance.com')
BEGIN
    INSERT INTO Users (Name, Role, Email, Password_Hash, Created_At)
    VALUES ('Admin User', 'Admin', 'admin@insurance.com', 
            '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 
            GETDATE());
END
GO

-- 5. Create Offers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Offers')
BEGIN
    CREATE TABLE Offers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_Id INT NOT NULL,
        Insurance_Type NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL CHECK (Status IN ('pending', 'approved', 'cancelled')),
        FOREIGN KEY (Customer_Id) REFERENCES Customers(Id) ON DELETE CASCADE
    );
END
GO

-- 6. Create Policies Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Policies')
BEGIN
    CREATE TABLE Policies (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Offer_Id INT NOT NULL,
        Start_Date DATE NOT NULL,
        End_Date DATE NOT NULL,
        Policy_Number NVARCHAR(100) NOT NULL UNIQUE,
        FOREIGN KEY (Offer_Id) REFERENCES Offers(Id) ON DELETE CASCADE
    );
END
GO

-- 7. Create Claims Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Claims')
BEGIN
    CREATE TABLE Claims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Policy_Id INT NOT NULL,
        Description NVARCHAR(MAX),
        Status NVARCHAR(50) NOT NULL CHECK (Status IN ('pending', 'in_review', 'resolved')),
        Created_At DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (Policy_Id) REFERENCES Policies(Id) ON DELETE CASCADE
    );
END
GO

-- 8. Create Payments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE Payments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Policy_Id INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Paid_At DATETIME NOT NULL DEFAULT GETDATE(),
        Method NVARCHAR(50) NOT NULL CHECK (Method IN ('nakit', 'kredi', 'havale')),
        FOREIGN KEY (Policy_Id) REFERENCES Policies(Id) ON DELETE CASCADE
    );
END
GO

-- 9. Create Documents Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
BEGIN
    CREATE TABLE Documents (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_Id INT NULL,
        Claim_Id INT NULL,
        File_Name NVARCHAR(255) NOT NULL,
        File_Url NVARCHAR(MAX) NOT NULL,
        Uploaded_At DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (Customer_Id) REFERENCES Customers(Id) ON DELETE SET NULL,
        FOREIGN KEY (Claim_Id) REFERENCES Claims(Id) ON DELETE SET NULL
    );
END
GO

-- 10. Verify Tables
SELECT 'Users Table:' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Customers Table:', COUNT(*) FROM Customers
UNION ALL
SELECT 'Offers Table:', COUNT(*) FROM Offers
UNION ALL
SELECT 'Policies Table:', COUNT(*) FROM Policies
UNION ALL
SELECT 'Claims Table:', COUNT(*) FROM Claims
UNION ALL
SELECT 'Payments Table:', COUNT(*) FROM Payments
UNION ALL
SELECT 'Documents Table:', COUNT(*) FROM Documents;
GO 