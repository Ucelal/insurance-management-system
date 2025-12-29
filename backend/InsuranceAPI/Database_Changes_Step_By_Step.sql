-- ========================================
-- VERİTABANI DEĞİŞİKLİKLERİ (ADIM ADIM)
-- ========================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

PRINT '1. Offers tablosundan alanları silme başlıyor...';

-- 1. Offers tablosundan alanları sil
ALTER TABLE Offers DROP COLUMN Description;
PRINT 'Description sütunu silindi.';

ALTER TABLE Offers DROP COLUMN Additional_Data;
PRINT 'Additional_Data sütunu silindi.';

ALTER TABLE Offers DROP COLUMN Approved_At;
PRINT 'Approved_At sütunu silindi.';

PRINT '2. Offers tablosunda alan isimlerini değiştirme başlıyor...';

-- 2. Offers tablosunda alan isimlerini değiştir
EXEC sp_rename 'Offers.Reviewed_By_Agent_Id', 'Reviewed_By', 'COLUMN';
PRINT 'Reviewed_By_Agent_Id -> Reviewed_By değiştirildi.';

EXEC sp_rename 'Offers.User_Id', 'Created_By', 'COLUMN';
PRINT 'User_Id -> Created_By değiştirildi.';

PRINT '3. Admin kullanıcılarını Agents tablosuna ekleme başlıyor...';

-- 3. Admin'leri Agents tablosuna ekle
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

PRINT 'Admin kullanıcıları Agents tablosuna eklendi.';

PRINT '4. Mevcut Offers kayıtlarını güncelleme başlıyor...';

-- 4. Mevcut Offers kayıtlarını güncelle (NULL olan Reviewed_By alanlarını düzelt)
UPDATE Offers 
SET Reviewed_By = (
    SELECT TOP 1 a.Agent_Id 
    FROM Agents a 
    INNER JOIN Users u ON a.User_Id = u.User_Id 
    WHERE u.Role = 'Admin'
)
WHERE Reviewed_By IS NULL;

PRINT 'Reviewed_By alanları güncellendi.';

-- 5. Created_By alanını güncelle (NULL olan kayıtları düzelt)
UPDATE Offers 
SET Created_By = (
    SELECT TOP 1 u.User_Id 
    FROM Users u 
    WHERE u.Role = 'Customer'
)
WHERE Created_By IS NULL;

PRINT 'Created_By alanları güncellendi.';

PRINT 'Tüm veritabanı değişiklikleri başarıyla tamamlandı!';



