-- =====================================================
-- CUSTOMERS TABLOSU ID KOLONUNU CUSTOMER_ID OLARAK GÜNCELLEME
-- DÜZELTİLMİŞ VERSİYON
-- =====================================================

USE [InsuranceSystem]
GO

-- 0. Önce mevcut tablo yapısını kontrol et
PRINT '0. Mevcut tablo yapısı kontrol ediliyor...'

-- Customers tablosu yapısını kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION

-- Offers tablosu yapısını kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Offers' 
ORDER BY ORDINAL_POSITION

-- Foreign key'leri kontrol et
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table_Name,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column_Name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'Customers' OR OBJECT_NAME(fk.parent_object_id) = 'Offers'

PRINT '====================================================='
PRINT 'TABLO YAPISI KONTROL EDİLDİ'
PRINT 'Yukarıdaki sonuçları inceleyin ve devam edin'
PRINT '====================================================='
GO

-- 1. Foreign key constraint'leri kaldır
PRINT '1. Foreign key constraint''ler kaldırılıyor...'

-- Offers tablosundaki CustomerId foreign key'i kaldır (tüm olası isimleri dene)
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Offers_Customers_CustomerId')
BEGIN
    ALTER TABLE [Offers] DROP CONSTRAINT [FK_Offers_Customers_CustomerId]
    PRINT '   - Offers tablosundaki FK_Offers_Customers_CustomerId kaldırıldı'
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Offers_Customers_Customer_Id')
BEGIN
    ALTER TABLE [Offers] DROP CONSTRAINT [FK_Offers_Customers_Customer_Id]
    PRINT '   - Offers tablosundaki FK_Offers_Customers_Customer_Id kaldırıldı'
END

-- Diğer olası foreign key isimlerini de kontrol et
DECLARE @fkName NVARCHAR(128)
SELECT @fkName = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) = 'Offers' 
  AND OBJECT_NAME(fk.referenced_object_id) = 'Customers'

IF @fkName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [Offers] DROP CONSTRAINT [' + @fkName + ']')
    PRINT '   - Offers tablosundaki ' + @fkName + ' kaldırıldı'
END

-- 2. Id kolonunu Customer_Id olarak yeniden adlandır
PRINT '2. Id kolonu Customer_Id olarak yeniden adlandırılıyor...'
EXEC sp_rename 'Customers.Id', 'Customer_Id', 'COLUMN'
PRINT '   - Id kolonu Customer_Id olarak yeniden adlandırıldı'

-- 3. Foreign key constraint'i yeniden ekle
PRINT '3. Foreign key constraint yeniden ekleniyor...'

-- Önce Offers tablosundaki CustomerId kolonunun gerçek adını bul
DECLARE @customerIdColumn NVARCHAR(128)
SELECT @customerIdColumn = COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Offers' 
  AND (COLUMN_NAME = 'CustomerId' OR COLUMN_NAME = 'Customer_Id')

IF @customerIdColumn IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [Offers] ADD CONSTRAINT [FK_Offers_Customers_CustomerId] 
        FOREIGN KEY ([' + @customerIdColumn + ']) REFERENCES [Customers]([Customer_Id])')
    PRINT '   - Offers tablosuna CustomerId FK eklendi (' + @customerIdColumn + ' -> Customer_Id)'
END
ELSE
BEGIN
    PRINT '   - UYARI: Offers tablosunda CustomerId kolonu bulunamadı!'
    PRINT '   - Lütfen kolon adını manuel olarak kontrol edin'
END

-- 4. Sonuçları kontrol et
PRINT '4. Güncelleme sonuçları kontrol ediliyor...'

-- Customers tablosu yapısını kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION

-- Foreign key'leri kontrol et
SELECT 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table_Name,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column_Name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'Customers'

PRINT '====================================================='
PRINT 'GÜNCELLEME TAMAMLANDI!'
PRINT '====================================================='
GO
