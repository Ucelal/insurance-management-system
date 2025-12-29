-- Add Policy_Pdf_Url column to Offers table
-- Run this script to fix the missing column error

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Offers]') AND name = 'Policy_Pdf_Url')
BEGIN
    ALTER TABLE [dbo].[Offers] 
    ADD [Policy_Pdf_Url] NVARCHAR(500) NULL;
    
    PRINT 'Policy_Pdf_Url column added to Offers table successfully.';
END
ELSE
BEGIN
    PRINT 'Policy_Pdf_Url column already exists in Offers table.';
END

-- Also add Admin_Notes and Rejection_Reason columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Offers]') AND name = 'Admin_Notes')
BEGIN
    ALTER TABLE [dbo].[Offers] 
    ADD [Admin_Notes] NVARCHAR(1000) NULL;
    
    PRINT 'Admin_Notes column added to Offers table successfully.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Offers]') AND name = 'Rejection_Reason')
BEGIN
    ALTER TABLE [dbo].[Offers] 
    ADD [Rejection_Reason] NVARCHAR(1000) NULL;
    
    PRINT 'Rejection_Reason column added to Offers table successfully.';
END
