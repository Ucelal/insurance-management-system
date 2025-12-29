-- =====================================================
-- KALAN FOREIGN KEY SORUNLARINI DÜZELTME
-- =====================================================

USE [InsuranceSystem]
GO

PRINT '====================================================='
PRINT 'KALAN FOREIGN KEY SORUNLARI DÜZELTİLİYOR'
PRINT '====================================================='
GO

-- 1. Önce mevcut tablo yapısını kontrol et
PRINT '1. MEVCUT TABLO YAPISI KONTROL EDİLİYOR...'

-- Claims tablosu
PRINT ''
PRINT 'CLAIMS TABLOSU:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Claims' 
ORDER BY ORDINAL_POSITION

-- Policies tablosu
PRINT ''
PRINT 'POLICIES TABLOSU:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Policies' 
ORDER BY ORDINAL_POSITION

-- Offers tablosu
PRINT ''
PRINT 'OFFERS TABLOSU:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Offers' 
ORDER BY ORDINAL_POSITION

-- SelectedCoverages tablosu
PRINT ''
PRINT 'SELECTEDCOVERAGES TABLOSU:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SelectedCoverages' 
ORDER BY ORDINAL_POSITION

-- Documents tablosu
PRINT ''
PRINT 'DOCUMENTS TABLOSU:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Documents' 
ORDER BY ORDINAL_POSITION

PRINT '====================================================='
PRINT 'TABLO YAPISI KONTROL EDİLDİ'
PRINT 'Yukarıdaki sonuçları inceleyin ve devam edin'
PRINT '====================================================='
GO

-- 2. Eksik foreign key'leri ekle (kolon isimlerini kontrol ederek)
PRINT ''
PRINT '2. EKSİK FOREIGN KEY''LER EKLENİYOR...'

-- Claims tablosundaki Agent_Id kolonunu kontrol et ve ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Claims' AND COLUMN_NAME = 'Agent_Id')
BEGIN
    -- Claims tablosuna Agent_Id kolonu ekle
    ALTER TABLE [Claims] ADD [Agent_Id] INT NULL
    PRINT '   - Claims tablosuna Agent_Id kolonu eklendi'
END

-- Claims tablosundaki Policy_Id kolonunu kontrol et ve ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Claims' AND COLUMN_NAME = 'Policy_Id')
BEGIN
    -- Claims tablosuna Policy_Id kolonu ekle
    ALTER TABLE [Claims] ADD [Policy_Id] INT NULL
    PRINT '   - Claims tablosuna Policy_Id kolonu eklendi'
END

-- Policies tablosundaki Customer_Id kolonunu kontrol et ve ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Policies' AND COLUMN_NAME = 'Customer_Id')
BEGIN
    -- Policies tablosuna Customer_Id kolonu ekle
    ALTER TABLE [Policies] ADD [Customer_Id] INT NULL
    PRINT '   - Policies tablosuna Customer_Id kolonu eklendi'
END

-- SelectedCoverages tablosundaki Coverage_Id kolonunu kontrol et ve ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SelectedCoverages' AND COLUMN_NAME = 'Coverage_Id')
BEGIN
    -- SelectedCoverages tablosuna Coverage_Id kolonu ekle
    ALTER TABLE [SelectedCoverages] ADD [Coverage_Id] INT NULL
    PRINT '   - SelectedCoverages tablosuna Coverage_Id kolonu eklendi'
END

-- SelectedCoverages tablosundaki Offer_Id kolonunu kontrol et ve ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'SelectedCoverages' AND COLUMN_NAME = 'Offer_Id')
BEGIN
    -- SelectedCoverages tablosuna Offer_Id kolonu ekle
    ALTER TABLE [SelectedCoverages] ADD [Offer_Id] INT NULL
    PRINT '   - SelectedCoverages tablosuna Offer_Id kolonu eklendi'
END

PRINT '   - Eksik kolonlar eklendi'
GO

-- 3. Foreign key'leri yeniden ekle
PRINT ''
PRINT '3. FOREIGN KEY''LER YENİDEN EKLENİYOR...'

-- Claims tablosundaki foreign key'ler
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Claims_Agents_AgentId')
BEGIN
    ALTER TABLE [Claims] ADD CONSTRAINT [FK_Claims_Agents_AgentId] 
        FOREIGN KEY ([Agent_Id]) REFERENCES [Agents]([Agent_Id])
    PRINT '   - Claims tablosuna FK_Claims_Agents_AgentId eklendi'
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Claims_Policies_PolicyId')
BEGIN
    ALTER TABLE [Claims] ADD CONSTRAINT [FK_Claims_Policies_PolicyId] 
        FOREIGN KEY ([Policy_Id]) REFERENCES [Policies]([Policy_Id])
    PRINT '   - Claims tablosuna FK_Claims_Policies_PolicyId eklendi'
END

-- Policies tablosundaki foreign key'ler
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Policies_Customers_CustomerId')
BEGIN
    ALTER TABLE [Policies] ADD CONSTRAINT [FK_Policies_Customers_CustomerId] 
        FOREIGN KEY ([Customer_Id]) REFERENCES [Customers]([Customer_Id])
    PRINT '   - Policies tablosuna FK_Policies_Customers_CustomerId eklendi'
END

-- SelectedCoverages tablosundaki foreign key'ler
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SelectedCoverages_Coverages_CoverageId')
BEGIN
    ALTER TABLE [SelectedCoverages] ADD CONSTRAINT [FK_SelectedCoverages_Coverages_CoverageId] 
        FOREIGN KEY ([Coverage_Id]) REFERENCES [Coverages]([Coverage_Id])
    PRINT '   - SelectedCoverages tablosuna FK_SelectedCoverages_Coverages_CoverageId eklendi'
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SelectedCoverages_Offers_OfferId')
BEGIN
    ALTER TABLE [SelectedCoverages] ADD CONSTRAINT [FK_SelectedCoverages_Offers_OfferId] 
        FOREIGN KEY ([Offer_Id]) REFERENCES [Offers]([Offer_Id])
    PRINT '   - SelectedCoverages tablosuna FK_SelectedCoverages_Offers_OfferId eklendi'
END

PRINT '   - Tüm foreign key''ler eklendi'
GO

-- 4. Sonuçları kontrol et
PRINT ''
PRINT '4. GÜNCELLEME SONUÇLARI KONTROL EDİLİYOR...'

-- Güncellenmiş primary key'leri kontrol et
SELECT 
    t.TABLE_NAME,
    pk.CONSTRAINT_NAME AS PrimaryKeyName,
    c.COLUMN_NAME AS PrimaryKeyColumn
FROM INFORMATION_SCHEMA.TABLES t
INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk 
    ON t.TABLE_NAME = pk.TABLE_NAME 
    AND pk.CONSTRAINT_TYPE = 'PRIMARY KEY'
INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE c 
    ON pk.CONSTRAINT_NAME = c.CONSTRAINT_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
ORDER BY t.TABLE_NAME

-- Güncellenmiş foreign key'leri kontrol et
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumnName
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY TableName, ColumnName

PRINT '====================================================='
PRINT 'FOREIGN KEY DÜZELTMELERİ TAMAMLANDI!'
PRINT '====================================================='
GO
