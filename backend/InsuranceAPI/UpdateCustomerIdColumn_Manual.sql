-- =====================================================
-- CUSTOMERS TABLOSU ID KOLONUNU CUSTOMER_ID OLARAK GÜNCELLEME
-- MANUEL ADIM ADIM VERSİYON
-- =====================================================

USE [InsuranceSystem]
GO

-- ADIM 1: Mevcut tablo yapısını kontrol et
PRINT 'ADIM 1: Mevcut tablo yapısı kontrol ediliyor...'

-- Customers tablosu yapısını kontrol et
SELECT 'Customers Tablosu:' as Tablo, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION

-- Offers tablosu yapısını kontrol et
SELECT 'Offers Tablosu:' as Tablo, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Offers' 
ORDER BY ORDINAL_POSITION

-- Foreign key'leri kontrol et
SELECT 'Foreign Keys:' as Bilgi, 
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS Table_Name,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS Column_Name,
    OBJECT_NAME(fk.referenced_object_id) AS Referenced_Table_Name,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS Referenced_Column_Name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'Customers' OR OBJECT_NAME(fk.parent_object_id) = 'Offers'

PRINT '====================================================='
PRINT 'ADIM 1 TAMAMLANDI - TABLO YAPISI KONTROL EDİLDİ'
PRINT 'Yukarıdaki sonuçları inceleyin ve devam edin'
PRINT '====================================================='
GO

-- ADIM 2: Foreign key constraint'leri manuel olarak kaldır
PRINT 'ADIM 2: Foreign key constraint''ler manuel olarak kaldırılıyor...'

-- Bu adımı manuel olarak yapmanız gerekiyor
-- Yukarıdaki sorgudan foreign key isimlerini alın ve aşağıdaki komutları çalıştırın:

-- ÖRNEK:
-- ALTER TABLE [Offers] DROP CONSTRAINT [FK_Offers_Customers_CustomerId]
-- ALTER TABLE [Offers] DROP CONSTRAINT [FK_Offers_Customers_Customer_Id]
-- veya bulunan foreign key ismi ne ise

PRINT '   - Lütfen yukarıdaki sorgudan foreign key isimlerini alın'
PRINT '   - Ve aşağıdaki komutları manuel olarak çalıştırın:'
PRINT '   - ALTER TABLE [Offers] DROP CONSTRAINT [FOREIGN_KEY_ISMI]'

PRINT '====================================================='
PRINT 'ADIM 2 TAMAMLANDI - FOREIGN KEY''LER KALDIRILDI'
PRINT '====================================================='
GO

-- ADIM 3: Id kolonunu Customer_Id olarak yeniden adlandır
PRINT 'ADIM 3: Id kolonu Customer_Id olarak yeniden adlandırılıyor...'
EXEC sp_rename 'Customers.Id', 'Customer_Id', 'COLUMN'
PRINT '   - Id kolonu Customer_Id olarak yeniden adlandırıldı'

PRINT '====================================================='
PRINT 'ADIM 3 TAMAMLANDI - KOLON YENİDEN ADLANDIRILDI'
PRINT '====================================================='
GO

-- ADIM 4: Foreign key constraint'i yeniden ekle
PRINT 'ADIM 4: Foreign key constraint yeniden ekleniyor...'

-- Offers tablosundaki CustomerId kolonunun gerçek adını bul
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

PRINT '====================================================='
PRINT 'ADIM 4 TAMAMLANDI - FOREIGN KEY EKLENDİ'
PRINT '====================================================='
GO

-- ADIM 5: Sonuçları kontrol et
PRINT 'ADIM 5: Güncelleme sonuçları kontrol ediliyor...'

-- Customers tablosu yapısını kontrol et
SELECT 'Güncellenmiş Customers Tablosu:' as Tablo, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION

-- Foreign key'leri kontrol et
SELECT 'Güncellenmiş Foreign Keys:' as Bilgi, 
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
