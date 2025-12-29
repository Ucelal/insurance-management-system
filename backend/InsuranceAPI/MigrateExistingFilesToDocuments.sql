-- Migration script to create Document records for existing uploaded files
-- This script will create Document records for files that exist in wwwroot/uploads but not in Documents table

-- First, let's check what offers have customerAdditionalInfo with file references
SELECT 
    Offer_Id,
    Customer_Id,
    Customer_Additional_Info,
    Status
FROM Offers 
WHERE Customer_Additional_Info IS NOT NULL 
  AND Customer_Additional_Info != ''
  AND Customer_Additional_Info LIKE '%uploads%';

-- If no results, we need to create Document records based on the files in wwwroot/uploads
-- For now, let's create a sample Document record for testing

-- Insert a sample document record for customer 2 (Test Müşterisi)
INSERT INTO Documents (
    Customer_Id,
    File_Name,
    File_Url,
    Uploaded_At
) VALUES (
    2, -- Customer_Id for Test Müşterisi
    'Java Kitap PDF.pdf',
    '/uploads/customer-documents/090947a4-26bf-4468-a120-1d80dc92ab18_Java Kitap PDF.pdf',
    GETDATE()
);

-- Insert another sample document record
INSERT INTO Documents (
    Customer_Id,
    File_Name,
    File_Url,
    Uploaded_At
) VALUES (
    2, -- Customer_Id for Test Müşterisi
    'HAFTA 1 PROJE ÖZETİ.pdf',
    '/uploads/customer-documents/health-reports/19c8370f-e773-44a7-8921-a52995eafebf_HAFTA 1 PROJE ÖZETİ (1).pdf',
    GETDATE()
);

-- Check the inserted records
SELECT * FROM Documents WHERE Customer_Id = 2;

PRINT 'Sample Document records created for testing';


