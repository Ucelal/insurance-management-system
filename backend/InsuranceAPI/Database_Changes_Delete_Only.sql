-- ========================================
-- VERİTABANI DEĞİŞİKLİKLERİ (SADECE SİLME)
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

PRINT '2. Admin kullanıcılarını Agents tablosuna ekleme başlıyor...';

-- 2. Admin'leri Agents tablosuna ekle
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

PRINT 'Sütun silme işlemleri tamamlandı!';



