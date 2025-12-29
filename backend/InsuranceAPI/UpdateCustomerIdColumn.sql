-- =====================================================
-- CUSTOMERS TABLOSU ID KOLONUNU CUSTOMER_ID OLARAK GÜNCELLEME
-- =====================================================

USE [InsuranceSystem]
GO

-- 1. Önce mevcut foreign key constraint'leri kontrol et ve kaldır
PRINT '1. Foreign key constraint''ler kontrol ediliyor...'

-- Offers tablosundaki CustomerId foreign key'i kaldır
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Offers_Customers_CustomerId')
BEGIN
    ALTER TABLE [Offers] DROP CONSTRAINT [FK_Offers_Customers_CustomerId]
    PRINT '   - Offers tablosundaki CustomerId FK kaldırıldı'
END

-- Documents tablosundaki CustomerId foreign key'i kaldır (eğer varsa)
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Documents_Customers_CustomerId')
BEGIN
    ALTER TABLE [Documents] DROP CONSTRAINT [FK_Documents_Customers_CustomerId]
    PRINT '   - Documents tablosundaki CustomerId FK kaldırıldı'
END

-- 2. Mevcut Id kolonunu Customer_Id olarak yeniden adlandır
PRINT '2. Id kolonu Customer_Id olarak yeniden adlandırılıyor...'

-- Önce mevcut kolonun adını değiştir
EXEC sp_rename 'Customers.Id', 'Customer_Id', 'COLUMN'
PRINT '   - Id kolonu Customer_Id olarak yeniden adlandırıldı'

-- 3. Yeni kolon için primary key constraint ekle
PRINT '3. Primary key constraint ekleniyor...'

-- Mevcut primary key'i kaldır (eğer varsa)
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'PK_Customers' AND object_id = OBJECT_ID('Customers'))
BEGIN
    ALTER TABLE [Customers] DROP CONSTRAINT [PK_Customers]
    PRINT '   - Eski primary key kaldırıldı'
END

-- Yeni primary key ekle
ALTER TABLE [Customers] ADD CONSTRAINT [PK_Customers] PRIMARY KEY ([Customer_Id])
PRINT '   - Yeni primary key eklendi: Customer_Id'

-- 4. Foreign key constraint'leri yeniden ekle
PRINT '4. Foreign key constraint''ler yeniden ekleniyor...'

-- Offers tablosuna CustomerId foreign key ekle
ALTER TABLE [Offers] ADD CONSTRAINT [FK_Offers_Customers_CustomerId] 
    FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([Customer_Id])
PRINT '   - Offers tablosuna CustomerId FK eklendi'

-- Documents tablosuna CustomerId foreign key ekle (eğer varsa)
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Documents') AND name = 'CustomerId')
BEGIN
    ALTER TABLE [Documents] ADD CONSTRAINT [FK_Documents_Customers_CustomerId] 
        FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([Customer_Id])
    PRINT '   - Documents tablosuna CustomerId FK eklendi'
END

-- 5. Index'leri güncelle
PRINT '5. Index''ler güncelleniyor...'

-- Customer_Id üzerinde index oluştur (eğer yoksa)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Customers_CustomerId' AND object_id = OBJECT_ID('Customers'))
BEGIN
    CREATE INDEX [IX_Customers_CustomerId] ON [Customers]([Customer_Id])
    PRINT '   - Customer_Id üzerinde index oluşturuldu'
END

-- 6. Sonuçları kontrol et
PRINT '6. Güncelleme sonuçları kontrol ediliyor...'

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
PRINT 'Customers tablosunda Id kolonu Customer_Id olarak değiştirildi.'
PRINT '====================================================='
GO


