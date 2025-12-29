-- ========================================
-- YENİ ADMIN EKLEME (ADM KODU İLE)
-- ========================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

PRINT 'Yeni admin kullanıcılarını Agents tablosuna ekleme başlıyor...';

-- Admin'leri Agents tablosuna ekle (ADM kodu ile)
INSERT INTO Agents (User_Id, Agent_Code, Department, Phone, Address)
SELECT 
    u.User_Id,
    'ADM' AS Agent_Code,  -- Tüm admin'ler için ADM kodu
    'Admin' AS Department,
    '0555-000-0000' AS Phone,
    'Admin Adresi' AS Address
FROM Users u
WHERE u.Role = 'Admin' 
AND NOT EXISTS (
    SELECT 1 FROM Agents a WHERE a.User_Id = u.User_Id
);

PRINT 'Admin kullanıcıları Agents tablosuna eklendi (ADM kodu ile).';



