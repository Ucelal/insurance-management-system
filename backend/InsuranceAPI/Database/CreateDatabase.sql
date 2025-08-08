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

-- 7. Verify Tables
SELECT 'Users Table:' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Customers Table:', COUNT(*) FROM Customers;
GO 