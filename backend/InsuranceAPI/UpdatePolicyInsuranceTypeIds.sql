-- Mevcut poliçelerin Ins_Type_Id değerlerini güncelle
-- Offer tablosundan InsuranceTypeId bilgisini alarak Policies tablosuna aktar

UPDATE p 
SET p.Ins_Type_Id = o.Insurance_Type_Id
FROM Policies p
INNER JOIN Offers o ON p.Offer_Id = o.Offer_Id
WHERE p.Ins_Type_Id IS NULL;

-- Güncellenen kayıtları kontrol et
SELECT 
    p.Policy_Id,
    p.Policy_Number,
    p.Ins_Type_Id,
    it.Name as Insurance_Type_Name,
    o.Insurance_Type_Id as Offer_Insurance_Type_Id
FROM Policies p
LEFT JOIN Offers o ON p.Offer_Id = o.Offer_Id
LEFT JOIN InsuranceTypes it ON p.Ins_Type_Id = it.Ins_Type_Id
ORDER BY p.Policy_Id;
