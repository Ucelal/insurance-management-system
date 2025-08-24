-- Insurance Management System Database Creation Script
-- Güncellenmiş ER diagram tasarımına göre

USE master;
GO

-- Veritabanı yoksa oluştur
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'InsuranceSystem')
BEGIN
    CREATE DATABASE InsuranceSystem;
END
GO

USE InsuranceSystem;
GO

-- Users tablosu
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

-- Agents tablosu (YENİ)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Agents')
BEGIN
    CREATE TABLE Agents (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        User_Id INT NOT NULL,
        Agent_Code NVARCHAR(10) NOT NULL UNIQUE,
        Department NVARCHAR(100) NOT NULL,
        Address NVARCHAR(500) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        FOREIGN KEY (User_Id) REFERENCES Users(Id) ON DELETE CASCADE
    );
END

-- Customers tablosu (güncellendi - email eklendi)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        User_Id INT NOT NULL,
        Type NVARCHAR(50) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Id_No NVARCHAR(50) NOT NULL UNIQUE,
        Address NVARCHAR(1000) NULL,
        Phone NVARCHAR(20) NULL,
        FOREIGN KEY (User_Id) REFERENCES Users(Id) ON DELETE CASCADE
    );
END

-- Offers tablosu (güncellendi - agent_id eklendi)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Offers')
BEGIN
    CREATE TABLE Offers (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_Id INT NOT NULL,
        Agent_Id INT NOT NULL,
        Insurance_Type NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
        FOREIGN KEY (Customer_Id) REFERENCES Customers(Id) ON DELETE CASCADE,
        FOREIGN KEY (Agent_Id) REFERENCES Agents(Id) ON DELETE NO ACTION
    );
END

-- Policies tablosu
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

-- Claims tablosu (güncellendi - yeni alanlar eklendi)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Claims')
BEGIN
    CREATE TABLE Claims (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Policy_Id INT NOT NULL,
        Created_By_User_Id INT NOT NULL,
        Processed_By_User_Id INT NULL,
        Description NVARCHAR(MAX) NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'pending',
        Created_At DATETIME NOT NULL DEFAULT GETDATE(),
        Processed_At DATETIME NULL,
        Notes NVARCHAR(1000) NULL,
        FOREIGN KEY (Policy_Id) REFERENCES Policies(Id) ON DELETE CASCADE,
        FOREIGN KEY (Created_By_User_Id) REFERENCES Users(Id) ON DELETE NO ACTION,
        FOREIGN KEY (Processed_By_User_Id) REFERENCES Users(Id) ON DELETE NO ACTION
    );
END

-- Payments tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Payments')
BEGIN
    CREATE TABLE Payments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Policy_Id INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        Paid_At DATETIME NOT NULL DEFAULT GETDATE(),
        Method NVARCHAR(50) NOT NULL,
        FOREIGN KEY (Policy_Id) REFERENCES Policies(Id) ON DELETE CASCADE
    );
END

-- Documents tablosu
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Documents')
BEGIN
    CREATE TABLE Documents (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Customer_Id INT NOT NULL,
        Claim_Id INT NULL,
        File_Name NVARCHAR(255) NOT NULL,
        File_Url NVARCHAR(1000) NOT NULL,
        Uploaded_At DATETIME NOT NULL DEFAULT GETDATE(),
        FOREIGN KEY (Customer_Id) REFERENCES Customers(Id) ON DELETE NO ACTION,
        FOREIGN KEY (Claim_Id) REFERENCES Claims(Id) ON DELETE SET NULL
    );
END

-- Indexes oluştur
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_IdNo')
BEGIN
    CREATE UNIQUE INDEX IX_Customers_IdNo ON Customers(Id_No);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Agents_AgentCode')
BEGIN
    CREATE UNIQUE INDEX IX_Agents_AgentCode ON Agents(Agent_Code);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Policies_PolicyNumber')
BEGIN
    CREATE UNIQUE INDEX IX_Policies_PolicyNumber ON Policies(Policy_Number);
END

-- Admin kullanıcısı oluştur (eğer yoksa)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@insurance.com')
BEGIN
    INSERT INTO Users (Name, Role, Email, Password_Hash, Created_At)
    VALUES ('Admin User', 'admin', 'admin@insurance.com', 
            '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 
            GETDATE());
END

-- Test verileri ekle
-- Admin Agent oluştur
IF NOT EXISTS (SELECT * FROM Agents WHERE Agent_Code = 'ADMIN001')
BEGIN
    DECLARE @AdminUserId INT = (SELECT Id FROM Users WHERE Email = 'admin@insurance.com');
    INSERT INTO Agents (User_Id, Agent_Code, Department, Address, Phone)
    VALUES (@AdminUserId, 'ADMIN001', 'Yönetim', 'Merkez Ofis', '555 000 0000');
END

-- Test Customer oluştur
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'test@customer.com')
BEGIN
    INSERT INTO Users (Name, Role, Email, Password_Hash, Created_At)
    VALUES ('Test Customer', 'customer', 'test@customer.com', 
            '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 
            GETDATE());
    
    DECLARE @TestUserId INT = (SELECT Id FROM Users WHERE Email = 'test@customer.com');
    INSERT INTO Customers (User_Id, Type, Email, Id_No, Address, Phone)
    VALUES (@TestUserId, 'bireysel', 'test@customer.com', '12345678901', 'Test Address', '555 123 4567');
END

-- Test Agent oluştur
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'agent@insurance.com')
BEGIN
    INSERT INTO Users (Name, Role, Email, Password_Hash, Created_At)
    VALUES ('Test Agent', 'agent', 'agent@insurance.com', 
            '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy', 
            GETDATE());
    
    DECLARE @AgentUserId INT = (SELECT Id FROM Users WHERE Email = 'agent@insurance.com');
    INSERT INTO Agents (User_Id, Agent_Code, Department, Address, Phone)
    VALUES (@AgentUserId, 'AG001', 'Satış', 'Test Agent Address', '555 999 8888');
END

-- Mevcut kullanıcıların hash'lerini güncelle (eğer geçersizse)
UPDATE Users 
SET Password_Hash = '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy'
WHERE Password_Hash IS NULL OR Password_Hash = '' OR Password_Hash NOT LIKE '$2a$%';

-- Tablo sayılarını kontrol et
SELECT 'Users Table:', COUNT(*) FROM Users
UNION ALL
SELECT 'Agents Table:', COUNT(*) FROM Agents
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