-- UpdateOfferAgentId.sql
-- Bu script Offers tablosundaki Agent_Id kolonunu nullable yapar

USE InsuranceSystem;
GO

-- Agent_Id kolonunu nullable yap
ALTER TABLE Offers 
ALTER COLUMN Agent_Id INT NULL;
GO

-- Mevcut 0 değerlerini NULL yap (opsiyonel)
UPDATE Offers 
SET Agent_Id = NULL 
WHERE Agent_Id = 0;
GO

PRINT 'Offers tablosundaki Agent_Id kolonu başarıyla nullable yapıldı.';
GO



