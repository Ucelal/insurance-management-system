-- Check what files are stored in customerAdditionalInfo for existing offers
SELECT 
    Offer_Id,
    Customer_Id,
    Customer_Additional_Info,
    Status
FROM Offers 
WHERE Customer_Additional_Info IS NOT NULL 
  AND Customer_Additional_Info != ''
  AND Customer_Additional_Info LIKE '%uploads%'
ORDER BY Offer_Id;


