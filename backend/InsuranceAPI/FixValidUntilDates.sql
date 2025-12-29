-- Mevcut tekliflerin ValidUntil değerlerini düzelt
-- DateTime.MinValue (0001-01-01) olan teklifleri güncelle

-- Önce mevcut durumu kontrol et
SELECT 
    Offer_Id,
    Description,
    Valid_Until,
    Insurance_Type_Id,
    Created_At
FROM Offers 
WHERE Valid_Until = '0001-01-01 00:00:00.0000000'
   OR Valid_Until < '1900-01-01'
ORDER BY Offer_Id;

-- DateTime.MinValue olan teklifleri güncelle
UPDATE Offers 
SET Valid_Until = DATEADD(day, 30, Created_At)
WHERE Valid_Until = '0001-01-01 00:00:00.0000000'
   OR Valid_Until < '1900-01-01';

-- Güncelleme sonrası kontrol
SELECT 
    Offer_Id,
    Description,
    Valid_Until,
    Insurance_Type_Id,
    Created_At,
    DATEDIFF(day, Created_At, Valid_Until) as Days_Added
FROM Offers 
ORDER BY Offer_Id;
