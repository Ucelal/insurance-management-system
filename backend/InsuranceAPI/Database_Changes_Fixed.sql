-- ========================================
-- VERİTABANI DEĞİŞİKLİKLERİ (DÜZELTİLMİŞ)
-- ========================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

-- 1. Offers tablosundan alanları sil
ALTER TABLE Offers DROP COLUMN Description;
ALTER TABLE Offers DROP COLUMN Additional_Data;
ALTER TABLE Offers DROP COLUMN Approved_At;

-- 2. Offers tablosunda alan isimlerini değiştir
EXEC sp_rename 'Offers.Reviewed_By_Agent_Id', 'Reviewed_By', 'COLUMN';
EXEC sp_rename 'Offers.User_Id', 'Created_By', 'COLUMN';

-- 3. Admin'leri Agents tablosuna ekle
-- Önce mevcut Admin kullanıcılarını bul
INSERT INTO Agents (User_Id, Agent_Code, Department, Phone, Address)
SELECT 
    u.User_Id,
    'ADMIN' + CAST(u.User_Id AS VARCHAR(10)) AS Agent_Code,
    'Admin' AS Department,
    '0555-000-0000' AS Phone,
    'Admin Adresi' AS Address
FROM Users u
WHERE u.Role = 'Admin' 
AND NOT EXISTS (
    SELECT 1 FROM Agents a WHERE a.User_Id = u.User_Id
);

-- 4. Mevcut Offers kayıtlarını güncelle (NULL olan Reviewed_By alanlarını düzelt)
UPDATE Offers 
SET Reviewed_By = (
    SELECT TOP 1 a.Agent_Id 
    FROM Agents a 
    INNER JOIN Users u ON a.User_Id = u.User_Id 
    WHERE u.Role = 'Admin'
)
WHERE Reviewed_By IS NULL;

-- 5. Created_By alanını güncelle (NULL olan kayıtları düzelt)
UPDATE Offers 
SET Created_By = (
    SELECT TOP 1 u.User_Id 
    FROM Users u 
    WHERE u.Role = 'Customer'
)
WHERE Created_By IS NULL;

PRINT 'Veritabanı değişiklikleri tamamlandı!';